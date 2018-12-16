using System.Linq;
using BaseLibrary.UI;
using BaseLibrary.UI.Elements;
using BaseLibrary.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Teleportation.Items;
using Teleportation.TileEntities;
using Terraria;
using Terraria.DataStructures;

namespace Teleportation.UI
{
	public class TeleporterPanel : UIDraggablePanel
	{
		[PathOverride("Teleportation/Textures/Whitelist_Projectile")]
		public static Texture2D WhitelistProjectile { get; set; }

		[PathOverride("Teleportation/Textures/Whitelist_Player")]
		public static Texture2D WhitelistPlayer { get; set; }

		[PathOverride("Teleportation/Textures/Whitelist_Item")]
		public static Texture2D WhitelistItem { get; set; }

		[PathOverride("Teleportation/Textures/Whitelist_NPC")]
		public static Texture2D WhitelistNPC { get; set; }

		public TETeleporter teleporter;

		private UITextInput inputDisplayName;
		private UITextButton buttonClose;
		private UITextButton buttonOptions;

		private UIPanel panelLocations;
		private UIGrid<UITeleporterItem> gridLocations;

		private UITextButton buttonDialOnce;
		private UITextButton buttonDial;
		private UITextButton buttonInterrupt;

		private BaseElement elementMain;
		private BaseElement elementSettings;

		public override void OnInitialize()
		{
			Width = (0, 0.25f);
			Height = (0, 0.4f);
			this.Center();
			SetPadding(8);

			inputDisplayName = new UITextInput
			{
				Width = (-56, 1),
				Height = (20, 0),
				HAlign = 0.5f,
				Text = teleporter.DisplayName
			};
			inputDisplayName.GetHoverText += () => "Click to change name";
			Append(inputDisplayName);

			buttonClose = new UITextButton("X")
			{
				Size = new Vector2(20),
				Left = (-20, 1),
				RenderPanel = false
			};
			buttonClose.SetPadding(0);
			buttonClose.GetHoverText += () => "Close";
			buttonClose.OnClick += (evt, element) => Teleportation.Instance.TeleporterUI.UI.CloseUI(teleporter);
			Append(buttonClose);

			buttonOptions = new UITextButton("O")
			{
				Size = new Vector2(20),
				RenderPanel = false
			};
			buttonOptions.SetPadding(0);
			buttonOptions.GetHoverText += () => "Options";
			buttonOptions.OnClick += (evt, element) =>
			{
				if (HasChild(elementMain))
				{
					RemoveChild(elementMain);
					Append(elementSettings);
				}
				else
				{
					RemoveChild(elementSettings);
					Append(elementMain);
				}
			};
			Append(buttonOptions);

			elementMain = new BaseElement
			{
				Width = (0, 1),
				Height = (-28, 1),
				Top = (28, 0)
			};

			#region Content
			panelLocations = new UIPanel
			{
				Width = (0, 1),
				Height = (-48, 1)
			};
			panelLocations.SetPadding(8);
			elementMain.Append(panelLocations);

			gridLocations = new UIGrid<UITeleporterItem>
			{
				Width = (0, 1),
				Height = (0, 1)
			};
			panelLocations.Append(gridLocations);

			foreach (TETeleporter teTeleporter in TileEntity.ByID.Values.OfType<TETeleporter>())
			{
				if (teTeleporter != teleporter)
				{
					UITeleporterItem teleporterItem = new UITeleporterItem(teTeleporter)
					{
						Width = (0, 1),
						Height = (60, 0),
						Selected = teleporter.destination == teTeleporter
					};
					gridLocations.Add(teleporterItem);
				}
			}

			buttonDialOnce = new UITextButton("Dial Once")
			{
				Width = (-4, 1 / 3f),
				Height = (40, 0),
				Top = (-40, 1)
			};
			buttonDialOnce.OnClick += (evt, element) =>
			{
				teleporter.destination = gridLocations.items.FirstOrDefault(item => item.Selected)?.teleporter;
				teleporter.dialOnce = true;
			};
			elementMain.Append(buttonDialOnce);

			buttonDial = new UITextButton("Dial")
			{
				Width = (-4, 1 / 3f),
				Height = (40, 0),
				Top = (-40, 1),
				HAlign = 0.5f
			};
			buttonDial.OnClick += (evt, element) =>
			{
				teleporter.destination = gridLocations.items.FirstOrDefault(item => item.Selected)?.teleporter;
				teleporter.dialOnce = false;
			};
			elementMain.Append(buttonDial);

			buttonInterrupt = new UITextButton("Interrupt")
			{
				Width = (-4, 1 / 3f),
				Height = (40, 0),
				Top = (-40, 1),
				HAlign = 1f
			};
			buttonInterrupt.OnClick += (evt, element) =>
			{
				teleporter.destination = null;
				gridLocations.items.ForEach(item => item.Selected = false);
				teleporter.dialOnce = false;
			};
			elementMain.Append(buttonInterrupt);
			#endregion

			Append(elementMain);

			elementSettings = new BaseElement
			{
				Width = (0, 1),
				Height = (-28, 1),
				Top = (28, 0)
			};

			#region Content
			UIText textWhitelist = new UIText("Whitelist")
			{
				Top = (8, 0)
			};
			elementSettings.Append(textWhitelist);

			UIToggleButton buttonPlayers = new UIToggleButton(WhitelistPlayer, ScaleMode.Zoom)
			{
				Size = new Vector2(40),
				Top = (36, 0),
				Padding = (5, 5, 5, 5),
				Toggled = teleporter.Whitelist[0]
			};
			buttonPlayers.GetHoverText += () => "Players";
			buttonPlayers.OnClick += (a, b) => teleporter.Whitelist[0] = !teleporter.Whitelist[0];
			elementSettings.Append(buttonPlayers);

			UIToggleButton buttonNPCs = new UIToggleButton(WhitelistNPC, ScaleMode.Zoom)
			{
				Size = new Vector2(40),
				Top = (36, 0),
				Left = (48, 0),
				Padding = (5, 5, 5, 5),
				Toggled = teleporter.Whitelist[1]
			};
			buttonNPCs.GetHoverText += () => "NPCs";
			buttonNPCs.OnClick += (a, b) => teleporter.Whitelist[1] = !teleporter.Whitelist[1];
			elementSettings.Append(buttonNPCs);

			UIToggleButton buttonItems = new UIToggleButton(WhitelistItem, ScaleMode.Zoom)
			{
				Size = new Vector2(40),
				Top = (36, 0),
				Left = (96, 0),
				Padding = (5, 5, 5, 5),
				Toggled = teleporter.Whitelist[2]
			};
			buttonItems.GetHoverText += () => "Items";
			buttonItems.OnClick += (a, b) => teleporter.Whitelist[2] = !teleporter.Whitelist[2];
			elementSettings.Append(buttonItems);

			UIToggleButton buttonProjectiles = new UIToggleButton(WhitelistProjectile, ScaleMode.Zoom)
			{
				Size = new Vector2(40),
				Top = (36, 0),
				Left = (144, 0),
				Padding = (5, 5, 5, 5),
				Toggled = teleporter.Whitelist[3]
			};
			buttonProjectiles.GetHoverText += () => "Projectiles";
			buttonProjectiles.OnClick += (a, b) => teleporter.Whitelist[3] = !teleporter.Whitelist[3];
			elementSettings.Append(buttonProjectiles);

			UIText textIcon = new UIText("Icon")
			{
				Top = (84, 0)
			};
			elementSettings.Append(textIcon);

			UIButton buttonIcon = new UIButton(Main.itemTexture[Teleportation.Instance.ItemType<Teleporter>()], ScaleMode.Zoom)
			{
				Size = new Vector2(40),
				Top = (112, 0),
				Padding = (5, 5, 5, 5),
				RenderPanel = true
			};
			buttonIcon.GetHoverText += () => $"Click with a [item:{Teleportation.Instance.ItemType<Pipette>()}] to set icon";
			buttonIcon.OnClick += (evt, element) =>
			{
				Pipette pipette = (Pipette)Main.mouseItem.modItem;
				if (pipette == null) return;

				if (pipette.entityType != null && pipette.entityID > 0)
				{
					teleporter.entityType = pipette.entityType;
					teleporter.entityID = pipette.entityID;
				}
			};
			elementSettings.Append(buttonIcon);
			#endregion

			// bar for the fuel?

			#region Bottom Panel
			//slotFuel = new UIContainerSlot(teleporter);
			//slotFuel.CanInteract += (current, mouse) => mouse.IsAir || mouse.type == Teleportation.Instance.ItemType<FuelCell>();
			//slotFuel.Left.Set(-48, 1);
			//slotFuel.Top.Set(-48, 1);
			//panelMain.Append(slotFuel);
			#endregion
		}

