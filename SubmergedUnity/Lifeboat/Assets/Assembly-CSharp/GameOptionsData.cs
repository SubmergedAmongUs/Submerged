using System;
using System.IO;
using System.Text;
using Hazel;
using InnerNet;
using UnityEngine;

public class GameOptionsData
{
	private const byte SkeldBit = 1;

	private const byte MiraBit = 2;

	private const byte PolusBit = 4;

	private const byte AirshipBit = 16;

	private const byte MapMask = 23;

	public const byte ServerVersion = 2;

	public const byte NewestVersion = 4;

	public static readonly string[] MapNames = new string[]
	{
		"The Skeld",
		"MIRA HQ",
		"Polus",
		"dlekS ehT",
		"Airship"
	};

	public static readonly float[] KillDistances = new float[]
	{
		1f,
		1.8f,
		2.5f
	};

	public static readonly string[] KillDistanceStrings = new string[]
	{
		"Short",
		"Normal",
		"Long"
	};

	public int MaxPlayers = 15;

	public GameKeywords Keywords = GameKeywords.Other;

	public byte MapId;

	public float PlayerSpeedMod = 1f;

	public float CrewLightMod = 1f;

	public float ImpostorLightMod = 1.5f;

	public float KillCooldown = 15f;

	public int NumCommonTasks = 1;

	public int NumLongTasks = 1;

	public int NumShortTasks = 2;

	public int NumEmergencyMeetings = 1;

	public int EmergencyCooldown = 15;

	public int NumImpostors = 1;

	public bool GhostsDoTasks = true;

	public int KillDistance = 1;

	public int DiscussionTime = 15;

	public int VotingTime = 120;

	public bool ConfirmImpostor = true;

	public bool VisualTasks = true;

	public bool AnonymousVotes;

	public TaskBarMode TaskBarMode;

	public bool isDefaults = true;

	private static readonly int[] RecommendedKillCooldown = new int[]
	{
		0,
		0,
		0,
		0,
		45,
		30,
		15,
		35,
		30,
		25,
		20,
		20,
		20,
		20,
		20,
		20
	};

	private static readonly int[] RecommendedImpostors = new int[]
	{
		0,
		0,
		0,
		0,
		1,
		1,
		1,
		2,
		2,
		2,
		2,
		2,
		3,
		3,
		3,
		3
	};

	private static readonly int[] MaxImpostors = new int[]
	{
		0,
		0,
		0,
		0,
		1,
		1,
		1,
		2,
		2,
		3,
		3,
		3,
		3,
		3,
		3,
		3
	};

	public static readonly int[] MinPlayers = new int[]
	{
		4,
		4,
		7,
		9
	};

	private StringBuilder settings = new StringBuilder(2048);

	public void ToggleMapFilter(byte newId)
	{
		byte b = (byte)(((int)this.MapId ^ 1 << (int)newId) & 23);
		if (b != 0)
		{
			this.MapId = b;
		}
	}

	public bool FilterContainsMap(byte newId)
	{
		int num = 1 << (int)newId;
		return ((int)this.MapId & num) == num;
	}

	public GameOptionsData()
	{
		try
		{
			SystemLanguage systemLanguage = Application.systemLanguage;
			if (systemLanguage <= (SystemLanguage) 14)
			{
				if (systemLanguage == (SystemLanguage) 1)
				{
					this.Keywords = GameKeywords.Arabic;
					goto IL_191;
				}
				if (systemLanguage == (SystemLanguage) 9)
				{
					this.Keywords = GameKeywords.Dutch;
					goto IL_191;
				}
				if (systemLanguage == (SystemLanguage) 14)
				{
					this.Keywords = GameKeywords.French;
					goto IL_191;
				}
			}
			else
			{
				if (systemLanguage == (SystemLanguage) 15)
				{
					this.Keywords = GameKeywords.German;
					goto IL_191;
				}
				switch ((int) systemLanguage)
				{
				case 21:
					this.Keywords = GameKeywords.Italian;
					goto IL_191;
				case 22:
					this.Keywords = GameKeywords.Japanese;
					goto IL_191;
				case 23:
					this.Keywords = GameKeywords.Korean;
					goto IL_191;
				case 24:
				case 25:
				case 26:
				case 29:
					break;
				case 27:
					this.Keywords = GameKeywords.Polish;
					goto IL_191;
				case 28:
					this.Keywords = GameKeywords.Portuguese;
					goto IL_191;
				case 30:
					this.Keywords = GameKeywords.Russian;
					goto IL_191;
				default:
					if (systemLanguage == (SystemLanguage) 34)
					{
						this.Keywords = GameKeywords.SpanishLA;
						goto IL_191;
					}
					break;
				}
			}
			this.Keywords = GameKeywords.Other;
			IL_191:;
		}
		catch
		{
		}
	}

