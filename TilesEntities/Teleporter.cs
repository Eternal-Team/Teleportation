using BaseLibrary;
using BaseLibrary.Tiles.TileEntites;
using BaseLibrary.UI;
using ContainerLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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
	// note: multiple inbounds connections, can't connect to them?

	public class Teleporter : BaseTE, IHasUI, IItemHandler
	{
		public override Type TileType => typeof(Tiles.Teleporter);

		private Rectangle Hitbox => new Rectangle(Position.X * 16 + 8, Position.Y * 16 - 8, 32, 8);

		public LegacySoundStyle CloseSound => SoundID.Item1;

		public LegacySoundStyle OpenSound => SoundID.Item1;

		public Guid UUID { get; set; }

		public BaseUIPanel UI { get; set; }

		public ItemHandler Handler { get; }

		public BaseLibrary.Ref<string> DisplayName = new BaseLibrary.Ref<string>("Teleporter");
		public Point16 Destination = Point16.NegativeOne;
		public bool[] Whitelist = {true, false, false, false};
		public bool DialOnce;

		public Texture2D EntityTexture;
		public DrawAnimation EntityAnimation;

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
			if (Destination == Point16.NegativeOne || Handler.Items[0].IsAir) return;

			bool teleported = false;

			if (Whitelist[0])
			{
				foreach (Player player in Main.player)
				{
					if (player != null && player.active && !player.dead && player.getRect().Intersects(Hitbox))
					{
						Handler.Shrink(0, 1);

						player.Teleport(Destination.ToWorldCoordinates(24 - player.width * 0.5f, -player.height));
						NetMessage.SendData(MessageID.Teleport, -1, -1, null, 0, player.whoAmI, player.position.X, player.position.Y);

						// todo: gets run on server, need to send a message to close the UI

						teleported = true;
					}
				}
			}

			if (Whitelist[1])
			{
				foreach (NPC npc in Main.npc)
				{
					if (npc != null && npc.active && npc.getRect().Intersects(Hitbox))
					{
						if (Main.rand.NextBool(10)) Handler.Shrink(0, 1);

						npc.Teleport(Destination.ToWorldCoordinates(24 - npc.width * 0.5f, -npc.height));

						teleported = true;
					}
				}
			}

			if (Whitelist[2])
			{
				foreach (Item item in Main.item)
				{
					if (item != null && item.active && item.getRect().Intersects(Hitbox))
					{
						if (Main.rand.NextBool(10)) Handler.Shrink(0, 1);

						item.position = Destination.ToWorldCoordinates(24 - item.width * 0.5f, -item.height);
						NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item.whoAmI);

						teleported = true;
					}
				}
			}

			if (Whitelist[3])
			{
				foreach (Projectile projectile in Main.projectile)
				{
					if (projectile != null && projectile.active && projectile.getRect().Intersects(Hitbox))
					{
						if (Main.rand.NextBool(10)) Handler.Shrink(0, 1);

						projectile.position = Destination.ToWorldCoordinates(24 - projectile.width * 0.5f, -projectile.height);
						NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, projectile.whoAmI);

						teleported = true;
					}
				}
			}

			if (DialOnce && teleported)
			{
				Destination = Point16.NegativeOne;
				DialOnce = false;
				Net.SendTeleporterDestination(this);
			}
		}

		// todo: should also append position/name to the file name so people can find it easily
		private string IconPath => $"{Main.SavePath}/Worlds/{Main.worldName}/{UUID}.png";
		private string MetadataPath => $"{Main.SavePath}/Worlds/{Main.worldName}/{UUID}.txt";

		public override TagCompound Save()
		{
			Directory.CreateDirectory($"{Main.SavePath}/Worlds/{Main.worldName}");

			using (FileStream st = File.Open(IconPath, FileMode.Create)) EntityTexture.SaveAsPng(st, EntityTexture.Width, EntityTexture.Height);
			File.WriteAllText(MetadataPath, $"{EntityAnimation.TicksPerFrame};{EntityAnimation.FrameCount}");

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
			UUID = tag.Get<Guid>("UUID");

			if (File.Exists(IconPath))
			{
				using (FileStream stream = File.Open(IconPath, FileMode.Open)) EntityTexture = Texture2D.FromStream(Main.graphics.GraphicsDevice, stream);
			}
			else EntityTexture = Main.itemTexture[mod.ItemType<Items.Teleporter>()];

			if (File.Exists(MetadataPath))
			{
				string[] text = File.ReadAllText(MetadataPath).Split(';');
				if (int.TryParse(text[0], out int ticksPerFrame) && int.TryParse(text[1], out int frameCount)) EntityAnimation = new DrawAnimationVertical(ticksPerFrame, frameCount);
			}
			else EntityAnimation = new DrawAnimationVertical(0, 1);

			DisplayName.Value = tag.GetString("DisplayName");
			Whitelist = tag.GetList<bool>("Whitelist").ToArray();

			Destination = tag.Get<Point16>("Destination");

			Handler.Load(tag.GetCompound("Items"));
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