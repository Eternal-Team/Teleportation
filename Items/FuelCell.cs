using BaseLibrary.Items;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace Teleportation.Items
{
	public class FuelCell : BaseItem
	{
		public override string Texture => "Teleportation/Textures/Items/FuelCell";

		public override void SetStaticDefaults()
		{
			Main.RegisterItemAnimation(item.type, new DrawAnimationVertical(5, 5));
		}

		public override void SetDefaults()
		{
			item.width = 20;
			item.height = 20;
			item.maxStack = 999;
			item.value = 100;
			item.rare = ItemRarityID.Cyan;
		}
	}
}