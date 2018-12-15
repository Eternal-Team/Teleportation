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
		public bool[] Whitelist = new bool[4];
		public TETeleporter destination;
		public bool dialOnce;

		public TETeleporter()
		{
			Handler = new ItemHandler(1);
			Handler.OnContentsChanged += slot => { };
		}

		#region Data
		public string entityType = "Item";
		public int entityID = -1;
		#endregion

		public void TeleportPlayer()
		{
			//	if (teleporter.destinationTeleporter != Point16.NegativeOne && Main.LocalPlayer.getRect().Intersects(teleporter.Hitbox) && !teleporter.fuel.IsAir)
			//	{
			//		Main.LocalPlayer.Teleport(new Vector2(teleporter.destinationTeleporter.X * 16 + 24 - Main.LocalPlayer.width * 0.5f, teleporter.destinationTeleporter.Y * 16 - Main.LocalPlayer.height), 2);
			//		Main.LocalPlayer.GetModPlayer<TPlayer>().teleportedTimer = 60;

			//		teleporter.fuel.stack--;
			//		if (teleporter.fuel.stack <= 0) teleporter.fuel.TurnToAir();

			//		teleporter.Sync();

			//		teleporter.CloseUI();
			//		Main.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy);
			//	}
		}

		public override void Update()
		{
			if (destination == null) return;

			bool teleported = false;

			if (Whitelist[0])
			{
				List<Player> players = Main.player.Where(player => Hitbox.Contains(player.getRect())).ToList();

				if (players.Any())
				{
					foreach (Player player in players)
					{
						player.Teleport(destination.Position.ToWorldCoordinates() + new Vector2(24 - player.width * 0.5f, -player.height));
						NetMessage.SendData(MessageID.Teleport, -1, -1, null, 0, player.whoAmI, player.position.X, player.position.Y);
					}

					teleported = true;
				}
			}

			if (dialOnce && teleported)
			{
				destination = null;
				dialOnce = false;
			}

			//if (teleporting && destinationTeleporter != Point16.NegativeOne && !fuel.IsAir)
			//{

			//	if (whitelist[1])
			//		foreach (Item item in Main.item.Where(x => x.getRect().Intersects(Hitbox)))
			//		{
			//			item.position = new Vector2(destinationTeleporter.X * 16 + 24 - item.width * 0.5f, destinationTeleporter.Y * 16 - 24 - item.height);
			//			NetMessage.SendData(MessageID.SyncItem, -1, -1, null, Main.item.ToList().IndexOf(item));
			//		}

			//	if (whitelist[2])
			//		foreach (NPC npc in Main.npc.Where(x => x.getRect().Intersects(Hitbox)))
			//			npc.Teleport(new Vector2(destinationTeleporter.X * 16 + 24 - npc.width * 0.5f, destinationTeleporter.Y * 16 - npc.height));
			//	if (whitelist[3])
			//		foreach (Projectile projectile in Main.projectile.Where(x => x.getRect().Intersects(Hitbox)))
			//		{
			//			projectile.position = new Vector2(destinationTeleporter.X * 16 + 24 - projectile.width * 0.5f, destinationTeleporter.Y * 16 - 24 - projectile.height);
			//			NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, Main.projectile.ToList().IndexOf(projectile));
			//		}
			//}
		}

		public override TagCompound Save() => new TagCompound
		{
			["DisplayName"] = DisplayName.Value,
			["Whitelist"] = Whitelist.ToList(),
			["Destination"] = destination?.Position ?? Point16.NegativeOne
		};

		public override void Load(TagCompound tag)
		{
			DisplayName.Value = tag.GetString("DisplayName");
			Whitelist = tag.GetList<bool>("Whitelist").ToArray();
			Point16 pos = tag.Get<Point16>("Destination");
			destination = pos != Point16.NegativeOne ? (TETeleporter)ByPosition[pos] : null;
		}

		public override void OnKill() => Handler.DropItems(new Rectangle(Position.X * 16, Position.Y * 16, 48, 16));
	}
}