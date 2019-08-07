using BaseLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using Teleportation.TileEntities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ObjectData;

namespace Teleportation
{
	public static class Hooking
	{
		private static void Main_DrawMap(ILContext il)
		{
			ILCursor cursor = new ILCursor(il);

			if (Teleportation.Instance.GetConfig<TeleportationConfig>().ShowTeleportersOnMap) EditMinimapStyle2(ref cursor);

			EditMinimapStyle1(ref cursor);

			EditFullscreenMap(ref cursor);
		}

		private static void EditFullscreenMap(ref ILCursor cursor)
		{
			if (cursor.TryGotoNext(i => i.MatchLdcI4(0), i => i.MatchStloc(157)))
			{
				cursor.Emit(OpCodes.Ldloc, 1);
				cursor.Emit(OpCodes.Ldloc, 2);
				cursor.Emit(OpCodes.Ldloc, 17);
				cursor.Emit(OpCodes.Ldloc, 125);
				cursor.Emit(OpCodes.Ldloc, 0);

				cursor.EmitDelegate<Func<float, float, float, float, string, string>>((mapX, mapY, mapScale, scale, text) =>
				{
					float textureScale = scale * mapScale.Remap(0.1f, 16f, 0.5f, 1f);

					foreach (TileEntity tileEntity in TileEntity.ByID.Values)
					{
						if (tileEntity is Teleporter teleporter)
						{
							TileObjectData data = TileObjectData.GetTileData(Main.tile[teleporter.Position.X, teleporter.Position.Y]);
							Vector2 position = new Vector2(teleporter.Position.X, teleporter.Position.Y) * mapScale;

							position.X += mapX - 10f * mapScale;
							position.Y += mapY - 10f * mapScale;
							position.X += data.Width * 0.5f * mapScale;
							position.Y += data.Height * mapScale;

							Texture2D texture = Main.itemTexture[Teleportation.Instance.ItemType(teleporter.TileType.Name)];
							Vector2 origin = new Vector2(texture.Width * 0.5f, texture.Height);

							Main.spriteBatch.Draw(texture, position, null, Color.White, 0f, origin, textureScale, SpriteEffects.None, 0f);

							if (Main.mouseX >= position.X - origin.X * textureScale && Main.mouseX <= position.X + origin.X * textureScale && Main.mouseY >= position.Y - origin.Y * textureScale && Main.mouseY <= position.Y)
							{
								text = teleporter.DisplayName.Value;
							}
						}
					}

					return text;
				});

				cursor.Emit(OpCodes.Stloc, 0);
			}
		}

		private static void EditMinimapStyle1(ref ILCursor cursor)
		{
			ILLabel label = cursor.DefineLabel();

			if (cursor.TryGotoNext(i => i.MatchBleUn(out _), i => i.MatchLdcR4(1f), i => i.MatchStloc(84)))
			{
				cursor.Remove();
				cursor.Emit(OpCodes.Ble_Un, label);
			}

			if (cursor.TryGotoNext(i => i.MatchLdcI4(0), i => i.MatchStloc(85)))
			{
				cursor.MarkLabel(label);

				cursor.Emit(OpCodes.Ldloc, 13);
				cursor.Emit(OpCodes.Ldloc, 14);
				cursor.Emit(OpCodes.Ldloc, 17);
				cursor.Emit(OpCodes.Ldloc, 5);
				cursor.Emit(OpCodes.Ldloc, 84);
				cursor.Emit(OpCodes.Ldloc, 11);
				cursor.Emit(OpCodes.Ldloc, 12);
				cursor.Emit(OpCodes.Ldloc, 0);
				cursor.Emit(OpCodes.Ldloc, 3);
				cursor.Emit(OpCodes.Ldloc, 4);

				cursor.EmitDelegate<Func<float, float, float, byte, float, float, float, string, float, float, string>>((playerX, playerY, mapScale, alpha, scale, x, y, text, mapX, mapY) =>
				{
					Color color = new Color(alpha, alpha, alpha, alpha);
					scale *= 0.6f;

					foreach (TileEntity tileEntity in TileEntity.ByID.Values)
					{
						if (tileEntity is Teleporter teleporter)
						{
							TileObjectData data = TileObjectData.GetTileData(Main.tile[teleporter.Position.X, teleporter.Position.Y]);

							Vector2 position = new Vector2((teleporter.Position.X - playerX) * mapScale, (teleporter.Position.Y - (float)Math.Floor(playerY)) * mapScale);

							position.X += mapX;
							position.Y += mapY;
							position.X += data.Width * 0.5f * mapScale;
							position.Y += data.Height * mapScale;
							position.X += x;
							position.Y += y;

							if (position.X > Main.miniMapX + 12 && position.X < Main.miniMapX + Main.miniMapWidth - 16 && position.Y > Main.miniMapY + 10 && position.Y < Main.miniMapY + Main.miniMapHeight - 14)
							{
								Texture2D texture = Main.itemTexture[Teleportation.Instance.ItemType(teleporter.TileType.Name)];
								Vector2 origin = new Vector2(texture.Width * 0.5f, texture.Height);

								Main.spriteBatch.Draw(texture, position, null, color, 0f, origin, scale, SpriteEffects.None, 0f);

								if (Main.mouseX >= position.X - origin.X * scale && Main.mouseX <= position.X + origin.X * scale && Main.mouseY >= position.Y - origin.Y * scale && Main.mouseY <= position.Y)
								{
									text = "";
									Utility.DrawMouseText(teleporter.DisplayName.Value);
								}
							}
						}
					}

					return text;
				});

				cursor.Emit(OpCodes.Stloc, 0);
			}
		}

		private static void EditMinimapStyle2(ref ILCursor cursor)
		{
			if (cursor.TryGotoNext(i => i.MatchLdcI4(0), i => i.MatchStloc(64)))
			{
				cursor.Emit(OpCodes.Ldloc, 1);
				cursor.Emit(OpCodes.Ldloc, 2);
				cursor.Emit(OpCodes.Ldloc, 17);
				cursor.Emit(OpCodes.Ldloc, 63);
				cursor.Emit(OpCodes.Ldloc, 5);

				cursor.EmitDelegate<Action<float, float, float, float, byte>>((mapX, mapY, mapScale, scale, alpha) =>
				{
					float textureScale = scale * mapScale.Remap(0.1f, 16f, 0.5f, 1f);
					Color color = new Color(alpha, alpha, alpha, alpha);

					foreach (TileEntity tileEntity in TileEntity.ByID.Values)
					{
						if (tileEntity is Teleporter teleporter)
						{
							TileObjectData data = TileObjectData.GetTileData(Main.tile[teleporter.Position.X, teleporter.Position.Y]);
							Vector2 position = new Vector2(teleporter.Position.X, teleporter.Position.Y) * mapScale;

							position.X += mapX - 10f * mapScale;
							position.Y += mapY - 10f * mapScale;
							position.X += data.Width * 0.5f * mapScale;
							position.Y += data.Height * mapScale;

							Texture2D texture = Main.itemTexture[Teleportation.Instance.ItemType(teleporter.TileType.Name)];
							Main.spriteBatch.Draw(texture, position, null, color, 0f, new Vector2(texture.Width * 0.5f, texture.Height), textureScale, SpriteEffects.None, 0f);
						}
					}
				});
			}
		}

		internal static void Load()
		{
			IL.Terraria.Main.DrawMap += Main_DrawMap;
		}
	}
}