using BaseLibrary;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Teleportation.TileEntities;
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
			SyncTeleporterItems,
			SyncTeleporterName,
			SyncTeleporterDestination,
			SyncTeleporterIcon,
			SyncTeleporterWhitelist
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
			packet.Write(teleporter.Destination.Position);
			packet.Send(ignoreClient: ignoreClient);
		}

		private static void ReceiveTeleporterDestination(BinaryReader reader, int whoAmI)
		{
			Teleporter teleporter = (Teleporter)TileEntity.ByID[reader.ReadInt32()];
			teleporter.Destination = (Teleporter)TileEntity.ByPosition[reader.ReadPoint16()];

			if (Main.netMode == NetmodeID.Server) SendTeleporterDestination(teleporter, whoAmI);
		}

		internal static void SendTeleporterIcon(Teleporter teleporter, int ignoreClient = -1)
		{
			if (Main.netMode == NetmodeID.SinglePlayer) return;

			ModPacket packet = GetPacket(PacketType.SyncTeleporterIcon);
			packet.Write(teleporter.ID);

			packet.Write(teleporter.EntityAnimation.TicksPerFrame);
			packet.Write(teleporter.EntityAnimation.FrameCount);

			teleporter.EntityTexture.SaveAsPng(packet.BaseStream, teleporter.EntityTexture.Width, teleporter.EntityTexture.Height);

			packet.Send(ignoreClient: ignoreClient);
		}

		private static void ReceiveTeleporterIcon(BinaryReader reader, int whoAmI)
		{
			Teleporter teleporter = (Teleporter)TileEntity.ByID[reader.ReadInt32()];
			teleporter.EntityAnimation = new DrawAnimationVertical(reader.ReadInt32(), reader.ReadInt32());
			teleporter.EntityTexture = Texture2D.FromStream(Main.graphics.GraphicsDevice, reader.BaseStream);

			if (Main.netMode == NetmodeID.Server) SendTeleporterIcon(teleporter, whoAmI);
		}

		// todo: needs to update UIs
		internal static void SendTeleporterWhitelist(Teleporter teleporter, int ignoreClient = -1)
		{
			if (Main.netMode == NetmodeID.SinglePlayer) return;

			ModPacket packet = GetPacket(PacketType.SyncTeleporterWhitelist);
			packet.Write(teleporter.ID);

			for (int i = 0; i < 4; i++) packet.Write(teleporter.Whitelist[i]);

			packet.Send(ignoreClient: ignoreClient);
		}

		private static void ReceiveTeleporterWhitelist(BinaryReader reader, int whoAmI)
		{
			Teleporter teleporter = (Teleporter)TileEntity.ByID[reader.ReadInt32()];

			for (int i = 0; i < 4; i++) teleporter.Whitelist[i] = reader.ReadBoolean();

			if (Main.netMode == NetmodeID.Server) SendTeleporterWhitelist(teleporter, whoAmI);
		}
	}
}