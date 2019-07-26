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
		internal static Texture2D teleporterGlow;
		internal static Texture2D whitelistPlayer;
		internal static Texture2D whitelistNPC;
		internal static Texture2D whitelistItem;
		internal static Texture2D whitelistProjectile;

		public override void Load()
		{
			Instance = this;

			if (!Main.dedServ)
			{
				teleporterEffect = ModContent.GetTexture("Teleportation/Textures/TeleporterEffect");
				teleporterGlow = ModContent.GetTexture("Teleportation/Textures/TeleporterGlow");
				whitelistPlayer = ModContent.GetTexture("Teleportation/Textures/Whitelist_Player");
				whitelistNPC = ModContent.GetTexture("Teleportation/Textures/Whitelist_NPC");
				whitelistItem = ModContent.GetTexture("Teleportation/Textures/Whitelist_Item");
				whitelistProjectile = ModContent.GetTexture("Teleportation/Textures/Whitelist_Projectile");
			}
		}

		public override void Unload() => Utility.UnloadNullableTypes();

		public override void UpdateUI(GameTime gameTime)
		{
			foreach (Teleporter teleporter in TileEntity.ByID.Values.OfType<Teleporter>())
			{
				teleporter.EntityAnimation?.Update();
			}
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI) => Net.HandlePacket(reader,whoAmI);
	}
}