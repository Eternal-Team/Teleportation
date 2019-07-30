using BaseLibrary.Items;

namespace Teleportation.Items
{
	public abstract class Teleporter : BaseItem
	{
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
		}
	}

	public class BasicTeleporter : Teleporter
	{
		public override string Texture => "Teleportation/Textures/Items/BasicTeleporter";

		public override void SetDefaults()
		{
			base.SetDefaults();

			item.createTile = mod.TileType<Tiles.BasicTeleporter>();
		}
	}

	public class AdvancedTeleporter : Teleporter
	{
		public override string Texture => "Teleportation/Textures/Items/AdvancedTeleporter";

		public override void SetDefaults()
		{
			base.SetDefaults();

			item.createTile = mod.TileType<Tiles.AdvancedTeleporter>();
		}
	}

	public class EliteTeleporter : Teleporter
	{
		public override string Texture => "Teleportation/Textures/Items/EliteTeleporter";

		public override void SetDefaults()
		{
			base.SetDefaults();

			item.createTile = mod.TileType<Tiles.EliteTeleporter>();
		}
	}

	public class UltimateTeleporter : Teleporter
	{
		public override string Texture => "Teleportation/Textures/Items/UltimateTeleporter";

		public override void SetDefaults()
		{
			base.SetDefaults();

			item.createTile = mod.TileType<Tiles.UltimateTeleporter>();
		}
	}
}