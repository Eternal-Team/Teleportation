using BaseLibrary;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace Teleportation
{
	// todo: minimap/fullscreen map display of teleporters

	public class Teleportation : Mod
	{
		internal static Teleportation Instance;

		internal static Texture2D teleporterEffect;
		internal static Texture2D[] teleporterGlow;
		internal static Texture2D[] whitelist;
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

				whitelist = new Texture2D[5];
				whitelist[0] = ModContent.GetTexture("Teleportation/Textures/UI/Whitelist_Player");
				whitelist[1] = ModContent.GetTexture("Teleportation/Textures/UI/Whitelist_NPC");
				whitelist[2] = ModContent.GetTexture("Teleportation/Textures/UI/Whitelist_Item");
				whitelist[3] = ModContent.GetTexture("Teleportation/Textures/UI/Whitelist_Projectile");
				whitelist[4] = ModContent.GetTexture("Teleportation/Textures/UI/Whitelist_Boss");

				textureDepositAll = ModContent.GetTexture("BaseLibrary/Textures/UI/DepositAll");
				textureRestock = ModContent.GetTexture("BaseLibrary/Textures/UI/Restock");
			}
		}

		public override void Unload() => Utility.UnloadNullableTypes();

		public override void HandlePacket(BinaryReader reader, int whoAmI) => Net.HandlePacket(reader, whoAmI);
	}
}