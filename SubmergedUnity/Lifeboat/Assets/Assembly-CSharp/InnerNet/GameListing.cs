using System;
using Hazel;

namespace InnerNet
{
	[Serializable]
	public struct GameListing
	{
		public uint IP;

		public ushort Port;

		public int GameId;

		public byte PlayerCount;

		public string HostName;

		public int Age;

		public int MaxPlayers;

		public int NumImpostors;

		public byte MapId;

		public static GameListing Deserialize(MessageReader reader)
		{
			GameListing result = default(GameListing);
			result.GameId = reader.ReadInt32();
			result.HostName = reader.ReadString();
			result.PlayerCount = reader.ReadByte();
			result.Age = reader.ReadPackedInt32();
			GameOptionsData gameOptionsData = GameOptionsData.Deserialize(reader);
			result.MapId = gameOptionsData.MapId;
			result.NumImpostors = gameOptionsData.NumImpostors;
			result.MaxPlayers = gameOptionsData.MaxPlayers;
			return result;
		}

		public static GameListing DeserializeV2(MessageReader reader)
		{
			return new GameListing
			{
				IP = reader.ReadUInt32(),
				Port = reader.ReadUInt16(),
				GameId = reader.ReadInt32(),
				HostName = reader.ReadString(),
				PlayerCount = reader.ReadByte(),
				Age = reader.ReadPackedInt32(),
				MapId = reader.ReadByte(),
				NumImpostors = (int)reader.ReadByte(),
				MaxPlayers = (int)reader.ReadByte()
			};
		}
	}
}
