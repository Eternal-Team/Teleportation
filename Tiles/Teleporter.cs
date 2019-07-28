using BaseLibrary;
using BaseLibrary.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Teleportation.Tiles
{
	public abstract class Teleporter : BaseTile
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
			Main.tileLighted[Type] = true;

			TileObjectData.newTile.Width = 3;
			TileObjectData.newTile.Height = 1;
			TileObjectData.newTile.Origin = new Point16(0, 0);
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 3, 0);
			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.newTile.CoordinateHeights = new[] {16};
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.addTile(Type);
			disableSmartCursor = true;

			ModTranslation name = CreateMapEntryName();
			AddMapEntry(Color.Cyan, name);
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			TileEntities.Teleporter teleporter = Utility.GetTileEntity<TileEntities.Teleporter>(i, j);
			if (teleporter?.Destination == null || !Main.tile[i, j].IsTopLeft() ) return true;

			Vector2 position = new Point16(i + 1, j).ToScreenCoordinates();

			spriteBatch.Draw(Teleportation.teleporterEffect, position + new Vector2(8, 2), null, Color.White * 0.75f, 0f, new Vector2(10, 100), new Vector2(2, 1), SpriteEffects.None, 0f);

			return true;
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			TileEntities.Teleporter teleporter = Utility.GetTileEntity<TileEntities.Teleporter>(i, j);
			if (teleporter == null) return;

			if (teleporter.Destination != null)
			{
				r = 1.0f;
				g = 0.863f;
				b = 0.0f;
			}
			else
			{
				r = 0.2f;
				g = 0.2f;
				b = 0.8f;
			}
		}

		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
		{
			Main.specX[nextSpecialDrawIndex] = i;
			Main.specY[nextSpecialDrawIndex] = j;
			nextSpecialDrawIndex++;
		}

		public override void RightClick(int i, int j)
		{
			TileEntities.Teleporter teleporter = Utility.GetTileEntity<TileEntities.Teleporter>(i, j);
			if (teleporter == null) return;

			BaseLibrary.BaseLibrary.PanelGUI.UI.HandleUI(teleporter);
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			TileEntities.Teleporter teleporter = Utility.GetTileEntity<TileEntities.Teleporter>(i, j);
			if (teleporter != null)
			{
				BaseLibrary.BaseLibrary.PanelGUI.UI.CloseUI(teleporter);
				teleporter.Kill(i, j);
			}
		}
	}

	public class BasicTeleporter : Teleporter
	{
		public override void SetDefaults()
		{
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(mod.GetTileEntity<TileEntities.BasicTeleporter>().Hook_AfterPlacement, -1, 0, false);

			base.SetDefaults();
		}

		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
		{
			TileEntities.Teleporter teleporter = Utility.GetTileEntity<TileEntities.Teleporter>(i, j);
			if (teleporter == null || !Main.tile[i, j].IsTopLeft()) return;

			Vector2 position = new Point16(i, j).ToScreenCoordinates();
			// todo: a dark texture when destination is not set and it is not receiving?
			spriteBatch.Draw(Teleportation.teleporterGlow[0], position + new Vector2(8, 2), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			base.KillMultiTile(i, j, frameX, frameY);

			Item.NewItem(i * 16, j * 16, 48, 16, mod.ItemType<Items.BasicTeleporter>());
		}
	}

	public class AdvancedTeleporter : Teleporter
	{
		public override void SetDefaults()
		{
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(mod.GetTileEntity<TileEntities.AdvancedTeleporter>().Hook_AfterPlacement, -1, 0, false);

			base.SetDefaults();
		}

		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
		{
			TileEntities.Teleporter teleporter = Utility.GetTileEntity<TileEntities.Teleporter>(i, j);
			if (teleporter == null || !Main.tile[i, j].IsTopLeft()) return;

			Vector2 position = new Point16(i, j).ToScreenCoordinates();

			spriteBatch.Draw(Teleportation.teleporterGlow[1], position + new Vector2(8, 2), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			base.KillMultiTile(i, j, frameX, frameY);

			Item.NewItem(i * 16, j * 16, 48, 16, mod.ItemType<Items.AdvancedTeleporter>());
		}
	}

	public class EliteTeleporter : Teleporter
	{
		public override void SetDefaults()
		{
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(mod.GetTileEntity<TileEntities.EliteTeleporter>().Hook_AfterPlacement, -1, 0, false);

			base.SetDefaults();
		}

		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
		{
			TileEntities.Teleporter teleporter = Utility.GetTileEntity<TileEntities.Teleporter>(i, j);
			if (teleporter == null || !Main.tile[i, j].IsTopLeft()) return;

			Vector2 position = new Point16(i, j).ToScreenCoordinates();

			spriteBatch.Draw(Teleportation.teleporterGlow[2], position + new Vector2(8, 2), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			base.KillMultiTile(i, j, frameX, frameY);

			Item.NewItem(i * 16, j * 16, 48, 16, mod.ItemType<Items.EliteTeleporter>());
		}
	}

	public class UltimateTeleporter : Teleporter
	{
		public override string Texture => "Teleportation/Textures/Tiles/UltimateTeleporter";

		public override void SetDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileSolidTop[Type] = false;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = false;
			Main.tileLavaDeath[Type] = false;
			Main.tileHammer[Type] = false;
			Main.tileLighted[Type] = true;

			TileObjectData.newTile.Width = 7;
			TileObjectData.newTile.Height = 2;
			TileObjectData.newTile.Origin = new Point16(0, 1);
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 7, 0);
			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.newTile.CoordinateHeights = new[] {16, 16};
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(mod.GetTileEntity<TileEntities.UltimateTeleporter>().Hook_AfterPlacement, -1, 0, false);
			TileObjectData.addTile(Type);
			disableSmartCursor = true;

			ModTranslation name = CreateMapEntryName();
			AddMapEntry(Color.Cyan, name);
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			TileEntities.Teleporter teleporter = Utility.GetTileEntity<TileEntities.Teleporter>(i, j);
			if (teleporter?.Destination == null || !Main.tile[i, j].IsTopLeft() ) return true;

			Vector2 position = new Point16(i + 1, j).ToScreenCoordinates();

			spriteBatch.Draw(Teleportation.teleporterEffect, position + new Vector2(8, 2), null, Color.White * 0.75f, 0f, new Vector2(10, 100), new Vector2(2, 1), SpriteEffects.None, 0f);

			return true;
		}

		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
		{
			TileEntities.Teleporter teleporter = Utility.GetTileEntity<TileEntities.Teleporter>(i, j);
			if (teleporter == null || !Main.tile[i, j].IsTopLeft()) return;

			Vector2 position = new Point16(i, j).ToScreenCoordinates();

			spriteBatch.Draw(Teleportation.teleporterGlow[3], position + new Vector2(8, 2), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			base.KillMultiTile(i, j, frameX, frameY);

			Item.NewItem(i * 16, j * 16, 112, 32, mod.ItemType<Items.UltimateTeleporter>());
		}
	}
}