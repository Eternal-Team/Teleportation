using System;
using System.Collections.Generic;
using System.Linq;
using BaseLibrary.Tiles.TileEntites;
using BaseLibrary.UI;
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
		public TileEntity destination;

		public Entity entity;

		public TETeleporter()
		{
			ID = Guid.NewGuid();

			Handler = new ItemHandler();
			Handler.OnContentsChanged += slot => { };
			Handler.IsItemValid += (slot, item) => item.type == mod.ItemType<FuelCell>();
		}

		public override void OnPlace()
		{
			entity = new Item();
			((Item)entity).SetDefaults(Teleportation.Instance.ItemType<Items.Teleporter>());

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
			["ID"] = ID.ToString(),
			["Items"] = Handler.Save()
		};

		public override void Load(TagCompound tag)
		{
			DisplayName.Value = tag.GetString("DisplayName");
			Whitelist = tag.GetList<bool>("Whitelist").ToArray();

			ByPosition.TryGetValue(tag.Get<Point16>("Destination"), out destination);

			ID = Guid.Parse(tag.GetString("ID"));
			Handler.Load(tag.GetCompound("Items"));
		}

		public override void OnKill() => Handler.DropItems(new Rectangle(Position.X * 16, Position.Y * 16, 48, 16));
	}
}