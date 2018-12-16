using System;
using System.Collections.Generic;
using System.Linq;
using BaseLibrary.Tiles.TileEntites;
using ContainerLibrary;
using Microsoft.Xna.Framework;
using Teleportation.Tiles;
using Teleportation.UI;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Teleportation.TileEntities
{
	public class TETeleporter : BaseTE
	{
		public override Type TileType => typeof(Teleporter);

		public Rectangle Hitbox => new Rectangle(Position.X * 16, Position.Y * 16 - 48, 48, 48);

		public TeleporterPanel UI => Teleportation.Instance.TeleporterUI.UI.OfType<TeleporterPanel>().FirstOrDefault(x => x.teleporter.ID == ID);
		public Vector2? UIPosition;

		public static LegacySoundStyle OpenSound => SoundID.Item1;
		public static LegacySoundStyle CloseSound => SoundID.Item1;

		public ItemHandler Handler;
		public Ref<string> DisplayName = new Ref<string>("Teleporter");
		public bool[] Whitelist = { true, false, false, false };
		public TETeleporter destination;
		public bool dialOnce;
		public string entityType = "Item";
		public int entityID = -1;

		public TETeleporter()
		{
			Handler = new ItemHandler(1);
			Handler.OnContentsChanged += slot => { };
		}

		public override void OnPlace()
		{
			entityID = Teleportation.Instance.ItemType<Items.Teleporter>();
		}

		public override void Update()
		{
			if (destination == null) return;

			bool teleported = false;

			if (Whitelist[0])
			{
				List<Player> players = Main.player.Where(player => player.getRect().Intersects(Hitbox)).ToList();

				if (players.Any())
				{
					foreach (Player player in players)
					{
						player.Teleport(destination.Position.ToWorldCoordinates(0, 0) + new Vector2(24 - player.width * 0.5f, -player.height));
						NetMessage.SendData(MessageID.Teleport, -1, -1, null, 0, player.whoAmI, player.position.X, player.position.Y);
					}

					teleported = true;
				}
			}

			if (Whitelist[1])
			{
				List<NPC> npcs = Main.npc.Where(npc => npc.getRect().Intersects(Hitbox)).ToList();

				if (npcs.Any())
				{
					foreach (NPC npc in npcs) npc.Teleport(destination.Position.ToWorldCoordinates(0, 0) + new Vector2(24 - npc.width * 0.5f, -npc.height));

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
						item.position = destination.Position.ToWorldCoordinates(0, 0) + new Vector2(24 - item.width * 0.5f, -item.height);
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
						projectile.position = destination.Position.ToWorldCoordinates(0, 0) + new Vector2(24 - projectile.width * 0.5f, -projectile.height);
						NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, projectile.whoAmI);
					}

					teleported = true;
				}
			}

			if (dialOnce && teleported)
			{
				destination = null;
				dialOnce = false;
			}
		}

		public override TagCompound Save() => new TagCompound
		{
			["DisplayName"] = DisplayName.Value,
			["Whitelist"] = Whitelist.ToList(),
			["Destination"] = destination?.Position ?? Point16.NegativeOne,
			["EntityType"] = entityType,
			["EntityID"] = entityID
		};

		public override void Load(TagCompound tag)
		{
			DisplayName.Value = tag.GetString("DisplayName");
			Whitelist = tag.GetList<bool>("Whitelist").ToArray();
			Point16 pos = tag.Get<Point16>("Destination");
			destination = pos != Point16.NegativeOne && ByPosition.ContainsKey(pos) ? (TETeleporter)ByPosition[pos] : null;
			entityType = tag.GetString("EntityType");
			int entityId = tag.GetInt("EntityID");
			entityID = entityId > 0 ? entityId : Teleportation.Instance.ItemType<Items.Teleporter>();
		}

		public override void OnKill() => Handler.DropItems(new Rectangle(Position.X * 16, Position.Y * 16, 48, 16));
	}
}