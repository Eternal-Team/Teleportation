using BaseLibrary;
using BaseLibrary.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Teleportation.TileEntities;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Teleportation.Tiles
{
	public class Teleporter : BaseTile
	{
		public override string Texture => "Teleportation/Textures/Tiles/Teleporter";

		public override void SetDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileSolidTop[Type] = false;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = false;
			Main.tileLavaDeath[Type] = false;
			Main.tileHammer[Type] = false;

			TileObjectData.newTile.Width = 3;
			TileObjectData.newTile.Height = 1;
			TileObjectData.newTile.Origin = new Point16(0, 0);
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 0);
			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.newTile.CoordinateHeights = new[] {16};
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(mod.GetTileEntity<TileEntities.Teleporter>().Hook_AfterPlacement, -1, 0, false);
			TileObjectData.addTile(Type);
			disableSmartCursor = true;

			ModTranslation name = CreateMapEntryName();
			AddMapEntry(Color.Cyan, name);
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			TileEntities.Teleporter teleporter = mod.GetTileEntity<TileEntities.Teleporter>(i, j);
			if (teleporter == null || !Main.tile[i, j].IsTopLeft() || teleporter.Destination == null) return true;

			Vector2 position = new Point16(i + 1, j).ToScreenCoordinates();

			spriteBatch.Draw(Teleportation.teleporterEffect, position + new Vector2(8, 2), null, Color.White * 0.75f, 0f, new Vector2(10, 100), new Vector2(2, 1), SpriteEffects.None, 0f);

			return true;
		}

		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
		{
			Main.specX[nextSpecialDrawIndex] = i;
			Main.specY[nextSpecialDrawIndex] = j;
			nextSpecialDrawIndex++;
		}

		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
		{
			TileEntities.Teleporter teleporter = mod.GetTileEntity<TileEntities.Teleporter>(i, j);
			if (teleporter == null || !Main.tile[i, j].IsTopLeft()) return;

			Vector2 position = new Point16(i, j).ToScreenCoordinates();

			spriteBatch.Draw(Teleportation.teleporterGlow, position + new Vector2(8, 4), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
		}

		public override void RightClick(int i, int j)
		{
			TileEntities.Teleporter teleporter = mod.GetTileEntity<TileEntities.Teleporter>(i, j);
			if (teleporter == null) return;

			BaseLibrary.BaseLibrary.PanelGUI.UI.HandleUI(teleporter);
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			TileEntities.Teleporter teleporter = mod.GetTileEntity<TileEntities.Teleporter>(i, j);
			if (teleporter != null) BaseLibrary.BaseLibrary.PanelGUI.UI.CloseUI(teleporter);

			Item.NewItem(i * 16, j * 16, 48, 16, mod.ItemType<Items.Teleporter>());
			mod.GetTileEntity<TileEntities.Teleporter>().Kill(i, j);
		}
	}
}