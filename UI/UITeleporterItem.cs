using System;
using BaseLibrary.UI.Elements;
using BaseLibrary.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Teleportation.Items;
using Teleportation.TileEntities;
using Terraria;
using Terraria.UI;

namespace Teleportation.UI
{
	public class UITeleporterItem : UIGridElement<UITeleporterItem>
	{
		public TETeleporter teleporter;
		public bool Selected;

		public UITeleporterItem(TETeleporter teleporter)
		{
			this.teleporter = teleporter;
			SetPadding(8);

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

			grid.items.ForEach(item => item.Selected = false);
			Selected = true;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			CalculatedStyle dimensions = GetDimensions();
			CalculatedStyle innerDimensions = GetInnerDimensions();

			spriteBatch.DrawPanel(dimensions, IsMouseHovering ? Utility.ColorPanel_Hovered : Selected ? Utility.ColorPanel_Selected : Utility.ColorPanel);

			CalculatedStyle iconDimensions = new CalculatedStyle(innerDimensions.X, innerDimensions.Y, innerDimensions.Height, innerDimensions.Height);
			spriteBatch.DrawPanel(iconDimensions);

			Texture2D texture = Main.itemTexture[Teleportation.Instance.ItemType<Teleporter>()];
			spriteBatch.Draw(texture, iconDimensions.Center(), null, Color.White, 0f, texture.Size() * 0.5f, Math.Min((iconDimensions.Width - 10f) / texture.Width, (iconDimensions.Height - 10f) / texture.Height), SpriteEffects.None, 0f);

			//ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, teleporter.DisplayName.Value, new Vector2(innerDimensions.X + innerDimensions.Height + 8, innerDimensions.Y), Color.White, 0f, Vector2.Zero, Vector2.One);

			//if (entity != null)
			//{
			//	typeof(Utility).InvokeMethod<object>("Draw" + teleporter.entityType, new[] { spriteBatch, entity, new Vector2(dimensions.X + 16f, dimensions.Y + 16f), new Vector2(dimensions.Height - 32f) });

			//	Utils.DrawBorderStringFourWay(spriteBatch, Main.fontMouseText, teleporter.name, dimensions.X + dimensions.Height, dimensions.Y + 8f, Color.White, Color.Black, Vector2.Zero);
			//	Utils.DrawBorderStringFourWay(spriteBatch, Main.fontMouseText, $"Position: {teleporter.Position.X}; {teleporter.Position.Y}", dimensions.X + dimensions.Height, dimensions.Y + dimensions.Height - 28f, Color.White, Color.Black, Vector2.Zero);
			//}
		}
	}
}