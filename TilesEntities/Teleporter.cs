using BaseLibrary;
using BaseLibrary.Tiles.TileEntites;
using BaseLibrary.UI;
using ContainerLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Teleportation.Items;
using Teleportation.UI;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Teleportation.TileEntities
{
	public class Teleporter : BaseTE, IHasUI, IItemHandler
	{
		public override Type TileType => typeof(Tiles.Teleporter);

		private Rectangle Hitbox => new Rectangle(Position.X * 16 + 8, Position.Y * 16 - 8, 32, 8);

		public LegacySoundStyle CloseSound => SoundID.Item1;

		public LegacySoundStyle OpenSound => SoundID.Item1;

		public Guid UUID { get; set; }

		public BaseUIPanel UI { get; set; }

		public ItemHandler Handler { get; }

		public Ref<string> DisplayName = new Ref<string>("Teleporter");
		public Point16 Destination;
		public bool[] Whitelist = {true, false, false, false};
		public bool DialOnce;

		public Texture2D EntityTexture;
		public DrawAnimation EntityAnimation;

		// todo: maybe assign each player a unique ID
		public List<string> WhitelistPlayers = new List<string>();

		public Teleporter()
		{
			UUID = Guid.NewGuid();

			Handler = new ItemHandler();
			Handler.OnContentsChanged += slot => Net.SendTeleporterItems(this);
			Handler.IsItemValid += (slot, item) => item.type == mod.ItemType<FuelCell>();
		}

		public override void OnPlace()
		{
			EntityTexture = Main.itemTexture[mod.ItemType<Items.Teleporter>()];
			EntityAnimation = new DrawAnimationVertical(0, 1);

			if (Main.netMode != NetmodeID.Server)
			{
				foreach (TeleporterPanel teleporterPanel in BaseLibrary.BaseLibrary.PanelGUI.UI.Elements.OfType<TeleporterPanel>())
				{
					teleporterPanel.PopulateGrid();
				}
			}
		}

		public override void Update()
		{
			if (Destination == Point16.NegativeOne) return;

			bool teleported = false;

			if (Whitelist[0])
			{
				// todo: use less linq

				List<Player> players = Main.player.Where(player => player.getRect().Intersects(Hitbox)).ToList();

				if (players.Any())
				{
					foreach (Player player in players)
					{
						player.Teleport(Destination.ToWorldCoordinates(0, 0) + new Vector2(24 - player.width * 0.5f, -player.height));
						NetMessage.SendData(MessageID.Teleport, -1, -1, null, 0, player.whoAmI, player.position.X, player.position.Y);

						// todo: gets run on server, need to send a message to close the UIs
						if (player == Main.LocalPlayer && UI != null) BaseLibrary.BaseLibrary.PanelGUI.UI.CloseUI(this);
					}

					teleported = true;
				}
			}

			if (Whitelist[1])
			{
				List<NPC> npcs = Main.npc.Where(npc => npc.getRect().Intersects(Hitbox)).ToList();

				if (npcs.Any())
				{
					foreach (NPC npc in npcs) npc.Teleport(Destination.ToWorldCoordinates(0, 0) + new Vector2(24 - npc.width * 0.5f, -npc.height));

					teleported = true;
				}
			}

			if (Whitelist[2])
			{
				List<Item> items = Main.item.Where(item => item.getRect().Intersects(Hitbox)).ToList();

				if (items.Any())
				{
					foreach (Item item in items)
					{
						item.position = Destination.ToWorldCoordinates(0, 0) + new Vector2(24 - item.width * 0.5f, -item.height);
						NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item.whoAmI);
					}

					teleported = true;
				}
			}

			if (Whitelist[3])
			{
				List<Projectile> projectiles = Main.projectile.Where(projectile => projectile.getRect().Intersects(Hitbox)).ToList();

				if (projectiles.Any())
				{
					foreach (Projectile projectile in projectiles)
					{
						projectile.position = Destination.ToWorldCoordinates(0, 0) + new Vector2(24 - projectile.width * 0.5f, -projectile.height);
						NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, projectile.whoAmI);
					}

					teleported = true;
				}
			}

			if (DialOnce && teleported)
			{
				Destination = Point16.NegativeOne;
				DialOnce = false;
				Net.SendTeleporterDestination(this);
			}
		}

		private string IconPath => $"{Main.SavePath}/Worlds/{Main.worldName}/{UUID}.teleporterIcon";

		// todo: could store as two files - texture and metadata
		// todo: or figure out how to store metadata on the png file
		// todo: would allows users to set custom icon

		public override TagCompound Save()
		{
			Directory.CreateDirectory($"{Main.SavePath}/Worlds/{Main.worldName}");
			using (FileStream stream = new FileStream(IconPath, FileMode.Create))
			{
				BinaryWriter writer = new BinaryWriter(stream);
				writer.Write(EntityAnimation.TicksPerFrame);
				writer.Write(EntityAnimation.FrameCount);

				EntityTexture.SaveAsPng(writer.BaseStream, EntityTexture.Width, EntityTexture.Height);
				writer.Close();
			}

			return new TagCompound
			{
				["DisplayName"] = DisplayName.Value,
				["Whitelist"] = Whitelist.ToList(),
				["Destination"] = Destination,
				["UUID"] = UUID,
				["Items"] = Handler.Save()
			};
		}

		public override void Load(TagCompound tag)
		{
			DisplayName.Value = tag.GetString("DisplayName");
			Whitelist = tag.GetList<bool>("Whitelist").ToArray();

			Destination = tag.Get<Point16>("Destination");

			UUID = tag.Get<Guid>("UUID");
			Handler.Load(tag.GetCompound("Items"));

			if (File.Exists(IconPath))
			{
				using (FileStream stream = new FileStream(IconPath, FileMode.Open))
				{
					using (BinaryReader reader = new BinaryReader(stream))
					{
						EntityAnimation = new DrawAnimationVertical(reader.ReadInt32(), reader.ReadInt32());

						using (MemoryStream memoryStream = new MemoryStream())
						{
							stream.CopyTo(memoryStream);

							EntityTexture = Texture2D.FromStream(Main.graphics.GraphicsDevice, memoryStream);
						}
					}
				}
			}
			else EntityTexture = Main.itemTexture[mod.ItemType<Items.Teleporter>()];
		}

		public override void NetSend(BinaryWriter writer, bool lightSend)
		{
			Handler.Write(writer);

			writer.Write(UUID);
			writer.Write(Destination);
			for (int i = 0; i < 4; i++) writer.Write(Whitelist[i]);

			writer.Write(EntityAnimation.TicksPerFrame);
			writer.Write(EntityAnimation.FrameCount);

			EntityTexture.SaveAsPng(writer.BaseStream, EntityTexture.Width, EntityTexture.Height);

			writer.Write(Name);
			writer.Write(DialOnce);
		}

		public override void NetReceive(BinaryReader reader, bool lightReceive)
		{
			Handler.Read(reader);

			UUID = reader.ReadGUID();
			Destination = reader.ReadPoint16();
			for (int i = 0; i < 4; i++) Whitelist[i] = reader.ReadBoolean();

			EntityAnimation = new DrawAnimationVertical(reader.ReadInt32(), reader.ReadInt32());
			EntityTexture = Texture2D.FromStream(Main.graphics.GraphicsDevice, reader.BaseStream);

			DisplayName.Value = reader.ReadString();
			DialOnce = reader.ReadBoolean();
		}

		public override void OnKill()
		{
			if (File.Exists(IconPath)) File.Delete(IconPath);

			Handler.DropItems(new Rectangle(Position.X * 16, Position.Y * 16, 48, 16));
		}
	}
}