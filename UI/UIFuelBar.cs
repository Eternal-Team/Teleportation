using BaseLibrary.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace Teleportation.UI
{
	public class UIFuelBar : BaseElement
	{
		public static Texture2D FuelBar { get; set; }

		public static Color ColorBar = new Color(109, 255, 255);

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(FuelBar, Dimensions.Position(), Color.White);
			spriteBatch.Draw(FuelBar, Dimensions.Position() + new Vector2(Dimensions.Width - 12, 0), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0f);
			spriteBatch.Draw(Main.magicPixel, new Rectangle((int)(Dimensions.X + 12), (int)Dimensions.Y, (int)(Dimensions.Width - 24), (int)Dimensions.Height), ColorBar);
		}
	}
}