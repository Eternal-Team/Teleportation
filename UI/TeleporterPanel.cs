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

		public Teleporter SelectedTeleporter;

		private UIGrid<UITeleporterItem> gridLocations;
		private UIToggleButton[] buttonsWhitelist;
		private BaseElement panelMain;
		private BaseElement panelSettings;
		private BaseElement currentPanel;

		public override void OnInitialize()
		{
			Width = (0, 0.2f);
			Height = (0, 0.35f);
			this.Center();

			UITextInput inputName = new UITextInput(ref Container.DisplayName)
			{
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
				SelectedTeleporter = null;
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

			gridLocations.scrollbar.Height = (-16, 1);
			gridLocations.scrollbar.Top = (8, 0);
			gridLocations.scrollbar.HAlign = 1;
			panelLocations.Append(gridLocations.scrollbar);

			UITextButton buttonDialOnce = new UITextButton(Language.GetText("Mods.Teleportation.UI.DialOnce"))
			{
				Width = (-4, 0.25f),
				Height = (40, 0),
				VAlign = 1f
			};
			buttonDialOnce.OnClick += (evt, element) =>
			{
				if (SelectedTeleporter == null) return;

				if (SelectedTeleporter.Destination == Container) SelectedTeleporter = null;
				else
				{
					Container.Destination = SelectedTeleporter;
					Container.DialOnce = true;
					Net.SendTeleporterDestination(Container);
				}
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
				if (SelectedTeleporter == null) return;

				if (SelectedTeleporter.Destination == Container) SelectedTeleporter = null;
				else
				{
					Container.Destination = SelectedTeleporter;
					Container.DialOnce = false;
					Net.SendTeleporterDestination(Container);
				}
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
				if (SelectedTeleporter != null) SelectedTeleporter.Destination = null;
				SelectedTeleporter = null;
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
			for (int i = 0; i < buttonsWhitelist.Length; i++)
			{
				buttonsWhitelist[i] = new UIToggleButton(Teleportation.whitelist[i], ScaleMode.Zoom)
				{
					Size = new Vector2(40),
					Top = (36, 0),
					Left = (48 * i, 0),
					Padding = (6, 6, 6, 6),
					Toggled = Container.Whitelist[i],
					HoverText = Language.GetText($"Mods.Teleportation.UI.Whitelist_{i}")
				};
				int pos = i;
				buttonsWhitelist[i].OnClick += (evt, element) =>
				{
					Container.Whitelist[pos] = !Container.Whitelist[pos];
					Net.SendTeleporterWhitelist(Container);
				};
				panelSettings.Append(buttonsWhitelist[i]);
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
				HoverText = Language.GetText("Mods.Teleportation.UI.SetIcon").Format(Teleportation.Instance.ItemType<Pipette>())
			};
			buttonIcon.OnClick += (evt, element) =>
			{
				Pipette pipette = (Pipette)Main.mouseItem.modItem;
				if (pipette?.icon == null) return;

				Container.Icon = pipette.icon.Clone();

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