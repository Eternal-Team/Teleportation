using BaseLibrary;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace Teleportation
{
	public class Teleportation : Mod
	{
		internal static Texture2D teleporterEffect;
		internal static Texture2D[] teleporterGlow;
		internal static Texture2D[] whitelist;
		internal static Texture2D textureInbound;
		internal static Texture2D textureOutbound;

		public override void Load()
		{
			Hooking.Load();

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

				textureInbound = ModContent.GetTexture("BaseLibrary/Textures/UI/DepositAll");
				textureOutbound = ModContent.GetTexture("BaseLibrary/Textures/UI/QuickStack");
			}
		}

		public override void Unload() => this.UnloadNullableTypes();

		public override void HandlePacket(BinaryReader reader, int whoAmI) => Net.HandlePacket(reader, whoAmI);
	}
}