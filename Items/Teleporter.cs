using BaseLibrary.Items;

namespace Teleportation.Items
{
	public class Teleporter : BaseItem
	{
		public override string Texture => "Teleportation/Textures/Items/Teleporter";

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
	}
}