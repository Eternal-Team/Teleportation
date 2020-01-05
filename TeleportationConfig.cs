using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace Teleportation
{
	public class TeleportationConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[DefaultValue(true), ReloadRequired, Label("$Mods.Teleportation.Config.ShowTeleportersOnMap")]
		public bool ShowTeleportersOnMap;
	}
}