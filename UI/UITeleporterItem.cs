using BaseLibrary.UI.Elements;
using BaseLibrary.Utility;
using Microsoft.Xna.Framework;
using Teleportation.TileEntities;
using Terraria;
using Terraria.UI;

namespace Teleportation.UI
{
	public class UITeleporterItem : UIPanel, IGridElement<UITeleporterItem>
	{
		public TETeleporter teleporter;
		public bool Selected;

		public UIGrid<UITeleporterItem> Grid { get; set; }

		public UITeleporterItem(TETeleporter teleporter)
		{
			this.teleporter = teleporter;
			SetPadding(8);

			UIIcon icon = new UIIcon(teleporter)
			{
				Height = (0, 1),
				SubstituteWidth = true
			};
			Append(icon);

			UIText textDisplayName = new UIText(teleporter.DisplayName.Value)
			{
				Left = (48, 0)
			};
			Append(textDisplayName);

			UIButton buttomShowOnMap = new UIButton(Main.mapIconTexture[0])
			{
				Size = new Vector2(20),
				HAlign = 1f
			};
			buttomShowOnMap.GetHoverText += () => "Show on map";
			Append(buttomShowOnMap);
		}

		public override void Click(UIMouseEvent evt)
		{
			if (evt.Target != this) return;

			Grid.items.ForEach(item => item.Selected = false);
			Selected = true;
		}

		public override void MouseOver(UIMouseEvent evt)
		{
			base.MouseOver(evt);

			BackgroundColor = Utility.ColorPanel_Hovered;
		}

		public override void MouseOut(UIMouseEvent evt)
		{
			base.MouseOut(evt);

			BackgroundColor = Selected ? Utility.ColorPanel_Selected : Utility.ColorPanel;
		}
	}
}