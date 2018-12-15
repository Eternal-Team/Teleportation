using System;
using System.Collections.Generic;
using System.Linq;
using BaseLibrary.Items;
using BaseLibrary.Utility;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Teleportation.Items
{
	public class Pipette : BaseItem
	{
		public string entityType;
		public int entityID = -1;

		public override string Texture => "Teleportation/Textures/Items/Pipette";

		public override bool CloneNewInstances => true;

		public override ModItem Clone(Item item)
		{
			Pipette clone = (Pipette)base.Clone(item);
			clone.entityType = entityType;
			clone.entityID = entityID;
			return clone;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Pipette");
			Tooltip.SetDefault("Used to pick icons from items, NPCs, projectiles and dusts");
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
			item.rare = ItemRarityID.Lime;
		}

		public override bool UseItem(Player player)
		{
			Point mouse = Main.MouseWorld.ToPoint();

			Entity e = Main.item.Select(x => (Entity)x).Concat(Main.npc).Concat(Main.projectile).FirstOrDefault(x => x.InvokeMethod<Rectangle>("getRect").Contains(mouse));
			if (e != null)
			{
				Type type = e.GetType();
				entityType = type.Name;
				entityID = e.GetValue<int>("type");
			}

			return true;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			if (entityType != null && entityID > 0) tooltips.Add(new TooltipLine(mod, "PipetteInfo", $"Entity Type: {entityType}\nEntity ID: {entityID}"));
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