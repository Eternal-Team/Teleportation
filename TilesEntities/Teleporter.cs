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
using Terraria.UI;

namespace Teleportation.TileEntities
{
	public class IconData
	{
		public enum Type
		{
			Item,
			NPC,
			Projectile
		}

		private Type type;
		private int id;
		private string uniqueKey;
		public DrawAnimationVertical animation;

		public Texture2D Texture
		{
			get
			{
				switch (type)
				{
					case Type.Item:
						return Main.itemTexture[id];
					case Type.NPC:
						return Main.npcTexture[id];
					case Type.Projectile:
						return Main.projectileTexture[id];
				}

				return null;
			}
		}

		public IconData()
		{
		}

		public IconData(Type type, int id)
		{
			this.type = type;
			this.id = id;

			switch (type)
			{
				case Type.Item:
					uniqueKey = ItemID.GetUniqueKey(id);
					animation = Main.itemAnimations[id] as DrawAnimationVertical ?? new DrawAnimationVertical(0, 1);
					break;
				case Type.NPC:
					uniqueKey = NPCID.GetUniqueKey(id);
					animation = new DrawAnimationVertical(5, Main.npcFrameCount[id]);
					break;
				case Type.Projectile:
					uniqueKey = ProjectileID.GetUniqueKey(id);
					animation = new DrawAnimationVertical(5, Main.projFrames[id]);
					break;
			}
		}

		public TagCompound Save() => new TagCompound
		{
			["Type"] = (int)type,
			["UniqueKey"] = uniqueKey,
			["TicksPerFrame"] = animation.TicksPerFrame,
			["FrameCount"] = animation.FrameCount
		};

		public void Load(TagCompound tag)
		{
			type = (Type)tag.Get<int>("Type");
			uniqueKey = tag.GetString("UniqueKey");

			switch (type)
			{
				case Type.Item:
					id = ItemID.TypeFromUniqueKey(uniqueKey);
					break;
				case Type.NPC:
					id = NPCID.TypeFromUniqueKey(uniqueKey);
					break;
				case Type.Projectile:
					id = ProjectileID.TypeFromUniqueKey(uniqueKey);
					break;
			}

			animation = new DrawAnimationVertical(tag.Get<int>("TicksPerFrame"), tag.Get<int>("FrameCount"));
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write((int)type);
			writer.Write(id);
			writer.Write(uniqueKey);
			writer.Write(animation.TicksPerFrame);
			writer.Write(animation.FrameCount);
		}

		public void Read(BinaryReader reader)
		{
			type = (Type)reader.ReadInt32();
			id = reader.ReadInt32();
			uniqueKey = reader.ReadString();
			animation = new DrawAnimationVertical(reader.ReadInt32(), reader.ReadInt32());
		}

		public IconData Clone() => new IconData
		{
			type = type,
			id = id,
			uniqueKey = uniqueKey,
			animation = new DrawAnimationVertical(animation.TicksPerFrame, animation.FrameCount)
		};
	}

	public abstract class Teleporter : BaseTE, IHasUI, IItemHandler
	{
		protected virtual Rectangle Hitbox => new Rectangle(Position.X * 16 + 8, Position.Y * 16 - 8, 32, 8);
		protected abstract int TeleportationDelay { get; }

		public LegacySoundStyle CloseSound => SoundID.Item1;

		public LegacySoundStyle OpenSound => SoundID.Item1;

		public Guid UUID { get; set; }

		public BaseUIPanel UI { get; set; }

		public ItemHandler Handler { get; }

		private int timer;

		public Point16 _destination;

		public BaseLibrary.Ref<string> DisplayName;
		public bool[] Whitelist;
		public bool DialOnce;

		public IconData Icon
		{
			get => icon ?? (icon = new IconData(IconData.Type.Item, mod.ItemType(TileType.Name)));
			set => icon = value;
		}

		public Teleporter Destination
		{
			get
			{
				if (ByPosition.TryGetValue(_destination, out TileEntity te)) return (Teleporter)te;
				return null;
			}
			set => _destination = value?.Position ?? Point16.NegativeOne;
		}

