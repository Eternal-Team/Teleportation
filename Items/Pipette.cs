using BaseLibrary;
using BaseLibrary.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
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

		public Texture2D EntityTexture;
		public DrawAnimation EntityAnimation;

		public override ModItem Clone()
		{
			Pipette clone = (Pipette)base.Clone();
			clone.EntityTexture = EntityTexture;
			clone.EntityAnimation = EntityAnimation;
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

			if (entity is Item item)
			{
				EntityTexture = Main.itemTexture[item.type];
				EntityAnimation = Main.itemAnimations[item.type];
			}
			else if (entity is NPC npc)
			{
				EntityTexture = Main.npcTexture[npc.type];
				EntityAnimation = new DrawAnimationVertical(5, Main.npcFrameCount[npc.type]);
			}
			else if (entity is Projectile projectile)
			{
				EntityTexture = Main.projectileTexture[projectile.type];
				EntityAnimation = new DrawAnimationVertical(5, Main.projFrames[projectile.type]);
			}

			return true;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			if (EntityTexture != null) tooltips.Add(new TooltipLine(mod, "PipetteInfo", ""));
		}

		public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
		{
			if (line.Name == "PipetteInfo")
			{
				Main.spriteBatch.DrawPanel(new Rectangle(line.X, line.Y, 56, 56));

				Main.spriteBatch.Draw(Utility.PointClampState, () =>
				{
					EntityAnimation?.Update();
					Rectangle rectangle = EntityAnimation?.GetFrame(EntityTexture) ?? EntityTexture.Frame();
					Main.spriteBatch.Draw(EntityTexture, new Vector2(line.X + 28, line.Y + 28), rectangle, Color.White, 0f, rectangle.Size() * 0.5f, Math.Min(40f / rectangle.Width, 40f / rectangle.Height), SpriteEffects.None, 0f);
				});

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