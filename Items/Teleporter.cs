using BaseLibrary.Items;
using Terraria.ID;
using Terraria.ModLoader;

namespace Teleportation.Items
{
	public class Teleporter : BaseItem
	{
		public override string Texture => "Teleportation/Textures/Items/Teleporter";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Teleporter");
			Tooltip.SetDefault("Beam me up, Scotty!");
		}

		public override void SetDefaults()
		{
			item.width = 16;
			item.height = 16;
			item.maxStack = 99;
			item.useTurn = true;
			item.autoReuse = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.useStyle = 1;
			item.consumable = true;
			item.createTile = mod.TileType<Tiles.Teleporter>();
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.HallowedBar, 10);
			recipe.AddIngredient(ItemID.Wire, 50);
			//recipe.AddRecipeGroup(Teleportation.mechSouls.GetText.Invoke(), 2);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}