		//public void InitializeWhitelist()
		//{
		//	panelWhitelist.Width.Pixels = 200;
		//	panelWhitelist.Height.Pixels = 200;
		//	panelWhitelist.GetX += () =>
		//	{
		//		CalculatedStyle dim = panelMain.GetDimensions();
		//		return dim.X + dim.Width + panelWhitelist.Width.Pixels <= Main.screenWidth ? dim.X + dim.Width : dim.X - panelWhitelist.Width.Pixels;
		//	};
		//	panelWhitelist.GetY += () => panelMain.GetDimensions().Y;
		//	panelWhitelist.SetPadding(0);

		//	for (int i = 0; i < 4; i++)
		//	{
		//		UITextButton button = new UITextButton(Teleportation.WhitelistLocal[i], 7);
		//		button.Width.Set(-16, 1);
		//		button.Height.Pixels = 40;
		//		button.Left.Pixels = 8;
		//		button.Top.Pixels = 8 * (i + 1) + 40 * i;
		//		int index = i;
		//		button.PanelColor = () => teleporter.whitelist[index] ? BaseLibrary.Utility.Utility.PanelColor_Selected : Utility.PanelColor;
		//		button.OnClick += (a, b) =>
		//		{
		//			teleporter.whitelist[index] = !teleporter.whitelist[index];

		//			teleporter.Sync();
		//		};
		//		panelWhitelist.Append(button);
		//	}
		//}

		//public override void Load()
		//{
		//	inputName.currentString = teleporter.name;

		//	gridLocations.Clear();
		//	foreach (KeyValuePair<int, TileEntity> pair in TileEntity.ByID.Where(x => x.Value is TETeleporter && x.Value != teleporter).OrderByDescending(x => ((TETeleporter)x.Value).name))
		//	{
		//		UITeleporterItem teleporterEntry = new UITeleporterItem((TETeleporter)pair.Value);
		//		teleporterEntry.Width.Set(0, 1);
		//		teleporterEntry.Height.Set(60, 0);
		//		teleporterEntry.Selected = () => pair.Value.Position == teleporter.destinationTeleporter;
		//		teleporterEntry.OnClick += (a, b) =>
		//		{
		//			teleporter.destinationTeleporter = teleporterEntry.Selected.Invoke() ? Point16.NegativeOne : teleporterEntry.teleporter.Position;

		//			teleporter.Sync();
		//		};
		//		gridLocations.Add(teleporterEntry);
		//	}
		//}
	}
}