using System;
using System.Collections.Generic;
using Hazel;

[Serializable]
public class QuickChatNetData
{
	public QuickChatNetType qcType;

	private StringNames qcKey;

	private byte qcPlayer;

	private Tuple<StringNames, byte>[] qcEntries;

	public QuickChatNetData()
	{
		this.qcType = QuickChatNetType.None;
	}

	public void SetSentence(StringNames key, List<StringNames> names, List<byte> players)
	{
		this.qcType = QuickChatNetType.Sentence;
		this.qcKey = key;
		this.qcEntries = QuickChatNetData.ToProtocolArray(names, players);
	}

	public void SetPhrase(StringNames key)
	{
		this.qcType = QuickChatNetType.Phrase;
		this.qcKey = key;
	}

	public void SetName(byte player)
	{
		this.qcType = QuickChatNetType.Player;
		this.qcPlayer = player;
	}

	public void Serialize(MessageWriter writer)
	{
		writer.Write((byte)this.qcType);
		switch (this.qcType)
		{
		case QuickChatNetType.Sentence:
			writer.WritePacked(this.qcEntries.Length);
			for (int i = 0; i < this.qcEntries.Length; i++)
			{
				Tuple<StringNames, byte> tuple = this.qcEntries[i];
				writer.Write((ushort)tuple.Item1);
				if (tuple.Item1 == StringNames.ANY)
				{
					writer.Write(tuple.Item2);
				}
			}
			writer.Write((ushort)this.qcKey);
			return;
		case QuickChatNetType.Phrase:
			writer.Write((ushort)this.qcKey);
			return;
		case QuickChatNetType.Player:
			writer.Write(this.qcPlayer);
			return;
		default:
			return;
		}
	}

	public static string Deserialize(MessageReader reader)
	{
		string result = "";
		switch (reader.ReadByte())
		{
		case 0:
		{
			int num = reader.ReadPackedInt32();
			List<StringNames> list = new List<StringNames>();
			string[] array = new string[num];
			byte b = 0;
			while ((int)b < num)
			{
				StringNames stringNames = (StringNames)reader.ReadUInt16();
				list.Add(stringNames);
				if (stringNames != StringNames.ANY)
				{
					array[(int)b] = DestroyableSingleton<TranslationController>.Instance.GetString(stringNames, Array.Empty<object>());
				}
				else
				{
					byte b2 = reader.ReadByte();
					if (b2 != 255)
					{
						GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(b2);
						array[(int)b] = playerById.PlayerName;
					}
					else
					{
						array[(int)b] = "";
					}
				}
				b += 1;
			}
			StringNames id = (StringNames)reader.ReadUInt16();
			string fitbvariant = DestroyableSingleton<TranslationController>.Instance.GetFITBVariant(id, list);
			if (fitbvariant != null)
			{
				string format = fitbvariant;
				object[] args = array;
				result = string.Format(format, args);
			}
			else
			{
				string @string = DestroyableSingleton<TranslationController>.Instance.GetString(id, Array.Empty<object>());
				object[] args = array;
				result = string.Format(@string, args);
			}
			break;
		}
		case 1:
			result = DestroyableSingleton<TranslationController>.Instance.GetString((StringNames)reader.ReadUInt16(), Array.Empty<object>());
			break;
		case 2:
		{
			byte b3 = reader.ReadByte();
			if (b3 != 255)
			{
				result = GameData.Instance.GetPlayerById(b3).PlayerName;
			}
			else
			{
				result = "";
			}
			break;
		}
		}
		return result;
	}

	private static Tuple<StringNames, byte>[] ToProtocolArray(List<StringNames> names, List<byte> players)
	{
		Tuple<StringNames, byte>[] array = new Tuple<StringNames, byte>[names.Count];
		for (int i = 0; i < names.Count; i++)
		{
			array[i] = new Tuple<StringNames, byte>(names[i], players[i]);
		}
		return array;
	}
}
