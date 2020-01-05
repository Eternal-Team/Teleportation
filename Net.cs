using BaseLibrary;
using BaseLibrary.UI.New;
using System.IO;
using Teleportation.TileEntities;
using Teleportation.UI;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Teleportation
{
	internal static class Net
	{
		internal enum PacketType : byte
		{
			TeleporterItems,
			TeleporterName,
			TeleporterDestination,
			TeleporterIcon,
			TeleporterWhitelist,
			TeleporterKill,
			CloseUI,
			TeleporterPlace
		}

		internal static ModPacket GetPacket(PacketType packetType)
		{
			ModPacket packet = ModContent.GetInstance<Teleportation>().GetPacket();
			packet.Write((byte)packetType);
			return packet;
		}

		internal static void HandlePacket(BinaryReader reader, int whoAmI)
		{
			PacketType packetType = (PacketType)reader.ReadByte();

			switch (packetType)
			{
				case PacketType.TeleporterItems:
					ReceiveTeleporterItems(reader, whoAmI);
					break;
				case PacketType.TeleporterName:
					ReceiveTeleporterName(reader, whoAmI);
					break;
				case PacketType.TeleporterDestination:
					ReceiveTeleporterDestination(reader, whoAmI);
					break;
				case PacketType.TeleporterIcon:
					ReceiveTeleporterIcon(reader, whoAmI);
					break;
				case PacketType.TeleporterWhitelist:
					ReceiveTeleporterWhitelist(reader, whoAmI);
					break;
				case PacketType.CloseUI:
					ReceiveCloseUI(reader, whoAmI);
					break;
				case PacketType.TeleporterPlace:
					ReceivePlaceTeleporter(reader, whoAmI);
					break;
				case PacketType.TeleporterKill:
					ReceiveKillTeleporter(reader, whoAmI);
					break;
			}
		}

		internal static void SendTeleporterItems(Teleporter teleporter, int ignoreClient = -1)
		{
			if (Main.netMode == NetmodeID.SinglePlayer) return;

			ModPacket packet = GetPacket(PacketType.TeleporterItems);
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

			ModPacket packet = GetPacket(PacketType.TeleporterName);
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

			ModPacket packet = GetPacket(PacketType.TeleporterName);
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

			ModPacket packet = GetPacket(PacketType.TeleporterIcon);
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

			ModPacket packet = GetPacket(PacketType.TeleporterWhitelist);
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
				foreach (BaseElement element in PanelUI.Instance.Children)
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
			PanelUI.Instance.CloseUI((IHasUI)TileEntity.ByPosition[reader.ReadPoint16()]);
		}

		internal static void SendPlaceTeleporter(int ignoreClient = -1)
		{
			if (Main.netMode == NetmodeID.SinglePlayer) return;

			ModPacket packet = GetPacket(PacketType.TeleporterPlace);
			packet.Send(ignoreClient: ignoreClient);
		}

		private static void ReceivePlaceTeleporter(BinaryReader reader, int whoAmI)
		{
			if (Main.netMode == NetmodeID.Server) SendPlaceTeleporter(whoAmI);
			else
			{
				foreach (BaseElement element in PanelUI.Instance.Children)
				{
					if (element is TeleporterPanel panel) panel.UpdateGrid();
				}
			}
		}

		internal static void SendKillTeleporter(Teleporter teleporter, int ignoreClient = -1)
		{
			if (Main.netMode == NetmodeID.SinglePlayer) return;

			ModPacket packet = GetPacket(PacketType.TeleporterKill);
			packet.Write(teleporter.Position);
			packet.Send(ignoreClient: ignoreClient);
		}

		private static void ReceiveKillTeleporter(BinaryReader reader, int whoAmI)
		{
			Teleporter killed = (Teleporter)TileEntity.ByPosition[reader.ReadPoint16()];

			if (Main.netMode == NetmodeID.Server) SendKillTeleporter(killed, whoAmI);
			else
			{
				foreach (TileEntity tileEntity in TileEntity.ByPosition.Values)
				{
					if (tileEntity is Teleporter teleporter && teleporter.Destination == killed) teleporter.Destination = null;
				}
			}
		}
	}
}