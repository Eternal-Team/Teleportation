using BaseLibrary;
using BaseLibrary.UI;
using System.IO;
using Teleportation.TileEntities;
using Teleportation.UI;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace Teleportation
{
	internal static class Net
	{
		internal enum PacketType : byte
		{
			SyncTeleporterItems,
			SyncTeleporterName,
			SyncTeleporterDestination,
			SyncTeleporterIcon,
			SyncTeleporterWhitelist,
			CloseUI
		}

		internal static ModPacket GetPacket(PacketType packetType)
		{
			ModPacket packet = Teleportation.Instance.GetPacket();
			packet.Write((byte)packetType);
			return packet;
		}

		internal static void HandlePacket(BinaryReader reader, int whoAmI)
		{
			PacketType packetType = (PacketType)reader.ReadByte();

			switch (packetType)
			{
				case PacketType.SyncTeleporterItems:
					ReceiveTeleporterItems(reader, whoAmI);
					break;
				case PacketType.SyncTeleporterName:
					ReceiveTeleporterName(reader, whoAmI);
					break;
				case PacketType.SyncTeleporterDestination:
					ReceiveTeleporterDestination(reader, whoAmI);
					break;
				case PacketType.SyncTeleporterIcon:
					ReceiveTeleporterIcon(reader, whoAmI);
					break;
				case PacketType.SyncTeleporterWhitelist:
					ReceiveTeleporterWhitelist(reader, whoAmI);
					break;
				case PacketType.CloseUI:
					ReceiveCloseUI(reader, whoAmI);
					break;
			}
		}

		internal static void SendTeleporterItems(Teleporter teleporter, int ignoreClient = -1)
		{
			if (Main.netMode == NetmodeID.SinglePlayer) return;

			ModPacket packet = GetPacket(PacketType.SyncTeleporterItems);
			packet.Write(teleporter.ID);
			teleporter.Handler.Write(packet);
			packet.Send(ignoreClient: ignoreClient);
		}

		private static void ReceiveTeleporterItems(BinaryReader reader, int whoAmI)
		{
			Teleporter teleporter = (Teleporter)TileEntity.ByID[reader.ReadInt32()];
			teleporter.Handler.Read(reader);

			if (Main.netMode == NetmodeID.Server) SendTeleporterItems(teleporter, whoAmI);
		}

		internal static void SendTeleporterName(Teleporter teleporter, int ignoreClient = -1)
		{
			if (Main.netMode == NetmodeID.SinglePlayer) return;

			ModPacket packet = GetPacket(PacketType.SyncTeleporterName);
			packet.Write(teleporter.ID);
			packet.Write(teleporter.DisplayName.Value);
			packet.Send(ignoreClient: ignoreClient);
		}

		private static void ReceiveTeleporterName(BinaryReader reader, int whoAmI)
		{
			Teleporter teleporter = (Teleporter)TileEntity.ByID[reader.ReadInt32()];
			teleporter.DisplayName.Value = reader.ReadString();

			if (Main.netMode == NetmodeID.Server) SendTeleporterName(teleporter, whoAmI);
		}

		internal static void SendTeleporterDestination(Teleporter teleporter, int ignoreClient = -1)
		{
			if (Main.netMode == NetmodeID.SinglePlayer) return;

			ModPacket packet = GetPacket(PacketType.SyncTeleporterName);
			packet.Write(teleporter.ID);
			packet.Write(teleporter._destination);
			packet.Send(ignoreClient: ignoreClient);
		}

		private static void ReceiveTeleporterDestination(BinaryReader reader, int whoAmI)
		{
			Teleporter teleporter = (Teleporter)TileEntity.ByID[reader.ReadInt32()];
			teleporter._destination = reader.ReadPoint16();

			if (Main.netMode == NetmodeID.Server) SendTeleporterDestination(teleporter, whoAmI);
		}

		internal static void SendTeleporterIcon(Teleporter teleporter, int ignoreClient = -1)
		{
			if (Main.netMode == NetmodeID.SinglePlayer) return;

			ModPacket packet = GetPacket(PacketType.SyncTeleporterIcon);
			packet.Write(teleporter.ID);
			teleporter.Icon.Write(packet);
			packet.Send(ignoreClient: ignoreClient);
		}

		private static void ReceiveTeleporterIcon(BinaryReader reader, int whoAmI)
		{
			Teleporter teleporter = (Teleporter)TileEntity.ByID[reader.ReadInt32()];

			teleporter.Icon.Read(reader);

			if (Main.netMode == NetmodeID.Server) SendTeleporterIcon(teleporter, whoAmI);
		}

		internal static void SendTeleporterWhitelist(Teleporter teleporter, int ignoreClient = -1)
		{
			if (Main.netMode == NetmodeID.SinglePlayer) return;

			ModPacket packet = GetPacket(PacketType.SyncTeleporterWhitelist);
			packet.Write(teleporter.ID);

			for (int i = 0; i < teleporter.Whitelist.Length; i++) packet.Write(teleporter.Whitelist[i]);

			packet.Send(ignoreClient: ignoreClient);
		}

		private static void ReceiveTeleporterWhitelist(BinaryReader reader, int whoAmI)
		{
			Teleporter teleporter = (Teleporter)TileEntity.ByID[reader.ReadInt32()];

			for (int i = 0; i < teleporter.Whitelist.Length; i++) teleporter.Whitelist[i] = reader.ReadBoolean();

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				foreach (UIElement element in BaseLibrary.BaseLibrary.PanelGUI.UI.Elements)
				{
					if (element is TeleporterPanel panel && panel.Container == teleporter) panel.UpdateWhitelist();
				}
			}

			if (Main.netMode == NetmodeID.Server) SendTeleporterWhitelist(teleporter, whoAmI);
		}

		internal static void SendCloseUI(Teleporter teleporter, int client)
		{
			if (Main.netMode != NetmodeID.Server) return;

			ModPacket packet = GetPacket(PacketType.CloseUI);
			packet.Write(teleporter.Position);
			packet.Send(client);
		}

		private static void ReceiveCloseUI(BinaryReader reader, int whoAmI)
		{
			BaseLibrary.BaseLibrary.PanelGUI.UI.CloseUI((IHasUI)TileEntity.ByPosition[reader.ReadPoint16()]);
		}
	}
}