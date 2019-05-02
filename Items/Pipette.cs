using System.Collections.Generic;
using System.IO;
using System.Linq;
using BaseLibrary.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Teleportation.Items
{
	public class Pipette : BaseItem
	{
		public override string Texture => "Teleportation/Textures/Items/Pipette";

		public override bool CloneNewInstances => true;

		//public Type EntityType;
		//public int EntityID = -1;

		public Texture2D EntityTexture;

		public override ModItem Clone()
		{
			Pipette clone = (Pipette)base.Clone();
			//clone.EntityType = EntityType;
			//clone.EntityID = EntityID;
			return clone;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Pipette");
			Tooltip.SetDefault("Used to pick icons from items, NPCs, projectiles");
		}

		public override void SetDefaults()
		{
			item.width = 16;
			item.height = 16;
			item.maxStack = 1;
			item.UseSound = SoundID.Item1;
			item.autoReuse = true;
			item.useTurn = true;
			item.useStyle = 1;
			item.useTime = 15;
			item.useAnimation = 15;
			item.rare = ItemRarityID.White;
		}

		public override bool UseItem(Player player)
		{
			Point mouse = Main.MouseWorld.ToPoint();

			Entity entity = Main.item.FirstOrDefault(e => e.Hitbox.Contains(mouse)) ?? (Entity)Main.npc.FirstOrDefault(e => e.Hitbox.Contains(mouse)) ?? Main.projectile.FirstOrDefault(e => e.Hitbox.Contains(mouse));
			if (entity == null) return false;

			if (entity is Item i)
			{
				//EntityTexture = i.GetTexture();
				EntityTexture = Main.itemTexture[i.type];
				using (FileStream stream = new FileStream($"{ModLoader.ModPath}/test.png", FileMode.Create))
				{
					EntityTexture.SaveAsPng(stream, EntityTexture.Width, EntityTexture.Height);

					using (BinaryWriter writer = new BinaryWriter(stream))
					{
						if (Main.itemAnimations[i.type] != null)
						{
							DrawAnimation animation = Main.itemAnimations[i.type];
							writer.Write(animation.TicksPerFrame);
							writer.Write(animation.FrameCount);
						}
					}
				}

				using (FileStream stream = new FileStream($"{ModLoader.ModPath}/test.png", FileMode.Open))
				{
					EntityTexture = Texture2D.FromStream(Main.graphics.GraphicsDevice, stream);
					stream.Position += 4;
					using (BinaryReader reader = new BinaryReader(stream))
					{
						if (reader.PeekChar() != -1)
						{
							DrawAnimationVertical animation = new DrawAnimationVertical(reader.ReadInt32(), reader.ReadInt32());
							Main.NewText(animation.TicksPerFrame + " : " + animation.FrameCount);
						}
					}
				}
			}

			//EntityType = entity.GetType();
			//EntityID = entity.GetValue<int>("type");

			return true;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			//if (EntityType != null) tooltips.Add(new TooltipLine(mod, "PipetteInfo", ""));
		}

		public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
		{
			//if (line.Name == "PipetteInfo" && EntityType != null)
			//{
			//	Main.spriteBatch.DrawPanel(new Rectangle(line.X, line.Y, 56, 56));

			//	Entity entity = (Entity)Activator.CreateInstance(EntityType);
			//	entity.InvokeMethod<object>("SetDefaults", EntityID);

			//	Main.spriteBatch.DrawEntity(entity, new Vector2(line.X + 28, line.Y + 28), new Vector2(40));

			//	return false;
			//}

			return base.PreDrawTooltipLine(line, ref yOffset);
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.IronBar);
			recipe.AddIngredient(ItemID.Glass, 2);
			recipe.AddTile(TileID.WorkBenches);
			recipe.anyIronBar = true;
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}