	public void SetRecommendations(int numPlayers, GameModes modes)
	{
		numPlayers = Mathf.Clamp(numPlayers, 4, 15);
		this.PlayerSpeedMod = 1f;
		this.CrewLightMod = 1f;
		this.ImpostorLightMod = 1.5f;
		this.KillCooldown = (float)GameOptionsData.RecommendedKillCooldown[numPlayers];
		this.NumCommonTasks = 1;
		this.NumLongTasks = 1;
		this.NumShortTasks = 2;
		this.NumEmergencyMeetings = 1;
		if (modes != GameModes.OnlineGame)
		{
			this.NumImpostors = GameOptionsData.RecommendedImpostors[numPlayers];
		}
		this.KillDistance = 1;
		this.DiscussionTime = 15;
		this.VotingTime = 120;
		this.isDefaults = true;
		this.ConfirmImpostor = true;
		this.VisualTasks = true;
		this.EmergencyCooldown = ((modes == GameModes.OnlineGame) ? 15 : 0);
	}

	public void Serialize(BinaryWriter writer, byte version)
	{
		writer.Write(version);
		writer.Write((byte)this.MaxPlayers);
		writer.Write((uint)this.Keywords);
		writer.Write(this.MapId);
		writer.Write(this.PlayerSpeedMod);
		writer.Write(this.CrewLightMod);
		writer.Write(this.ImpostorLightMod);
		writer.Write(this.KillCooldown);
		writer.Write((byte)this.NumCommonTasks);
		writer.Write((byte)this.NumLongTasks);
		writer.Write((byte)this.NumShortTasks);
		writer.Write(this.NumEmergencyMeetings);
		writer.Write((byte)this.NumImpostors);
		writer.Write((byte)this.KillDistance);
		writer.Write(this.DiscussionTime);
		writer.Write(this.VotingTime);
		writer.Write(this.isDefaults);
		if (version > 1)
		{
			writer.Write((byte)this.EmergencyCooldown);
		}
		if (version > 2)
		{
			writer.Write(this.ConfirmImpostor);
			writer.Write(this.VisualTasks);
		}
		if (version > 3)
		{
			writer.Write(this.AnonymousVotes);
			writer.Write((byte)this.TaskBarMode);
		}
	}

	public static GameOptionsData Deserialize(BinaryReader reader)
	{
		try
		{
			byte b = reader.ReadByte();
			GameOptionsData gameOptionsData = new GameOptionsData();
			gameOptionsData.MaxPlayers = (int)reader.ReadByte();
			gameOptionsData.Keywords = (GameKeywords)reader.ReadUInt32();
			gameOptionsData.MapId = reader.ReadByte();
			gameOptionsData.PlayerSpeedMod = reader.ReadSingle();
			gameOptionsData.CrewLightMod = reader.ReadSingle();
			gameOptionsData.ImpostorLightMod = reader.ReadSingle();
			gameOptionsData.KillCooldown = reader.ReadSingle();
			gameOptionsData.NumCommonTasks = (int)reader.ReadByte();
			gameOptionsData.NumLongTasks = (int)reader.ReadByte();
			gameOptionsData.NumShortTasks = (int)reader.ReadByte();
			gameOptionsData.NumEmergencyMeetings = reader.ReadInt32();
			gameOptionsData.NumImpostors = (int)reader.ReadByte();
			gameOptionsData.KillDistance = (int)reader.ReadByte();
			gameOptionsData.DiscussionTime = reader.ReadInt32();
			gameOptionsData.VotingTime = reader.ReadInt32();
			gameOptionsData.isDefaults = reader.ReadBoolean();
			try
			{
				if (b > 1)
				{
					gameOptionsData.EmergencyCooldown = (int)reader.ReadByte();
				}
				if (b > 2)
				{
					gameOptionsData.ConfirmImpostor = reader.ReadBoolean();
					gameOptionsData.VisualTasks = reader.ReadBoolean();
				}
				if (b > 3)
				{
					gameOptionsData.AnonymousVotes = reader.ReadBoolean();
					gameOptionsData.TaskBarMode = (TaskBarMode)reader.ReadByte();
				}
			}
			catch
			{
			}
			return gameOptionsData;
		}
		catch
		{
		}
		return null;
	}

