using BaseLibrary;
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
	}
}