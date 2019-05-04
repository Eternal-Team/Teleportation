using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BaseLibrary.Tiles.TileEntites;
using BaseLibrary.UI;
using ContainerLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Teleportation.Tiles;
using Teleportation.UI;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Teleportation.TileEntities
{
	public class TETeleporter : BaseTE, IHasUI, IItemHandler
	{
		public override Type TileType => typeof(Teleporter);

		private Rectangle Hitbox => new Rectangle(Position.X * 16 + 8, Position.Y * 16 - 8, 32, 8);

		public LegacySoundStyle CloseSound => SoundID.Item1;

		public LegacySoundStyle OpenSound => SoundID.Item1;

		public new Guid ID { get; set; }

		public BaseUIPanel UI { get; set; }

		public ItemHandler Handler { get; }

		public bool[] Whitelist = { true, false, false, false };
		public bool dialOnce;

		public Ref<string> DisplayName = new Ref<string>("Teleporter");
		public TileEntity Destination;

		public Texture2D EntityTexture;
		public DrawAnimation EntityAnimation;

		public TETeleporter()
		{
			ID = Guid.NewGuid();

			Handler = new ItemHandler();
			Handler.OnContentsChanged += slot => { };
			Handler.IsItemValid += (slot, item) => item.type == mod.ItemType<FuelCell>();
		}

		public override void OnPlace()
		{
			EntityTexture = Main.itemTexture[mod.ItemType<Items.Teleporter>()];

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
			if (Destination == null) return;

			bool teleported = false;

			if (Whitelist[0])
			{
				List<Player> players = Main.player.Where(player => player.getRect().Intersects(Hitbox)).ToList();

				if (players.Any())
				{
					foreach (Player player in players)
					{
						player.Teleport(Destination.Position.ToWorldCoordinates(0, 0) + new Vector2(24 - player.width * 0.5f, -player.height));
						NetMessage.SendData(MessageID.Teleport, -1, -1, null, 0, player.whoAmI, player.position.X, player.position.Y);

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
					foreach (NPC npc in npcs) npc.Teleport(Destination.Position.ToWorldCoordinates(0, 0) + new Vector2(24 - npc.width * 0.5f, -npc.height));

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
						item.position = Destination.Position.ToWorldCoordinates(0, 0) + new Vector2(24 - item.width * 0.5f, -item.height);
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
						projectile.position = Destination.Position.ToWorldCoordinates(0, 0) + new Vector2(24 - projectile.width * 0.5f, -projectile.height);
						NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, projectile.whoAmI);
					}

					teleported = true;
				}
			}

			if (dialOnce && teleported)
			{
				Destination = null;
				dialOnce = false;
			}
		}

		public override TagCompound Save()
		{
			Directory.CreateDirectory($"{Main.SavePath}/Worlds/{Main.worldName}");
			using (FileStream stream = new FileStream($"{Main.SavePath}/Worlds/{Main.worldName}/{ID}.png", FileMode.Create))
			{
				BinaryWriter writer = new BinaryWriter(stream);
				writer.Write(EntityAnimation?.TicksPerFrame ?? -1);
				writer.Write(EntityAnimation?.FrameCount ?? -1);

				EntityTexture.SaveAsPng(stream, EntityTexture.Width, EntityTexture.Height);

				writer.Close();
			}

			return new TagCompound
			{
				["DisplayName"] = DisplayName.Value,
				["Whitelist"] = Whitelist.ToList(),
				["Destination"] = Destination?.Position ?? Point16.NegativeOne,
				["ID"] = ID.ToString(),
				["Items"] = Handler.Save()
			};
		}

		public override void Load(TagCompound tag)
		{
			DisplayName.Value = tag.GetString("DisplayName");
			Whitelist = tag.GetList<bool>("Whitelist").ToArray();

			ByPosition.TryGetValue(tag.Get<Point16>("Destination"), out Destination);

			ID = Guid.Parse(tag.GetString("ID"));
			Handler.Load(tag.GetCompound("Items"));

			string path = $"{Main.SavePath}/Worlds/{Main.worldName}/{ID}.png";
			if (File.Exists(path))
			{
				using (FileStream stream = new FileStream(path, FileMode.Open))
				{
					using (BinaryReader reader = new BinaryReader(stream))
					{
						int ticksPerFrame = reader.ReadInt32();
						int frameCount = reader.ReadInt32();
						if (ticksPerFrame != -1 && frameCount != -1) EntityAnimation = new DrawAnimationVertical(ticksPerFrame, frameCount);

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

		public override void OnKill() => Handler.DropItems(new Rectangle(Position.X * 16, Position.Y * 16, 48, 16));
	}
}