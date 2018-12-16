using BaseLibrary.UI.Elements;
using BaseLibrary.Utility;
using Microsoft.Xna.Framework.Graphics;
using Teleportation.TileEntities;
using Terraria.UI;

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
			CalculatedStyle dimensions = GetDimensions();

			spriteBatch.DrawPanel(dimensions);

			if (teleporter.entityID < 1) return;

			switch (teleporter.entityType)
			{
				case "Item":
					spriteBatch.DrawItem(BaseLibrary.BaseLibrary.itemCache[teleporter.entityID], dimensions.Center(), GetInnerDimensions().Size());
					break;
				case "NPC":
					spriteBatch.DrawNPC(BaseLibrary.BaseLibrary.npcCache[teleporter.entityID], dimensions.Center(), GetInnerDimensions().Size());
					break;
				case "Projectile":
					spriteBatch.DrawProjectile(BaseLibrary.BaseLibrary.projectileCache[teleporter.entityID], dimensions.Center(), GetInnerDimensions().Size());
					break;
			}
		}
	}
}