using BaseLibrary;
using BaseLibrary.UI;
using BaseLibrary.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Teleportation.TileEntities;
using Terraria;
using Terraria.Localization;
using Terraria.UI;

namespace Teleportation.UI
{
	public class UITeleporterItem : UIPanel, IGridElement<UITeleporterItem>
	{
		private Teleporter teleporter;

		private TeleporterPanel panel;

		public bool Selected => panel.SelectedDestination == teleporter;

		public UIGrid<UITeleporterItem> Grid { get; set; }

		private UIButton buttomShowOnMap;
		private UITexture textureConnection;

		private bool Outbound => panel.Container.Destination == teleporter;
		private bool Inbound => teleporter.Destination == panel.Container;
		private Texture2D Texture => Outbound ? Teleportation.textureRestock : Inbound ? Teleportation.textureDepositAll : null;

		public UITeleporterItem(Teleporter teleporter, TeleporterPanel panel)
		{
			this.panel = panel;
			this.teleporter = teleporter;

			UIIcon icon = new UIIcon(teleporter)
			{
				Height = (0, 1),
				SubstituteWidth = true
			};
			Append(icon);

			UIText textDisplayName = new UIText(teleporter.DisplayName)
			{
				Left = (48, 0)
			};
			Append(textDisplayName);

			buttomShowOnMap = new UIButton(Main.mapIconTexture[0])
			{
				Size = new Vector2(20),
				HAlign = 1f,
				HoverText = Language.GetText("Mods.Teleportation.UI.ShowOnMap")
			};
			Append(buttomShowOnMap);

			textureConnection = new UITexture(null, ScaleMode.Stretch)
			{
				Size = new Vector2(20),
				HAlign = 1,
				VAlign = 1
			};
			textureConnection.GetHoverText += () => Outbound ? Language.GetTextValue("Mods.Teleportation.UI.OutboundConnection") : Inbound ? Language.GetTextValue("Mods.Teleportation.UI.InboundConnection") : "";
			Append(textureConnection);
		}

		public override void Click(UIMouseEvent evt)
		{
			if (evt.Target == buttomShowOnMap) return;

			panel.SelectedDestination = Selected ? null : teleporter;
		}

		public override void PreDraw(SpriteBatch spriteBatch)
		{
			textureConnection.texture = Texture;
			textureConnection.Rotation = Outbound ? 90 : 0;
			BackgroundColor = IsMouseHovering ? Utility.ColorPanel_Hovered : Selected ? Utility.ColorPanel_Selected : Utility.ColorPanel;
		}
	}
}