	public static GameOptionsData Deserialize(MessageReader reader)
	{
		try
		{
			byte b = reader.ReadByte();
			GameOptionsData gameOptionsData = new GameOptionsData();
			gameOptionsData.MaxPlayers = (int)reader.ReadByte();
			gameOptionsData.Keywords = (GameKeywords)reader.ReadUInt32();
			gameOptionsData.MapId = reader.ReadByte();
			gameOptionsData.PlayerSpeedMod = reader.ReadSingle();
			gameOptionsData.CrewLightMod = reader.ReadSingle();
			gameOptionsData.ImpostorLightMod = reader.ReadSingle();
			gameOptionsData.KillCooldown = reader.ReadSingle();
			gameOptionsData.NumCommonTasks = (int)reader.ReadByte();
			gameOptionsData.NumLongTasks = (int)reader.ReadByte();
			gameOptionsData.NumShortTasks = (int)reader.ReadByte();
			gameOptionsData.NumEmergencyMeetings = reader.ReadInt32();
			gameOptionsData.NumImpostors = (int)reader.ReadByte();
			gameOptionsData.KillDistance = (int)reader.ReadByte();
			gameOptionsData.DiscussionTime = reader.ReadInt32();
			gameOptionsData.VotingTime = reader.ReadInt32();
			gameOptionsData.isDefaults = reader.ReadBoolean();
			try
			{
				if (b > 1)
				{
					gameOptionsData.EmergencyCooldown = (int)reader.ReadByte();
				}
				if (b > 2)
				{
					gameOptionsData.ConfirmImpostor = reader.ReadBoolean();
					gameOptionsData.VisualTasks = reader.ReadBoolean();
				}
				if (b > 3)
				{
					gameOptionsData.AnonymousVotes = reader.ReadBoolean();
					gameOptionsData.TaskBarMode = (TaskBarMode)reader.ReadByte();
				}
			}
			catch
			{
			}
			return gameOptionsData;
		}
		catch
		{
		}
		return null;
	}

