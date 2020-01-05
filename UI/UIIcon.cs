using BaseLibrary;
using BaseLibrary.UI.New;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Teleportation.TileEntities;
using Terraria;
using Terraria.DataStructures;

namespace Teleportation.UI
{
	public class UIIcon : BaseElement
	{
		private Teleporter teleporter;
		private Texture2D Texture => teleporter.Icon.Texture;
		private DrawAnimation Animation => teleporter.Icon.animation;

		public UIIcon(Teleporter teleporter)
		{
			this.teleporter = teleporter;
			Padding = new Padding(8);
		}

		protected override void Update(GameTime gameTime)
		{
			Animation.Update();
		}

		protected override void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.DrawPanel(Dimensions);

			spriteBatch.Draw(Utility.ImmediateState, () =>
			{
				Rectangle rectangle = Animation.GetFrame(Texture);
				spriteBatch.Draw(Texture, Utility.Center(Dimensions), rectangle, Color.White, 0f, rectangle.Size() * 0.5f, Math.Min(InnerDimensions.Width / rectangle.Width, InnerDimensions.Height / rectangle.Height), SpriteEffects.None, 0f);
			});
		}
	}
}