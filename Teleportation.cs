using System.Linq;
using BaseLibrary;
using Microsoft.Xna.Framework;
using Teleportation.TileEntities;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Teleportation
{
	public class Teleportation : Mod
	{
		internal static Teleportation Instance;

		public override void Load()
		{
			Instance = this;
		}

		public override void Unload() => Utility.UnloadNullableTypes();

		public override void UpdateUI(GameTime gameTime)
		{
			foreach (TETeleporter teleporter in TileEntity.ByID.Values.OfType<TETeleporter>())
			{
				teleporter.EntityAnimation?.Update();
			}
		}
	}
}