	public byte[] ToBytes(byte version)
	{
		byte[] result;
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
			{
				this.Serialize(binaryWriter, version);
				binaryWriter.Flush();
				memoryStream.Position = 0L;
				result = memoryStream.ToArray();
			}
		}
		return result;
	}

	public static GameOptionsData FromBytes(byte[] bytes)
	{
		GameOptionsData result;
		using (MemoryStream memoryStream = new MemoryStream(bytes))
		{
			using (BinaryReader binaryReader = new BinaryReader(memoryStream))
			{
				result = (GameOptionsData.Deserialize(binaryReader) ?? new GameOptionsData());
			}
		}
		return result;
	}

	public override string ToString()
	{
		return this.ToHudString(15);
	}

	public string ToHudString(int numPlayers)
	{
		numPlayers = Mathf.Clamp(numPlayers, 0, GameOptionsData.MaxImpostors.Length);
		this.settings.Length = 0;
		try
		{
			this.settings.AppendLine(DestroyableSingleton<TranslationController>.Instance.GetString(this.isDefaults ? StringNames.GameRecommendedSettings : StringNames.GameCustomSettings, Array.Empty<object>()));
			int num = GameOptionsData.MaxImpostors[numPlayers];
			string value = (this.MapId == 0 && Constants.ShouldFlipSkeld()) ? "Dleks" : GameOptionsData.MapNames[(int)this.MapId];
			this.AppendItem(this.settings, StringNames.GameMapName, value);
			this.settings.Append(string.Format("{0}: {1}", DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameNumImpostors, Array.Empty<object>()), this.NumImpostors));
			if (this.NumImpostors > num)
			{
				this.settings.Append(string.Format(" ({0}: {1})", DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Limit, Array.Empty<object>()), num));
			}
			this.settings.AppendLine();
			this.AppendItem(this.settings, StringNames.GameConfirmImpostor, this.ConfirmImpostor);
			this.AppendItem(this.settings, StringNames.GameNumMeetings, this.NumEmergencyMeetings);
			this.AppendItem(this.settings, StringNames.GameAnonymousVotes, this.AnonymousVotes);
			this.AppendItem(this.settings, StringNames.GameEmergencyCooldown, DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameSecondsAbbrev, new object[]
			{
				this.EmergencyCooldown
			}));
			this.AppendItem(this.settings, StringNames.GameDiscussTime, DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameSecondsAbbrev, new object[]
			{
				this.DiscussionTime
			}));
			if (this.VotingTime > 0)
			{
				this.AppendItem(this.settings, StringNames.GameVotingTime, DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameSecondsAbbrev, new object[]
				{
					this.VotingTime
				}));
			}
			else
			{
				this.AppendItem(this.settings, StringNames.GameVotingTime, DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameSecondsAbbrev, new object[]
				{
					"∞"
				}));
			}
			this.AppendItem(this.settings, StringNames.GamePlayerSpeed, this.PlayerSpeedMod, "x");
			this.AppendItem(this.settings, StringNames.GameCrewLight, this.CrewLightMod, "x");
			this.AppendItem(this.settings, StringNames.GameImpostorLight, this.ImpostorLightMod, "x");
			this.AppendItem(this.settings, StringNames.GameKillCooldown, DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameSecondsAbbrev, new object[]
			{
				this.KillCooldown
			}));
			this.AppendItem(this.settings, StringNames.GameKillDistance, DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SettingShort + this.KillDistance, Array.Empty<object>()));
			this.AppendItem(this.settings, StringNames.GameTaskBarMode, DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SettingNormalTaskMode + (int)this.TaskBarMode, Array.Empty<object>()));
			this.AppendItem(this.settings, StringNames.GameVisualTasks, this.VisualTasks);
			this.AppendItem(this.settings, StringNames.GameCommonTasks, this.NumCommonTasks);
			this.AppendItem(this.settings, StringNames.GameLongTasks, this.NumLongTasks);
			this.AppendItem(this.settings, StringNames.GameShortTasks, this.NumShortTasks);
		}
		catch
		{
		}
		return this.settings.ToString();
	}

	private void AppendItem(StringBuilder settings, StringNames stringName, bool value)
	{
		settings.Append(DestroyableSingleton<TranslationController>.Instance.GetString(stringName, Array.Empty<object>()));
		settings.Append(": ");
		settings.AppendLine(DestroyableSingleton<TranslationController>.Instance.GetString(value ? StringNames.SettingsOn : StringNames.SettingsOff, Array.Empty<object>()));
	}

	private void AppendItem(StringBuilder settings, StringNames stringName, float value, string secs)
	{
		settings.Append(DestroyableSingleton<TranslationController>.Instance.GetString(stringName, Array.Empty<object>()));
		settings.Append(": ");
		settings.Append(value);
		settings.AppendLine(secs);
	}

	private void AppendItem(StringBuilder settings, StringNames stringName, int value, string secs)
	{
		settings.Append(DestroyableSingleton<TranslationController>.Instance.GetString(stringName, Array.Empty<object>()));
		settings.Append(": ");
		settings.Append(value);
		settings.AppendLine(secs);
	}

	private void AppendItem(StringBuilder settings, StringNames stringName, string value)
	{
		settings.Append(DestroyableSingleton<TranslationController>.Instance.GetString(stringName, Array.Empty<object>()));
		settings.Append(": ");
		settings.AppendLine(value);
	}

	private void AppendItem(StringBuilder settings, StringNames stringName, int value)
	{
		settings.Append(DestroyableSingleton<TranslationController>.Instance.GetString(stringName, Array.Empty<object>()));
		settings.Append(": ");
		settings.Append(value);
		settings.AppendLine();
	}

	public int GetAdjustedNumImpostors(int playerCount)
	{
		int numImpostors = PlayerControl.GameOptions.NumImpostors;
		int num = 3;
		if (GameData.Instance.PlayerCount < GameOptionsData.MaxImpostors.Length)
		{
			num = GameOptionsData.MaxImpostors[GameData.Instance.PlayerCount];
		}
		return Mathf.Clamp(numImpostors, 1, num);
	}
}