		public bool Active => Destination != null || ByPosition.Values.OfType<Teleporter>().Any(te => te.Destination == this);

		private IconData icon;

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
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				foreach (UIElement element in BaseLibrary.BaseLibrary.PanelGUI.Elements)
				{
					if (element is TeleporterPanel panel) panel.UpdateGrid();
				}
			}
			else Net.SendPlaceTeleporter();
		}

		public override void Update()
		{
			if (Destination == null || Handler.Items[0].IsAir) return;
			if (++timer < TeleportationDelay) return;
			timer = 0;

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

						if (Main.netMode == NetmodeID.SinglePlayer) BaseLibrary.BaseLibrary.PanelGUI.UI.CloseUI(this);
						else Net.SendCloseUI(this, player.whoAmI);

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

						npc.Teleport(new Vector2(Destination.Hitbox.Center.X - npc.width * 0.5f, Destination.Hitbox.Bottom - npc.height));

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

		public override TagCompound Save() => new TagCompound
		{
			["DisplayName"] = DisplayName.Value,
			["Whitelist"] = Whitelist.ToList(),
			["Destination"] = _destination,
			["UUID"] = UUID,
			["Items"] = Handler.Save(),
			["Icon"] = Icon.Save()
		};

		public override void Load(TagCompound tag)
		{
			UUID = tag.Get<Guid>("UUID");
			DisplayName.Value = tag.GetString("DisplayName");
			Whitelist = tag.GetList<bool>("Whitelist").ToArray();
			_destination = tag.Get<Point16>("Destination");
			Handler.Load(tag.GetCompound("Items"));
			Icon.Load(tag.GetCompound("Icon"));
		}

		public override void NetSend(BinaryWriter writer, bool lightSend)
		{
			writer.Write(UUID);
			writer.Write(_destination);
			for (int i = 0; i < Whitelist.Length; i++) writer.Write(Whitelist[i]);

			writer.Write(DisplayName.Value);
			writer.Write(DialOnce);

			Handler.Write(writer);

			Icon.Write(writer);
		}

		public override void NetReceive(BinaryReader reader, bool lightReceive)
		{
			UUID = reader.ReadGUID();
			_destination = reader.ReadPoint16();
			for (int i = 0; i < Whitelist.Length; i++) Whitelist[i] = reader.ReadBoolean();

			DisplayName.Value = reader.ReadString();
			DialOnce = reader.ReadBoolean();

			Handler.Read(reader);

			Icon.Read(reader);
		}

		public override void OnKill()
		{
			foreach (TileEntity tileEntity in ByPosition.Values)
			{
				if (tileEntity is Teleporter teleporter && teleporter.Destination == this) teleporter.Destination = null;
			}

			Net.SendKillTeleporter(this);

			Handler.DropItems(new Rectangle(Position.X * 16, Position.Y * 16, 48, 16));
		}

		public abstract bool ConsumeFuel(int type);

		public abstract IEnumerable<Teleporter> GetConnections();
	}

	public class BasicTeleporter : Teleporter
	{
		public override Type TileType => typeof(Tiles.BasicTeleporter);

		protected override int TeleportationDelay => 60;

		public override bool ConsumeFuel(int type) => Handler.Shrink(0, type > 0 ? 1 : 5);

		public override IEnumerable<Teleporter> GetConnections()
		{
			foreach (TileEntity tileEntity in ByID.Values)
			{
				if (tileEntity is BasicTeleporter teleporter && teleporter != this && Vector2.DistanceSquared(Position.ToWorldCoordinates(24), teleporter.Position.ToWorldCoordinates(24)) <= 32 * 32 * 256) yield return teleporter;
			}
		}
	}

	public class AdvancedTeleporter : Teleporter
	{
		public override Type TileType => typeof(Tiles.AdvancedTeleporter);

		protected override int TeleportationDelay => 30;

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

		protected override int TeleportationDelay => 15;

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

		protected override int TeleportationDelay => 6;

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