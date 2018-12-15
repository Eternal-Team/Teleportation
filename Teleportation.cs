using System.Collections.Generic;
using System.Linq;
using BaseLibrary.UI;
using BaseLibrary.Utility;
using Microsoft.Xna.Framework;
using Teleportation.TileEntities;
using Teleportation.UI;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.UI;

namespace Teleportation
{
	public class Teleportation : Mod
	{
		public static Teleportation Instance;

		public GUI<TeleporterUI> TeleporterUI;

		public override void Load()
		{
			Instance = this;

			if (!Main.dedServ)
			{
				this.LoadTextures();

				TeleporterUI = Utility.SetupGUI<TeleporterUI>();
				TeleporterUI.Visible = true;
			}
		}

		public override void Unload() => Utility.UnloadNullableTypes();

		public override void PreSaveAndQuit() => TeleporterUI.UI.Elements.Clear();

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int HotbarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Hotbar"));

			if (HotbarIndex != -1 && TeleporterUI != null) layers.Insert(HotbarIndex + 1, TeleporterUI.InterfaceLayer);
		}

		public override void UpdateUI(GameTime gameTime)
		{
			foreach (TETeleporter teleporter in TileEntity.ByPosition.Values.OfType<TETeleporter>())
			{
				if (!Main.LocalPlayer.WithinRange(teleporter.Position.ToWorldCoordinates(24), 240)) TeleporterUI.UI.CloseUI(teleporter);
			}

			TeleporterUI.Update(gameTime);
		}
	}
}