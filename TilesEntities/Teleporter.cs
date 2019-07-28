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
	// note: apperently teleporting NPCs should be more expensive

	// note: should you be able to terminate connection from both dialer and destination?
	// note: multiple inbounds connections, can't connect to them

	public abstract class Teleporter : BaseTE, IHasUI, IItemHandler
	{
		protected virtual Rectangle Hitbox => new Rectangle(Position.X * 16 + 8, Position.Y * 16 - 8, 32, 8);

		public LegacySoundStyle CloseSound => SoundID.Item1;

		public LegacySoundStyle OpenSound => SoundID.Item1;

		public Guid UUID { get; set; }

		public BaseUIPanel UI { get; set; }

		public ItemHandler Handler { get; }

		private Texture2D _entityTexture;

		public BaseLibrary.Ref<string> DisplayName;

		private Point16 _destination;
		public Teleporter Destination
		{
			get
			{
				if (ByPosition.TryGetValue(_destination, out TileEntity te)) return (Teleporter)te;
				return null;
			}
			set => _destination = value?.Position ?? Point16.NegativeOne;
		}

		public bool[] Whitelist;
		public bool DialOnce;
		public DrawAnimation EntityAnimation;

		public Texture2D EntityTexture
		{
			get => _entityTexture ?? (_entityTexture = Main.itemTexture[mod.ItemType(TileType.Name)]);
			set => _entityTexture = value;
		}

		public Teleporter()
		{
			UUID = Guid.NewGuid();

			DisplayName = new BaseLibrary.Ref<string>("Teleporter");
			Destination = null;
			Whitelist = new[] { true, false, false, false, false };

			Handler = new ItemHandler();
			Handler.OnContentsChanged += slot => Net.SendTeleporterItems(this);
			Handler.IsItemValid += (slot, item) => item.type == mod.ItemType<FuelCell>();
		}

		public override void OnPlace()
		{
			EntityAnimation = new DrawAnimationVertical(0, 1);

			if (Main.netMode != NetmodeID.Server)
			{
				foreach (TeleporterPanel teleporterPanel in BaseLibrary.BaseLibrary.PanelGUI.UI.Elements.OfType<TeleporterPanel>()) teleporterPanel.PopulateGrid();
			}
		}

		public override void Update()
		{
			if (Destination == null || Handler.Items[0].IsAir) return;

			bool teleported = false;

			if (Whitelist[0])
			{
				foreach (Player player in Main.player)
				{
					if (player != null && player.active && !player.dead && player.getRect().Intersects(Hitbox))
					{
						if (!ConsumeFuel(0)) break;

						player.Teleport(new Vector2(Destination.Hitbox.Center.X - player.width * 0.5f, Destination.Hitbox.Bottom - player.height));
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
					if (npc != null && npc.active && !npc.boss && npc.getRect().Intersects(Hitbox))
					{
						if (!ConsumeFuel(1)) break;

						npc.Teleport(new Vector2(Destination.Hitbox.Center.X - npc.width * 0.5f, Destination.Hitbox.Bottom - npc.height));

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
						if (!ConsumeFuel(2)) break;

						item.position = new Vector2(Destination.Hitbox.Center.X - item.width * 0.5f, Destination.Hitbox.Bottom - item.height);
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
						if (!ConsumeFuel(3)) break;

						projectile.position = new Vector2(Destination.Hitbox.Center.X - projectile.width * 0.5f, Destination.Hitbox.Bottom - projectile.height);
						NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, projectile.whoAmI);

						teleported = true;
					}
				}
			}

			if (Whitelist[4])
			{
				foreach (NPC npc in Main.npc)
				{
					if (npc != null && npc.active && npc.boss && npc.getRect().Intersects(Hitbox))
					{
						if (!ConsumeFuel(4)) break;

						npc.Teleport(new Vector2(Destination.Hitbox.Center.X - npc.width * 0.5f, Destination.Hitbox.Bottom- npc.height));

						teleported = true;
					}
				}
			}

			if (DialOnce && teleported)
			{
				Destination = null;
				DialOnce = false;
				Net.SendTeleporterDestination(this);
			}
		}

		private string IconPath => $"{Main.SavePath}/Worlds/{Main.worldName}/{DisplayName.Value} {UUID}.png";
		private string MetadataPath => $"{Main.SavePath}/Worlds/{Main.worldName}/{DisplayName.Value} {UUID}.txt";

		public override TagCompound Save()
		{
			Directory.CreateDirectory($"{Main.SavePath}/Worlds/{Main.worldName}");

			using (FileStream stream = File.Open(IconPath, FileMode.Create)) EntityTexture.SaveAsPng(stream, EntityTexture.Width, EntityTexture.Height);
			File.WriteAllText(MetadataPath, $"{EntityAnimation.TicksPerFrame};{EntityAnimation.FrameCount}");

			return new TagCompound
			{
				["DisplayName"] = DisplayName.Value,
				["Whitelist"] = Whitelist.ToList(),
				["Destination"] = _destination,
				["UUID"] = UUID,
				["Items"] = Handler.Save()
			};
		}

		public override void Load(TagCompound tag)
		{
			UUID = tag.Get<Guid>("UUID");
			DisplayName.Value = tag.GetString("DisplayName");
			Whitelist = tag.GetList<bool>("Whitelist").ToArray();
			_destination = tag.Get<Point16>("Destination");
			Handler.Load(tag.GetCompound("Items"));

			if (File.Exists(IconPath))
			{
				// bug: won't work on server because graphicsdevice is null, what I will have to do is distribute the files to clients
				using (FileStream stream = File.Open(IconPath, FileMode.Open)) EntityTexture = Texture2D.FromStream(Main.graphics.GraphicsDevice, stream);
			}

			if (File.Exists(MetadataPath))
			{
				string[] text = File.ReadAllText(MetadataPath).Split(';');
				if (int.TryParse(text[0], out int ticksPerFrame) && int.TryParse(text[1], out int frameCount)) EntityAnimation = new DrawAnimationVertical(ticksPerFrame, frameCount);
			}
			else EntityAnimation = new DrawAnimationVertical(0, 1);
		}

		public override void NetSend(BinaryWriter writer, bool lightSend)
		{
			Handler.Write(writer);

			writer.Write(UUID);
			writer.Write(_destination);
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
			_destination = reader.ReadPoint16();
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

		public abstract bool ConsumeFuel(int type);

		public abstract IEnumerable<Teleporter> GetConnections();
	}

	public class BasicTeleporter : Teleporter
	{
		public override Type TileType => typeof(Tiles.BasicTeleporter);

		public override bool ConsumeFuel(int type) => Handler.Shrink(0, type > 0 ? 1 : 5);

		public override IEnumerable<Teleporter> GetConnections()
		{
			foreach (TileEntity tileEntity in ByID.Values)
			{
				if (tileEntity is BasicTeleporter teleporter && teleporter != this && Vector2.DistanceSquared(Position.ToWorldCoordinates(24), teleporter.Position.ToWorldCoordinates(24)) <= 128 * 128 * 256) yield return teleporter;
			}
		}
	}

	public class AdvancedTeleporter : Teleporter
	{
		public override Type TileType => typeof(Tiles.AdvancedTeleporter);

		public override bool ConsumeFuel(int type)
		{
			if (type == 0) return Handler.Shrink(0, 3);
			return !Main.rand.NextBool(5) || Handler.Shrink(0, 1);
		}

		public override IEnumerable<Teleporter> GetConnections()
		{
			foreach (TileEntity tileEntity in ByID.Values)
			{
				if (tileEntity is AdvancedTeleporter teleporter && teleporter != this && Vector2.DistanceSquared(Position.ToWorldCoordinates(24), teleporter.Position.ToWorldCoordinates(24)) <= 1024 * 1024 * 256) yield return teleporter;
			}
		}
	}

	public class EliteTeleporter : Teleporter
	{
		public override Type TileType => typeof(Tiles.EliteTeleporter);

		public override bool ConsumeFuel(int type)
		{
			if (type == 0) return Handler.Shrink(0, 1);
			return !Main.rand.NextBool(10) || Handler.Shrink(0, 1);
		}

		public override IEnumerable<Teleporter> GetConnections()
		{
			foreach (TileEntity tileEntity in ByID.Values)
			{
				if (tileEntity is EliteTeleporter teleporter && teleporter != this) yield return teleporter;
			}
		}
	}

	public class UltimateTeleporter : Teleporter
	{
		public override Type TileType => typeof(Tiles.UltimateTeleporter);

		protected override Rectangle Hitbox => new Rectangle(Position.X * 16 + 8, Position.Y * 16 - 8, 96, 8);
		
		public override bool ConsumeFuel(int type)
		{
			if (type == 0) return !Main.rand.NextBool(3) || Handler.Shrink(0, 1);
			if (type == 4) return Handler.Shrink(0, 10);
			return !Main.rand.NextBool(25) || Handler.Shrink(0, 1);
		}

		public override IEnumerable<Teleporter> GetConnections()
		{
			foreach (TileEntity tileEntity in ByID.Values)
			{
				if (tileEntity is UltimateTeleporter teleporter && teleporter != this) yield return teleporter;
			}
		}
	}
}