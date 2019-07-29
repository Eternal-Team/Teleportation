using BaseLibrary;
using BaseLibrary.UI;
using BaseLibrary.UI.Elements;
using ContainerLibrary;
using Microsoft.Xna.Framework;
using Teleportation.Items;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Teleporter = Teleportation.TileEntities.Teleporter;
using UltimateTeleporter = Teleportation.TileEntities.UltimateTeleporter;

namespace Teleportation.UI
{
	public class TeleporterPanel : BaseUIPanel<Teleporter>, IItemHandlerUI
	{
		public ItemHandler Handler => Container.Handler;
		public string GetTexture(Item item) => Teleportation.Instance.GetItem<Items.Teleporter>().Texture;

		private UIGrid<UITeleporterItem> gridLocations;

		private BaseElement panelMain;
		private BaseElement panelSettings;
		private BaseElement currentPanel;

		private UIToggleButton[] buttonsWhitelist;

		public Teleporter SelectedDestination;

		public override void OnInitialize()
		{
			Width = (0, 0.2f);
			Height = (0, 0.35f);
			this.Center();

			UITextInput inputName = new UITextInput(ref Container.DisplayName)
			{
				Width = (Container.DisplayName.ToString().Measure(Utility.Font).X, 0),
				Height = (20, 0),
				HAlign = 0.5f,
				MaxLength = 24,
				RenderPanel = false,
				HintText = "Teleporter",
				HorizontalAlignment = HorizontalAlignment.Center,
				SizeToText = true
			};
			inputName.OnTextChange += () => Net.SendTeleporterName(Container);
			Append(inputName);

			UITextButton buttonClose = new UITextButton("X")
			{
				Size = new Vector2(20),
				Left = (-20, 1),
				RenderPanel = false,
				Padding = (0, 0, 0, 0),
				HoverText = Language.GetText("Mods.BaseLibrary.UI.Close")
			};
			buttonClose.OnClick += (evt, element) => BaseLibrary.BaseLibrary.PanelGUI.UI.CloseUI(Container);
			Append(buttonClose);

			UITextButton buttonOptions = new UITextButton("O")
			{
				Size = new Vector2(20),
				RenderPanel = false,
				Padding = (0, 0, 0, 0),
				HoverText = Language.GetText("Mods.BaseLibrary.UI.Options")
			};
			buttonOptions.OnClick += (evt, element) =>
			{
				RemoveChild(currentPanel);
				currentPanel = currentPanel == panelSettings ? panelMain : panelSettings;
				Append(currentPanel);
			};
			Append(buttonOptions);

			currentPanel = new BaseElement
			{
				Width = (0, 1),
				Height = (-28, 1),
				Top = (28, 0)
			};
			Append(currentPanel);

			SetupMainPanel();
			SetupSettingsPanel();

			RemoveChild(currentPanel);
			currentPanel = panelMain;
			Append(currentPanel);
		}

		private void SetupMainPanel()
		{
			panelMain = new BaseElement
			{
				Width = (0, 1),
				Height = (-28, 1),
				Top = (28, 0)
			};

			UIPanel panelLocations = new UIPanel
			{
				Width = (0, 1),
				Height = (-48, 1)
			};
			panelMain.Append(panelLocations);

			gridLocations = new UIGrid<UITeleporterItem>
			{
				Width = (-28, 1),
				Height = (0, 1)
			};
			panelLocations.Append(gridLocations);
			UpdateGrid();

			UIScrollbar scrollbarLocations = new UIScrollbar
			{
				Height = (-16, 1),
				Top = (8, 0),
				HAlign = 1
			};
			gridLocations.SetScrollbar(scrollbarLocations);
			panelLocations.Append(scrollbarLocations);

			UITextButton buttonDialOnce = new UITextButton(Language.GetText("Mods.Teleportation.UI.DialOnce"))
			{
				Width = (-4, 0.25f),
				Height = (40, 0),
				VAlign = 1f
			};
			buttonDialOnce.OnClick += (evt, element) =>
			{
				Container.Destination = SelectedDestination;
				Container.DialOnce = true;
				Net.SendTeleporterDestination(Container);
			};
			panelMain.Append(buttonDialOnce);

			UITextButton buttonDial = new UITextButton(Language.GetText("Mods.Teleportation.UI.Dial"))
			{
				Width = (-4, 0.25f),
				Height = (40, 0),
				VAlign = 1f,
				HAlign = 1 / 3f
			};
			buttonDial.OnClick += (evt, element) =>
			{
				Container.Destination = SelectedDestination;
				Container.DialOnce = false;
				Net.SendTeleporterDestination(Container);
			};
			panelMain.Append(buttonDial);

			UITextButton buttonInterrupt = new UITextButton(Language.GetText("Mods.Teleportation.UI.Interrupt"))
			{
				Width = (-4, 0.25f),
				Height = (40, 0),
				HAlign = 2 / 3f,
				VAlign = 1f
			};
			buttonInterrupt.OnClick += (evt, element) =>
			{
				SelectedDestination = null;
				Container.Destination = null;
				Container.DialOnce = false;
				Net.SendTeleporterDestination(Container);
			};
			panelMain.Append(buttonInterrupt);

			UIContainerSlot slotFuel = new UIContainerSlot(() => Container.Handler)
			{
				Width = (-4, 0.25f),
				HAlign = 1f,
				VAlign = 1f,
				Padding = (0, 24, 24, 0),
				PreviewItem = new Item()
			};
			slotFuel.PreviewItem.SetDefaults(Teleportation.Instance.ItemType<FuelCell>());
			panelMain.Append(slotFuel);
		}

