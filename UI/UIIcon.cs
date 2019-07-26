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
		private Texture2D Texture => teleporter.EntityTexture;
		private DrawAnimation Animation => teleporter.EntityAnimation;

		public UIIcon(Teleporter teleporter)
		{
			this.teleporter = teleporter;
			SetPadding(5);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			spriteBatch.DrawPanel(Dimensions);

			if (Texture == null) return;

			// different state?
			spriteBatch.Draw(Utility.PointClampState, () =>
			{
				Rectangle rectangle = Animation?.GetFrame(Texture) ?? Texture.Frame();
				spriteBatch.Draw(Texture, Dimensions.Center(), rectangle, Color.White, 0f, rectangle.Size() * 0.5f, Math.Min(InnerDimensions.Width / rectangle.Width, InnerDimensions.Height / rectangle.Height), SpriteEffects.None, 0f);
			});
		}
	}
}