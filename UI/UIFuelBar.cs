using BaseLibrary.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;

namespace Teleportation.UI
{
	public class UIFuelBar : BaseElement
	{
		public static Texture2D FuelBar { get; set; }

		public static Color ColorBar = new Color(109, 255, 255);

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			CalculatedStyle dimensions = GetDimensions();

			spriteBatch.Draw(FuelBar, dimensions.Position(), Color.White);
			spriteBatch.Draw(FuelBar, dimensions.Position() + new Vector2(dimensions.Width - 12, 0), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0f);
			spriteBatch.Draw(Main.magicPixel, new Rectangle((int)(dimensions.X + 12), (int)dimensions.Y, (int)(dimensions.Width - 24), (int)dimensions.Height), ColorBar);
		}
	}
}