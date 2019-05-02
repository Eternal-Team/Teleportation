using BaseLibrary;
using BaseLibrary.UI.Elements;
using Microsoft.Xna.Framework.Graphics;
using Teleportation.TileEntities;
using Terraria;

namespace Teleportation.UI
{
	public class UIIcon : BaseElement
	{
		private TETeleporter teleporter;

		public UIIcon(TETeleporter teleporter)
		{
			this.teleporter = teleporter;
			SetPadding(5);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			spriteBatch.DrawPanel(Dimensions);

			if (teleporter.entity == null) return;

			Main.spriteBatch.DrawEntity(teleporter.entity, Dimensions.Center(), InnerDimensions.Size());
		}
	}
}