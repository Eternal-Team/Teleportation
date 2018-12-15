using System.Collections.Generic;
using System.Linq;
using Teleportation.TileEntities;
using Terraria;
using TheOneLibrary.Base.UI;

namespace Teleportation.UI
{
	public class TeleporterUI : BaseUI
	{
		public override void OnInitialize()
		{
		}

		public List<T> OfType<T>() => Elements.OfType<T>().ToList();

		public void HandleUI(TETeleporter te)
		{
			if (te.UI != null) CloseUI(te);
			else OpenUI(te);
		}

		public void CloseUI(TETeleporter te)
		{
			if (te.UI == null) return;

			te.UIPosition = te.UI.Position;
			Elements.Remove(te.UI);
			Main.PlaySound(TETeleporter.CloseSound);
		}

		public void OpenUI(TETeleporter te)
		{
			TeleporterPanel teUI = new TeleporterPanel
			{
				teleporter = te
			};
			teUI.Activate();
			if (te.UIPosition != null)
			{
				teUI.HAlign = teUI.VAlign = 0f;
				teUI.Position = te.UIPosition.Value;
			}

			Append(teUI);
			Main.PlaySound(TETeleporter.OpenSound);
		}
	}
}