using BaseLibrary.UI.Elements;
using BaseLibrary.UI.New;
using ContainerLibrary;
using Microsoft.Xna.Framework;
using Teleportation.Items;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Teleportation.UI
{
	public class TeleporterPanel : BaseUIPanel<TileEntities.Teleporter>, IItemHandlerUI
	{
		public ItemHandler Handler => Container.Handler;
		public string GetTexture(Item item) => ModContent.GetInstance<Teleporter>().Texture;

		public TileEntities.Teleporter SelectedTeleporter;

		private UIGrid<UITeleporterItem> gridLocations;
		private UIToggleButton[] buttonsWhitelist;
		private BaseElement panelMain;
		private BaseElement panelSettings;
		private BaseElement currentPanel;

		public TeleporterPanel(TileEntities.Teleporter teleporter) : base(teleporter)
		{
			Width.Percent = 20;
			Height.Percent = 35;

			UITextInput inputName = new UITextInput(ref Container.DisplayName)
			{
				X = { Percent = 50 },
				MaxLength = 24,
				RenderPanel = false,
				HintText = "Teleporter",
				HorizontalAlignment = HorizontalAlignment.Center,
				SizeToText = true
			};
			inputName.OnTextChange += () => Net.SendTeleporterName(Container);
			Add(inputName);

			UITextButton buttonClose = new UITextButton("X")
			{
				Size = new Vector2(20),
				X = { Pixels = -20, Percent = 100 },
				RenderPanel = false,
				Padding = Padding.Zero,
				HoverText = Language.GetText("Mods.BaseLibrary.UI.Close")
			};
			buttonClose.OnClick += args => PanelUI.Instance.CloseUI(Container);
			Add(buttonClose);

			UITextButton buttonOptions = new UITextButton("O")
			{
				Size = new Vector2(20),
				RenderPanel = false,
				Padding = Padding.Zero,
				HoverText = Language.GetText("Mods.BaseLibrary.UI.Options")
			};
			buttonOptions.OnClick += args =>
			{
				Remove(currentPanel);
				SelectedTeleporter = null;
				currentPanel = currentPanel == panelSettings ? panelMain : panelSettings;
				Add(currentPanel);
			};
			Add(buttonOptions);

			currentPanel = new BaseElement
			{
				Width = { Percent = 100 },
				Height = { Pixels = -28, Percent = 100 },
				Y = { Pixels = 28 }
			};

			Add(currentPanel);
			SetupMainPanel();
			SetupSettingsPanel();
			Remove(currentPanel);
			currentPanel = panelMain;
			Add(currentPanel);
		}

		private void SetupMainPanel()
		{
			panelMain = new BaseElement
			{
				Width = { Percent = 100 },
				Height = { Pixels = -28, Percent = 100 },
				Y = { Pixels = 28 }
			};

			UIPanel panelLocations = new UIPanel
			{
				Width = { Percent = 100 },
				Height = { Pixels = -48, Percent = 100 }
			};
			panelMain.Add(panelLocations);

			gridLocations = new UIGrid<UITeleporterItem>
			{
				Width = { Pixels = -28, Percent = 100 },
				Height = { Percent = 100 }
			};
			panelLocations.Add(gridLocations);
			UpdateGrid();

			gridLocations.scrollbar.Height = new StyleDimension { Percent = 100 };
			gridLocations.scrollbar.X.Percent = 100;
			panelLocations.Add(gridLocations.scrollbar);

			UITextButton buttonDialOnce = new UITextButton(Language.GetText("Mods.Teleportation.UI.DialOnce"))
			{
				Width = { Pixels = -4, Percent = 25 },
				Height = { Pixels = 40 },
				Y = { Percent = 100 }
			};
			buttonDialOnce.OnClick += args =>
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
			panelMain.Add(buttonDialOnce);

			UITextButton buttonDial = new UITextButton(Language.GetText("Mods.Teleportation.UI.Dial"))
			{
				Width = { Pixels = -4, Percent = 25 },
				Height = { Pixels = 40 },
				Y = { Percent = 100 },
				X = { Percent = 33 }
			};
			buttonDial.OnClick += args =>
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
			panelMain.Add(buttonDial);

			UITextButton buttonInterrupt = new UITextButton(Language.GetText("Mods.Teleportation.UI.Interrupt"))
			{
				Width = { Pixels = -4, Percent = 25 },
				Height = { Pixels = 40 },
				X = { Percent = 50 },
				Y = { Percent = 100 }
			};
			buttonInterrupt.OnClick += args =>
			{
				if (SelectedTeleporter != null) SelectedTeleporter.Destination = null;
				SelectedTeleporter = null;
				Container.Destination = null;
				Container.DialOnce = false;
				Net.SendTeleporterDestination(Container);
			};
			panelMain.Add(buttonInterrupt);

			UIContainerSlot slotFuel = new UIContainerSlot(() => Container.Handler)
			{
				Width = { Pixels = -4, Percent = 25 },
				X = { Percent = 100 },
				Y = { Percent = 100 },
				Padding = new Padding(0, 24, 24, 0),
				PreviewItem = new Item()
			};
			slotFuel.PreviewItem.SetDefaults(ModContent.ItemType<FuelCell>());
			panelMain.Add(slotFuel);
		}

		private void SetupSettingsPanel()
		{
			panelSettings = new BaseElement
			{
				Width = { Percent = 100 },
				Height = { Pixels = -28, Percent = 100 },
				Y = { Pixels = 28 }
			};

			UIText textWhitelist = new UIText(Language.GetText("Mods.BaseLibrary.UI.Whitelist"))
			{
				Y = { Pixels = 8 }
			};
			panelSettings.Add(textWhitelist);

			buttonsWhitelist = new UIToggleButton[Container is TileEntities.UltimateTeleporter ? 5 : 4];
			for (int i = 0; i < buttonsWhitelist.Length; i++)

			{
				buttonsWhitelist[i] = new UIToggleButton(Teleportation.whitelist[i], BaseLibrary.UI.ScaleMode.Zoom)
				{
					Size = new Vector2(40),
					Y = { Pixels = 36 },
					X = { Pixels = 48 * i },
					Padding = new Padding(6, 6, 6, 6),
					Toggled = Container.Whitelist[i],
					HoverText = Language.GetText($"Mods.Teleportation.UI.Whitelist_{i}")
				};
				int pos = i;
				buttonsWhitelist[i].OnClick += args =>
				{
					Container.Whitelist[pos] = !Container.Whitelist[pos];
					Net.SendTeleporterWhitelist(Container);
				};
				panelSettings.Add(buttonsWhitelist[i]);
			}

			UIText textIcon = new UIText(Language.GetText("Mods.BaseLibrary.UI.Icon"))
			{
				Y = { Pixels = 84 }
			};

			panelSettings.Add(textIcon);
			UIIcon buttonIcon = new UIIcon(Container)
			{
				Size = new Vector2(40),
				Y = { Pixels = 112 },
				Padding = new Padding(6, 6, 6, 6),
				HoverText = Language.GetText("Mods.Teleportation.UI.SetIcon").Format(ModContent.ItemType<Pipette>())
			};
			buttonIcon.OnClick += args =>
			{
				Pipette pipette = (Pipette)Main.mouseItem.modItem;
				if (pipette?.icon == null) return;

				Container.Icon = pipette.icon.Clone();

				Net.SendTeleporterIcon(Container);

				Main.PlaySound(SoundID.MenuTick);
			};
			panelSettings.Add(buttonIcon);
		}

		public void UpdateGrid()
		{
			gridLocations.Clear();

			foreach (TileEntities.Teleporter teleporter in Container.GetConnections())
			{
				UITeleporterItem teleporterItem = new UITeleporterItem(teleporter, this)
				{
					Width = { Percent = 100 },
					Height = { Pixels = 60 }
				};
				gridLocations.Add(teleporterItem);
			}
		}

		internal void UpdateWhitelist()
		{
			for (int i = 0; i < buttonsWhitelist.Length; i++) buttonsWhitelist[i].Toggled = Container.Whitelist[i];
		}
	}
}