using BaseLibrary.Input;
using BaseLibrary.Input.Mouse;
using BaseLibrary.UI;
using BaseLibrary.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Teleportation.TileEntities;
using Terraria;
using Terraria.Localization;

namespace Teleportation.UI
{
	public class UITeleporterItem : UIPanel, IGridElement<UITeleporterItem>
	{
		private Teleporter teleporter;

		private TeleporterPanel panel;

		public bool Selected => panel.SelectedTeleporter == teleporter;

		public UIGrid<UITeleporterItem> Grid { get; set; }

		private UIButton buttomShowOnMap;
		private UITexture textureConnection;

		private bool Outbound => panel.Container.Destination == teleporter;
		private bool Inbound => teleporter.Destination == panel.Container;
		private Texture2D Texture => Outbound ? Teleportation.textureOutbound : Inbound ? Teleportation.textureInbound : null;

		public UITeleporterItem(Teleporter teleporter, TeleporterPanel panel)
		{
			this.panel = panel;
			this.teleporter = teleporter;

			UIIcon icon = new UIIcon(teleporter)
			{
				Height = { Percent = 100 }
				//SubstituteWidth = true
			};
			Add(icon);

			UIText textDisplayName = new UIText(teleporter.DisplayName)
			{
				X = { Pixels = 48 }
			};
			Add(textDisplayName);

			buttomShowOnMap = new UIButton(Main.mapIconTexture[0])
			{
				Size = new Vector2(20),
				X = { Percent = 100 },
				HoverText = Language.GetText("Mods.Teleportation.UI.ShowOnMap")
			};
			Add(buttomShowOnMap);

			textureConnection = new UITexture(null, ScaleMode.Stretch)
			{
				Size = new Vector2(20),
				X = { Percent = 100 },
				Y = { Percent = 100 }
			};
			//textureConnection.GetHoverText += () => Outbound ? Language.GetTextValue("Mods.Teleportation.UI.OutboundConnection") : Inbound ? Language.GetTextValue("Mods.Teleportation.UI.InboundConnection") : "";
			Add(textureConnection);
		}

		protected override void MouseClick(MouseButtonEventArgs args)
		{
			if (args.Button != MouseButton.Left) return;

			panel.SelectedTeleporter = Selected ? null : teleporter;
		}

		//public override void PreDraw(SpriteBatch spriteBatch)
		//{
		//	textureConnection.texture = Texture;
		//	textureConnection.Rotation = Outbound ? 90 : 0;
		//	BackgroundColor = IsMouseHovering ? Utility.ColorPanel_Hovered : Selected ? Utility.ColorPanel_Selected : Utility.ColorPanel;
		//}
	}
}