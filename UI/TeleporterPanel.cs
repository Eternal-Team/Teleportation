using System.Linq;
using BaseLibrary;
using BaseLibrary.UI;
using BaseLibrary.UI.Elements;
using Microsoft.Xna.Framework;
using Teleportation.Items;
using Teleportation.TileEntities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Teleportation.UI
{
	public class TeleporterPanel : BaseUIPanel<TETeleporter>
	{
		private UIGrid<UITeleporterItem> gridLocations;

		private BaseElement elementMain;
		private BaseElement elementSettings;

		public override void OnInitialize()
		{
			Width = (0, 0.25f);
			Height = (0, 0.4f);
			this.Center();
			SetPadding(8);

			// Top Panel
			{
				UIText textName = new UIText(Container.DisplayName.Value)
				{
					Width = (-56, 1),
					Height = (20, 0),
					HAlign = 0.5f
				};
				textName.GetHoverText += () => "Click to change name";
				Append(textName);

				UITextButton buttonClose = new UITextButton("X")
				{
					Size = new Vector2(20),
					Left = (-20, 1),
					RenderPanel = false,
					Padding = (0, 0, 0, 0)
				};
				buttonClose.GetHoverText += () => "Close";
				buttonClose.OnClick += (evt, element) => BaseLibrary.BaseLibrary.PanelGUI.UI.CloseUI(Container);
				Append(buttonClose);

				UITextButton buttonOptions = new UITextButton("O")
				{
					Size = new Vector2(20),
					RenderPanel = false,
					Padding = (0, 0, 0, 0)
				};
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
			}

			// Middle Panel
			{
				// Main
				{
					elementMain = new BaseElement
					{
						Width = (0, 1),
						Height = (-28, 1),
						Top = (28, 0)
					};
					Append(elementMain);

					UIPanel panelLocations = new UIPanel
					{
						Width = (0, 1),
						Height = (-48, 1)
					};
					elementMain.Append(panelLocations);

					gridLocations = new UIGrid<UITeleporterItem>
					{
						Width = (0, 1),
						Height = (0, 1)
					};
					panelLocations.Append(gridLocations);
					PopulateGrid();
				}

				// Options
				{
					elementSettings = new BaseElement
					{
						Width = (0, 1),
						Height = (-28, 1),
						Top = (28, 0)
					};

					UIText textWhitelist = new UIText("Whitelist")
					{
						Top = (8, 0)
					};
					elementSettings.Append(textWhitelist);

					UIToggleButton buttonPlayers = new UIToggleButton(ModContent.GetTexture("Teleportation/Textures/Whitelist_Player"), ScaleMode.Zoom)
					{
						Size = new Vector2(40),
						Top = (36, 0),
						Padding = (6, 6, 6, 6),
						Toggled = Container.Whitelist[0]
					};
					buttonPlayers.GetHoverText += () => "Players";
					buttonPlayers.OnClick += (a, b) => Container.Whitelist[0] = !Container.Whitelist[0];
					elementSettings.Append(buttonPlayers);

					UIToggleButton buttonNPCs = new UIToggleButton(ModContent.GetTexture("Teleportation/Textures/Whitelist_NPC"), ScaleMode.Zoom)
					{
						Size = new Vector2(40),
						Top = (36, 0),
						Left = (48, 0),
						Padding = (6, 6, 6, 6),
						Toggled = Container.Whitelist[1]
					};
					buttonNPCs.GetHoverText += () => "NPCs";
					buttonNPCs.OnClick += (a, b) => Container.Whitelist[1] = !Container.Whitelist[1];
					elementSettings.Append(buttonNPCs);

					UIToggleButton buttonItems = new UIToggleButton(ModContent.GetTexture("Teleportation/Textures/Whitelist_Item"), ScaleMode.Zoom)
					{
						Size = new Vector2(40),
						Top = (36, 0),
						Left = (96, 0),
						Padding = (6, 6, 6, 6),
						Toggled = Container.Whitelist[2]
					};
					buttonItems.GetHoverText += () => "Items";
					buttonItems.OnClick += (a, b) => Container.Whitelist[2] = !Container.Whitelist[2];
					elementSettings.Append(buttonItems);

					UIToggleButton buttonProjectiles = new UIToggleButton(ModContent.GetTexture("Teleportation/Textures/Whitelist_Projectile"), ScaleMode.Zoom)
					{
						Size = new Vector2(40),
						Top = (36, 0),
						Left = (144, 0),
						Padding = (6, 6, 6, 6),
						Toggled = Container.Whitelist[3]
					};
					buttonProjectiles.GetHoverText += () => "Projectiles";
					buttonProjectiles.OnClick += (a, b) => Container.Whitelist[3] = !Container.Whitelist[3];
					elementSettings.Append(buttonProjectiles);

					UIText textIcon = new UIText("Icon")
					{
						Top = (84, 0)
					};
					elementSettings.Append(textIcon);

					UIIcon buttonIcon = new UIIcon(Container)
					{
						Size = new Vector2(40),
						Top = (112, 0),
						Padding = (6, 6, 6, 6)
					};
					buttonIcon.GetHoverText += () => $"Click with a [item:{Teleportation.Instance.ItemType<Pipette>()}] to set icon";
					buttonIcon.OnClick += (evt, element) =>
					{
						Pipette pipette = (Pipette)Main.mouseItem.modItem;
						if (pipette == null) return;

						if (pipette.EntityTexture != null) Container.EntityTexture = pipette.EntityTexture;
						Container.EntityAnimation = pipette.EntityAnimation != null ? new DrawAnimationVertical(pipette.EntityAnimation.TicksPerFrame, pipette.EntityAnimation.FrameCount) : null;

						Main.PlaySound(SoundID.MenuTick);
					};
					elementSettings.Append(buttonIcon);
				}
			}

			// Bottom Panel
			{
				UITextButton buttonDialOnce = new UITextButton("Dial Once")
				{
					Width = (-4, 1 / 3f),
					Height = (40, 0),
					Top = (-40, 1)
				};
				buttonDialOnce.OnClick += (evt, element) =>
				{
					Container.Destination = gridLocations.items.FirstOrDefault(item => item.Selected)?.teleporter;
					Container.dialOnce = true;
				};
				elementMain.Append(buttonDialOnce);

				UITextButton buttonDial = new UITextButton("Dial")
				{
					Width = (-4, 1 / 3f),
					Height = (40, 0),
					Top = (-40, 1),
					HAlign = 0.5f
				};
				buttonDial.OnClick += (evt, element) =>
				{
					Container.Destination = gridLocations.items.FirstOrDefault(item => item.Selected)?.teleporter;
					Container.dialOnce = false;
				};
				elementMain.Append(buttonDial);

				UITextButton buttonInterrupt = new UITextButton("Interrupt")
				{
					Width = (-4, 1 / 3f),
					Height = (40, 0),
					Top = (-40, 1),
					HAlign = 1f
				};
				buttonInterrupt.OnClick += (evt, element) =>
				{
					Container.Destination = null;
					gridLocations.items.ForEach(item => item.Selected = false);
					Container.dialOnce = false;
				};
				elementMain.Append(buttonInterrupt);
			}
		}

		public void PopulateGrid()
		{
			gridLocations.Clear();

			foreach (TETeleporter teleporter in TileEntity.ByID.Values.Where(te => te is TETeleporter && te != Container))
			{
				UITeleporterItem teleporterItem = new UITeleporterItem(teleporter)
				{
					Width = (0, 1),
					Height = (60, 0),
					Selected = Container.Destination == teleporter
				};
				gridLocations.Add(teleporterItem);
			}
		}
	}
}