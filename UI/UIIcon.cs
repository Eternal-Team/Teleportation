using BaseLibrary;
using BaseLibrary.UI.Elements;
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
			SetPadding(8);
		}

		public override void Update(GameTime gameTime)
		{
			Animation.Update();
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			spriteBatch.DrawPanel(Dimensions);

			spriteBatch.Draw(Utility.ImmediateState, () =>
			{
				Rectangle rectangle = Animation.GetFrame(Texture);
				spriteBatch.Draw(Texture, Dimensions.Center(), rectangle, Color.White, 0f, rectangle.Size() * 0.5f, Math.Min(InnerDimensions.Width / rectangle.Width, InnerDimensions.Height / rectangle.Height), SpriteEffects.None, 0f);
			});
		}
	}
}