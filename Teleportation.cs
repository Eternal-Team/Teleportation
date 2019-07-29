using BaseLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Linq;
using Teleportation.TileEntities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Teleportation
{
	// todo: minimap/fullscreen map display of teleporters

	public class Teleportation : Mod
	{
		internal static Teleportation Instance;

		internal static Texture2D teleporterEffect;
		internal static Texture2D[] teleporterGlow;
		internal static Texture2D whitelistPlayer;
		internal static Texture2D whitelistNPC;
		internal static Texture2D whitelistItem;
		internal static Texture2D whitelistProjectile;
		internal static Texture2D whitelistBoss;
		internal static Texture2D textureDepositAll;
		internal static Texture2D textureRestock;

		public override void Load()
		{
			Instance = this;

			if (!Main.dedServ)
			{
				teleporterEffect = ModContent.GetTexture("Teleportation/Textures/Tiles/TeleporterEffect");

				teleporterGlow = new Texture2D[4];
				teleporterGlow[0] = ModContent.GetTexture("Teleportation/Textures/Tiles/BasicTeleporter_Glow");
				teleporterGlow[1] = ModContent.GetTexture("Teleportation/Textures/Tiles/AdvancedTeleporter_Glow");
				teleporterGlow[2] = ModContent.GetTexture("Teleportation/Textures/Tiles/EliteTeleporter_Glow");
				teleporterGlow[3] = ModContent.GetTexture("Teleportation/Textures/Tiles/UltimateTeleporter_Glow");

				whitelistPlayer = ModContent.GetTexture("Teleportation/Textures/UI/Whitelist_Player");
				whitelistNPC = ModContent.GetTexture("Teleportation/Textures/UI/Whitelist_NPC");
				whitelistItem = ModContent.GetTexture("Teleportation/Textures/UI/Whitelist_Item");
				whitelistProjectile = ModContent.GetTexture("Teleportation/Textures/UI/Whitelist_Projectile");
				whitelistBoss = ModContent.GetTexture("Teleportation/Textures/UI/Whitelist_Boss");

				textureDepositAll = ModContent.GetTexture("BaseLibrary/Textures/UI/DepositAll");
				textureRestock = ModContent.GetTexture("BaseLibrary/Textures/UI/Restock");
			}
		}

		public override void Unload() => Utility.UnloadNullableTypes();

		public override void UpdateUI(GameTime gameTime)
		{
			foreach (Teleporter teleporter in TileEntity.ByID.Values.OfType<Teleporter>()) teleporter.EntityAnimation?.Update();
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI) => Net.HandlePacket(reader, whoAmI);
	}
}