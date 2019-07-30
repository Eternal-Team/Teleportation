using BaseLibrary;
using BaseLibrary.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Teleportation.TileEntities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Teleportation.Items
{
	public class Pipette : BaseItem
	{
		public override string Texture => "Teleportation/Textures/Items/Pipette";

		public override bool CloneNewInstances => true;

		public IconData icon;

		public override ModItem Clone()
		{
			Pipette clone = (Pipette)base.Clone();
			clone.icon = icon?.Clone();
			return clone;
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
			Entity entity = Main.MouseWorld.GetEntityAtPos();
			if (entity == null) return false;

			if (entity is Item item) icon = new IconData(IconData.Type.Item, item.type);
			else if (entity is NPC npc) icon = new IconData(IconData.Type.NPC, npc.type);
			else if (entity is Projectile projectile) icon = new IconData(IconData.Type.Projectile, projectile.type);

			return true;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			if (icon != null) tooltips.Add(new TooltipLine(mod, "PipetteInfo", ""));
		}

		public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
		{
			if (line.Name == "PipetteInfo")
			{
				Main.spriteBatch.DrawPanel(new Rectangle(line.X, line.Y, 56, 56));

				int offset = 0;
				Main.spriteBatch.Draw(Utility.PointClampState, () =>
				{
					icon.animation.Update();
					Rectangle rectangle = icon.animation.GetFrame(icon.Texture);
					float scale = Math.Min(40f / rectangle.Width, 40f / rectangle.Height);
					Main.spriteBatch.Draw(icon.Texture, new Vector2(line.X + 28, line.Y + 28), rectangle, Color.White, 0f, rectangle.Size() * 0.5f, scale, SpriteEffects.None, 0f);
					offset += line.Y + 56;
				});
				yOffset += offset;

				return false;
			}

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