		private void SetupSettingsPanel()
		{
			panelSettings = new BaseElement
			{
				Width = (0, 1),
				Height = (-28, 1),
				Top = (28, 0)
			};

			UIText textWhitelist = new UIText(Language.GetText("Mods.BaseLibrary.UI.Whitelist"))
			{
				Top = (8, 0)
			};
			panelSettings.Append(textWhitelist);

			buttonsWhitelist = new UIToggleButton[Container is UltimateTeleporter ? 5 : 4];

			buttonsWhitelist[0] = new UIToggleButton(Teleportation.whitelistPlayer, ScaleMode.Zoom)
			{
				Size = new Vector2(40),
				Top = (36, 0),
				Padding = (6, 6, 6, 6),
				Toggled = Container.Whitelist[0],
				HoverText = Language.GetText("Mods.BaseLibrary.UI.Players")
			};
			buttonsWhitelist[0].OnClick += (evt, element) =>
			{
				Container.Whitelist[0] = !Container.Whitelist[0];
				Net.SendTeleporterWhitelist(Container);
			};
			panelSettings.Append(buttonsWhitelist[0]);

			buttonsWhitelist[1] = new UIToggleButton(Teleportation.whitelistNPC, ScaleMode.Zoom)
			{
				Size = new Vector2(40),
				Top = (36, 0),
				Left = (48, 0),
				Padding = (6, 6, 6, 6),
				Toggled = Container.Whitelist[1],
				HoverText = Language.GetText("Mods.BaseLibrary.UI.NPCs")
			};
			buttonsWhitelist[1].OnClick += (evt, element) =>
			{
				Container.Whitelist[1] = !Container.Whitelist[1];
				Net.SendTeleporterWhitelist(Container);
			};
			panelSettings.Append(buttonsWhitelist[1]);

			buttonsWhitelist[2] = new UIToggleButton(Teleportation.whitelistItem, ScaleMode.Zoom)
			{
				Size = new Vector2(40),
				Top = (36, 0),
				Left = (96, 0),
				Padding = (6, 6, 6, 6),
				Toggled = Container.Whitelist[2],
				HoverText = Language.GetText("Mods.BaseLibrary.UI.Items")
			};
			buttonsWhitelist[2].OnClick += (evt, element) =>
			{
				Container.Whitelist[2] = !Container.Whitelist[2];
				Net.SendTeleporterWhitelist(Container);
			};
			panelSettings.Append(buttonsWhitelist[2]);

			buttonsWhitelist[3] = new UIToggleButton(Teleportation.whitelistProjectile, ScaleMode.Zoom)
			{
				Size = new Vector2(40),
				Top = (36, 0),
				Left = (144, 0),
				Padding = (6, 6, 6, 6),
				Toggled = Container.Whitelist[3],
				HoverText = Language.GetText("Mods.BaseLibrary.UI.Projectiles")
			};
			buttonsWhitelist[3].OnClick += (evt, element) =>
			{
				Container.Whitelist[3] = !Container.Whitelist[3];
				Net.SendTeleporterWhitelist(Container);
			};
			panelSettings.Append(buttonsWhitelist[3]);

			if (Container is UltimateTeleporter)
			{
				buttonsWhitelist[4] = new UIToggleButton(Teleportation.whitelistBoss, ScaleMode.Zoom)
				{
					Size = new Vector2(40),
					Top = (36, 0),
					Left = (192, 0),
					Padding = (6, 6, 6, 6),
					Toggled = Container.Whitelist[4],
					HoverText = Language.GetText("Mods.BaseLibrary.UI.Bosses")
				};
				buttonsWhitelist[4].OnClick += (evt, element) =>
				{
					Container.Whitelist[4] = !Container.Whitelist[4];
					Net.SendTeleporterWhitelist(Container);
				};
				panelSettings.Append(buttonsWhitelist[4]);
			}

			UIText textIcon = new UIText(Language.GetText("Mods.BaseLibrary.UI.Icon"))
			{
				Top = (84, 0)
			};
			panelSettings.Append(textIcon);

			UIIcon buttonIcon = new UIIcon(Container)
			{
				Size = new Vector2(40),
				Top = (112, 0),
				Padding = (6, 6, 6, 6),
				HoverText = Language.GetTextValue("Mods.Teleportation.UI.SetIcon", Teleportation.Instance.ItemType<Pipette>())
			};
			buttonIcon.OnClick += (evt, element) =>
			{
				Pipette pipette = (Pipette)Main.mouseItem.modItem;
				if (pipette == null) return;

				if (pipette.EntityTexture != null) Container.EntityTexture = pipette.EntityTexture;
				if (pipette.EntityAnimation != null) Container.EntityAnimation = pipette.EntityAnimation;

				Net.SendTeleporterIcon(Container);

				Main.PlaySound(SoundID.MenuTick);
			};
			panelSettings.Append(buttonIcon);
		}

		public void UpdateGrid()
		{
			gridLocations.Clear();

			foreach (Teleporter teleporter in Container.GetConnections())
			{
				UITeleporterItem teleporterItem = new UITeleporterItem(teleporter, this)
				{
					Width = (0, 1),
					Height = (60, 0)
				};
				gridLocations.Add(teleporterItem);
			}
		}

		public void UpdateWhitelist()
		{
			for (int i = 0; i < buttonsWhitelist.Length; i++) buttonsWhitelist[i].Toggled = Container.Whitelist[i];
		}
	}
}