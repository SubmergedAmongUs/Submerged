using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using InnerNet;
using UnityEngine;

public static class SaveManager
{
	private const int schemaVersion = 1;

	private static bool loaded;

	private static bool loadedStats;

	private static bool loadedAnnounce;

	private static bool loadedQCFavorites;

	private static string lastPlayerName;

	private static byte sfxVolume = byte.MaxValue;

	private static byte musicVolume = byte.MaxValue;

	private static bool showMinPlayerWarning = true;

	private static bool showOnlineHelp = true;

	private static byte showAdsScreen = 0;

	private static int privacyPolicyVersion = 0;

	private static int birthDateDay = 1;

	private static int birthDateMonth = 1;

	private static int birthDateYear = 2021;

	private static string birthDateSetDate = "";

	private static string epicAccountId = "";

	private static bool vsync = false;

	private static bool censorChat = true;

	private static int chatModeType = 0;

	private static bool isGuest = false;

	private static bool hasLoggedIn = false;

	private static string guardianEmail = "";

	private static int accountLoginStatus = 0;

	private static ControlTypes touchConfig = ControlTypes.Keyboard;

	private static float joyStickSize = 1f;

	private static uint colorConfig;

	private static uint lastPet;

	private static uint lastHat;

	private static uint lastSkin;

	private static uint lastLanguage = uint.MaxValue;

	private static GameOptionsData hostOptionsData;

	private static GameOptionsData searchOptionsData;

	private static int lastSchemaVersion;

	private static Announcement lastAnnounce;

	public const int quickChatFavoriteSlots = 20;

	private static string[] quickChatFavorites = new string[20];

	private static SecureDataFile purchaseFile = new SecureDataFile(Path.Combine(PlatformPaths.persistentDataPath, "secureNew"));

	private static HashSet<string> purchases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static string dobInfo = "";

	public static Announcement LastAnnouncement
	{
		get
		{
			SaveManager.LoadAnnouncement();
			return SaveManager.lastAnnounce;
		}
		set
		{
			SaveManager.lastAnnounce = value;
			SaveManager.SaveAnnouncement(false);
		}
	}

	public static void DeleteAll()
	{
		string text = Path.Combine(PlatformPaths.persistentDataPath, SaveManager.GetPrefsName());
		if (FileIO.Exists(text))
		{
			FileIO.Delete(text);
			Debug.Log("File " + text + " deleted!");
			return;
		}
		Debug.LogWarning("File " + text + " did not exist, so didn't delete anything.");
	}

	private static string GetPrefsName()
	{
		if (!string.IsNullOrEmpty(SaveManager.epicAccountId))
		{
			return SaveManager.epicAccountId + "_playerPrefs";
		}
		return "playerPrefs";
	}

	public static bool BoughtNoAds
	{
		get
		{
			return true;
		}
	}

	public static bool GetPurchase(string key)
	{
		SaveManager.LoadSecureData();
		return SaveManager.purchases.Contains(key);
	}

	public static void ClearPurchased(string key)
	{
	}

	public static void SetPurchased(string key)
	{
		SaveManager.LoadSecureData();
		SaveManager.purchases.Add(key ?? "null");
		if (key == "bought_ads")
		{
			SaveManager.ShowAdsScreen = ShowAdsState.Purchased;
		}
		SaveManager.SaveSecureData(false);
	}

	public static void SaveLocalDoB(int year, int mo, int day)
	{
		SaveManager.LoadSecureData();
		SaveManager.dobInfo = string.Concat(new string[]
		{
			year.ToString(),
			"-",
			mo.ToString().PadLeft(2, '0'),
			"-",
			day.ToString().PadLeft(2, '0')
		});
		string item = "";
		foreach (string text in SaveManager.purchases)
		{
			if (text.Split(new char[]
			{
				'-'
			}).Length == 3)
			{
				item = text;
			}
		}
		SaveManager.purchases.Remove(item);
		SaveManager.purchases.Add(SaveManager.dobInfo);
		SaveManager.SaveSecureData(false);
	}

	public static bool GetLocalDoB()
	{
		SaveManager.LoadSecureData();
		if (string.IsNullOrEmpty(SaveManager.dobInfo))
		{
			return false;
		}
		string[] array = SaveManager.dobInfo.Split(new char[]
		{
			'-'
		});
		if (array.Length != 3)
		{
			return false;
		}
		int num;
		if (!int.TryParse(array[0], out num))
		{
			return false;
		}
		int num2;
		if (!int.TryParse(array[1], out num2))
		{
			return false;
		}
		int num3;
		if (!int.TryParse(array[2], out num3))
		{
			return false;
		}
		SaveManager.BirthDateYear = num;
		SaveManager.BirthDateMonth = num2;
		SaveManager.BirthDateDay = num3;
		return true;
	}

	private static void LoadSecureData()
	{
		if (!SaveManager.purchaseFile.Loaded)
		{
			try
			{
				SaveManager.purchaseFile.LoadData(delegate(BinaryReader reader)
				{
					while (reader.BaseStream.Position < reader.BaseStream.Length)
					{
						string text = reader.ReadString();
						if (text.Split(new char[]
						{
							'-'
						}).Length == 3)
						{
							SaveManager.dobInfo = text;
						}
						else
						{
							SaveManager.purchases.Add(text);
						}
					}
				});
			}
			catch (NullReferenceException)
			{
			}
			catch (Exception ex)
			{
				string str = "Deleted corrupt secure file outer: ";
				Exception ex2 = ex;
				Debug.Log(str + ((ex2 != null) ? ex2.ToString() : null));
				SaveManager.purchaseFile.Delete();
			}
		}
	}

	public static void SaveSecureData(bool saveNow = false)
	{
		SaveManager.purchaseFile.SaveData(new object[]
		{
			SaveManager.purchases
		});
	}

	public static bool VSync
	{
		get
		{
			SaveManager.LoadPlayerPrefs(false);
			return SaveManager.vsync;
		}
		set
		{
			SaveManager.LoadPlayerPrefs(false);
			SaveManager.vsync = value;
			SaveManager.SavePlayerPrefs(false);
		}
	}

	public static QuickChatModes ChatModeType
	{
		get
		{
			SaveManager.LoadPlayerPrefs(false);
			if (SaveManager.chatModeType == 0)
			{
				SaveManager.chatModeType = 1;
			}
			return (QuickChatModes)SaveManager.chatModeType;
		}
		set
		{
			SaveManager.LoadPlayerPrefs(false);
			SaveManager.chatModeType = (int)value;
			SaveManager.SavePlayerPrefs(false);
		}
	}

	public static bool CensorChat
	{
		get
		{
			SaveManager.LoadPlayerPrefs(false);
			return SaveManager.censorChat;
		}
		set
		{
			SaveManager.LoadPlayerPrefs(false);
			SaveManager.censorChat = value;
			SaveManager.SavePlayerPrefs(false);
		}
	}

	public static ShowAdsState ShowAdsScreen
	{
		get
		{
			SaveManager.LoadPlayerPrefs(false);
			return (ShowAdsState)SaveManager.showAdsScreen;
		}
		set
		{
			SaveManager.LoadPlayerPrefs(false);
			SaveManager.showAdsScreen = (byte)value;
			SaveManager.SavePlayerPrefs(false);
		}
	}

	public static int AcceptedPrivacyPolicy
	{
		get
		{
			SaveManager.LoadPlayerPrefs(false);
			return SaveManager.privacyPolicyVersion;
		}
		set
		{
			SaveManager.LoadPlayerPrefs(false);
			SaveManager.privacyPolicyVersion = value;
			SaveManager.SavePlayerPrefs(false);
		}
	}

	public static EOSManager.AccountLoginStatus AccountLoginStatus
	{
		get
		{
			SaveManager.LoadPlayerPrefs(false);
			return (EOSManager.AccountLoginStatus)SaveManager.accountLoginStatus;
		}
		set
		{
			SaveManager.LoadPlayerPrefs(false);
			SaveManager.accountLoginStatus = (int)value;
			SaveManager.SavePlayerPrefs(false);
		}
	}

	public static int BirthDateMonth
	{
		get
		{
			SaveManager.LoadPlayerPrefs(false);
			return SaveManager.birthDateMonth;
		}
		set
		{
			SaveManager.LoadPlayerPrefs(false);
			SaveManager.birthDateMonth = value;
			SaveManager.SavePlayerPrefs(false);
		}
	}

	public static int BirthDateDay
	{
		get
		{
			SaveManager.LoadPlayerPrefs(false);
			return SaveManager.birthDateDay;
		}
		set
		{
			SaveManager.LoadPlayerPrefs(false);
			SaveManager.birthDateDay = value;
			SaveManager.SavePlayerPrefs(false);
		}
	}

	public static int BirthDateYear
	{
		get
		{
			SaveManager.LoadPlayerPrefs(false);
			return SaveManager.birthDateYear;
		}
		set
		{
			SaveManager.LoadPlayerPrefs(false);
			SaveManager.birthDateYear = value;
			SaveManager.SavePlayerPrefs(false);
		}
	}

	public static string BirthDateSetDate
	{
		get
		{
			SaveManager.LoadPlayerPrefs(false);
			return SaveManager.birthDateSetDate;
		}
		set
		{
			SaveManager.LoadPlayerPrefs(false);
			SaveManager.birthDateSetDate = value;
			SaveManager.SavePlayerPrefs(false);
		}
	}

	public static bool ShowMinPlayerWarning
	{
		get
		{
			SaveManager.LoadPlayerPrefs(false);
			return SaveManager.showMinPlayerWarning;
		}
		set
		{
			SaveManager.LoadPlayerPrefs(false);
			SaveManager.showMinPlayerWarning = value;
			SaveManager.SavePlayerPrefs(false);
		}
	}

	public static bool ShowOnlineHelp
	{
		get
		{
			SaveManager.LoadPlayerPrefs(false);
			return SaveManager.showOnlineHelp;
		}
		set
		{
			SaveManager.LoadPlayerPrefs(false);
			SaveManager.showOnlineHelp = value;
			SaveManager.SavePlayerPrefs(false);
		}
	}

	public static float SfxVolume
	{
		get
		{
			SaveManager.LoadPlayerPrefs(false);
			return (float)SaveManager.sfxVolume / 255f;
		}
		set
		{
			SaveManager.LoadPlayerPrefs(false);
			SaveManager.sfxVolume = (byte)(value * 255f);
			SaveManager.SavePlayerPrefs(false);
		}
	}

	public static float MusicVolume
	{
		get
		{
			SaveManager.LoadPlayerPrefs(false);
			return (float)SaveManager.musicVolume / 255f;
		}
		set
		{
			SaveManager.LoadPlayerPrefs(false);
			SaveManager.musicVolume = (byte)(value * 255f);
			SaveManager.SavePlayerPrefs(false);
		}
	}

	public static ControlTypes ControlMode
	{
		get
		{
			SaveManager.LoadPlayerPrefs(false);
			return SaveManager.touchConfig;
		}
		set
		{
			SaveManager.LoadPlayerPrefs(false);
			SaveManager.touchConfig = value;
			SaveManager.SavePlayerPrefs(false);
		}
	}

	public static float JoystickSize
	{
		get
		{
			SaveManager.LoadPlayerPrefs(false);
			return SaveManager.joyStickSize;
		}
		set
		{
			SaveManager.LoadPlayerPrefs(false);
			SaveManager.joyStickSize = value;
			SaveManager.SavePlayerPrefs(false);
		}
	}

	private static string GetDefaultName()
	{
		return DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.EnterName, Array.Empty<object>());
	}

	public static string PlayerName
	{
		get
		{
			SaveManager.LoadPlayerPrefs(false);
			string text = string.IsNullOrWhiteSpace(SaveManager.lastPlayerName) ? DestroyableSingleton<AccountManager>.Instance.GetRandomName() : SaveManager.lastPlayerName;
			if (text.Length > 10)
			{
				text = text.Substring(0, 10);
			}
			SaveManager.lastPlayerName = text;
			return SaveManager.lastPlayerName;
		}
		set
		{
			SaveManager.LoadPlayerPrefs(false);
			SaveManager.lastPlayerName = value;
			SaveManager.SavePlayerPrefs(false);
		}
	}

	public static string GuardianEmail
	{
		get
		{
			SaveManager.LoadPlayerPrefs(false);
			return SaveManager.guardianEmail;
		}
		set
		{
			SaveManager.LoadPlayerPrefs(false);
			SaveManager.guardianEmail = value;
			SaveManager.SavePlayerPrefs(false);
		}
	}

	public static uint LastPet
	{
		get
		{
			SaveManager.LoadPlayerPrefs(false);
			return SaveManager.lastPet;
		}
		set
		{
			SaveManager.LoadPlayerPrefs(false);
			SaveManager.lastPet = value;
			SaveManager.SavePlayerPrefs(false);
		}
	}

	public static uint LastHat
	{
		get
		{
			SaveManager.LoadPlayerPrefs(false);
			return SaveManager.lastHat;
		}
		set
		{
			SaveManager.LoadPlayerPrefs(false);
			SaveManager.lastHat = value;
			SaveManager.SavePlayerPrefs(false);
		}
	}

	public static uint LastSkin
	{
		get
		{
			SaveManager.LoadPlayerPrefs(false);
			return SaveManager.lastSkin;
		}
		set
		{
			SaveManager.LoadPlayerPrefs(false);
			SaveManager.lastSkin = value;
			SaveManager.SavePlayerPrefs(false);
		}
	}

	public static uint LastLanguage
	{
		get
		{
			SaveManager.LoadPlayerPrefs(false);
			if (SaveManager.lastLanguage > 13U)
			{
				SaveManager.lastLanguage = TranslationController.SelectDefaultLanguage();
			}
			return SaveManager.lastLanguage;
		}
		set
		{
			SaveManager.LoadPlayerPrefs(false);
			SaveManager.lastLanguage = value;
			SaveManager.SavePlayerPrefs(false);
		}
	}

	public static byte BodyColor
	{
		get
		{
			SaveManager.LoadPlayerPrefs(false);
			return (byte)SaveManager.colorConfig;
		}
		set
		{
			SaveManager.LoadPlayerPrefs(false);
			SaveManager.colorConfig = (uint)value;
			SaveManager.SavePlayerPrefs(false);
		}
	}

	public static GameOptionsData GameHostOptions
	{
		get
		{
			if (SaveManager.hostOptionsData == null)
			{
				SaveManager.hostOptionsData = SaveManager.LoadGameOptions("gameHostOptions");
			}
			SaveManager.hostOptionsData.NumImpostors = Mathf.Clamp(SaveManager.hostOptionsData.NumImpostors, 1, 3);
			SaveManager.hostOptionsData.KillDistance = Mathf.Clamp(SaveManager.hostOptionsData.KillDistance, 0, 2);
			return SaveManager.hostOptionsData;
		}
		set
		{
			SaveManager.hostOptionsData = value;
			SaveManager.SaveGameOptions(SaveManager.hostOptionsData, "gameHostOptions", false);
		}
	}

	public static GameOptionsData GameSearchOptions
	{
		get
		{
			if (SaveManager.searchOptionsData == null)
			{
				SaveManager.searchOptionsData = SaveManager.LoadGameOptions("gameSearchOptions");
			}
			return SaveManager.searchOptionsData;
		}
		set
		{
			SaveManager.searchOptionsData = value;
			SaveManager.SaveGameOptions(SaveManager.searchOptionsData, "gameSearchOptions", false);
		}
	}

	private static GameOptionsData LoadGameOptions(string filename)
	{
		string path = Path.Combine(PlatformPaths.persistentDataPath, filename);
		if (File.Exists(path))
		{
			using (FileStream fileStream = File.OpenRead(path))
			{
				using (BinaryReader binaryReader = new BinaryReader(fileStream))
				{
					return GameOptionsData.Deserialize(binaryReader) ?? new GameOptionsData();
				}
			}
		}
		return new GameOptionsData();
	}

	public static void SaveGameOptions(GameOptionsData data, string filename, bool saveNow = false)
	{
		FileIO.WriteAllBytes(Path.Combine(PlatformPaths.persistentDataPath, filename), data.ToBytes(4));
	}

	private static void LoadAnnouncement()
	{
		if (SaveManager.loadedAnnounce)
		{
			return;
		}
		SaveManager.loadedAnnounce = true;
		string path = Path.Combine(PlatformPaths.persistentDataPath, "announcement");
		if (FileIO.Exists(path))
		{
			string[] array = FileIO.ReadAllText(path).Split(new char[1]);
			if (array.Length == 3)
			{
				Announcement announcement = default(Announcement);
				SaveManager.TryGetUint(array, 0, out announcement.Id, 0U);
				announcement.AnnounceText = array[1];
				SaveManager.TryGetDateTime(array, 2, out announcement.DateFetched);
				SaveManager.lastAnnounce = announcement;
				return;
			}
			SaveManager.lastAnnounce = default(Announcement);
		}
	}

	public static void SaveAnnouncement(bool saveNow = false)
	{
		string path = Path.Combine(PlatformPaths.persistentDataPath, "announcement");
		Debug.Log("SaveAnnouncements");
		FileIO.WriteAllText(path, string.Join("\0", new object[]
		{
			SaveManager.lastAnnounce.Id,
			SaveManager.lastAnnounce.AnnounceText,
			SaveManager.lastAnnounce.DateFetched.ToString(DateTimeFormatInfo.InvariantInfo)
		}));
	}

	public static void LoadPlayerPrefs(bool overrideLoad = false)
	{
		if (!overrideLoad && SaveManager.loaded)
		{
			return;
		}
		string path = Path.Combine(PlatformPaths.persistentDataPath, SaveManager.GetPrefsName());
		Debug.Log("Loading save file " + SaveManager.GetPrefsName());
		if (FileIO.Exists(path))
		{
			SaveManager.loaded = true;
			string[] array = FileIO.ReadAllText(path).Split(new char[]
			{
				','
			});
			SaveManager.lastPlayerName = array[0];
			int num;
			if (array.Length > 1 && int.TryParse(array[1], out num))
			{
				SaveManager.touchConfig = (ControlTypes)num;
			}
			if (array.Length <= 2 || !uint.TryParse(array[2], out SaveManager.colorConfig))
			{
				SaveManager.colorConfig = (uint)Palette.PlayerColors.RandomIdx<Color32>();
			}
			SaveManager.TryGetBool(array, 8, out SaveManager.showMinPlayerWarning, false);
			SaveManager.TryGetBool(array, 9, out SaveManager.showOnlineHelp, false);
			SaveManager.TryGetUint(array, 10, out SaveManager.lastHat, 0U);
			SaveManager.TryGetByte(array, 11, out SaveManager.sfxVolume);
			SaveManager.TryGetByte(array, 12, out SaveManager.musicVolume);
			SaveManager.TryGetFloat(array, 13, out SaveManager.joyStickSize, 1f);
			SaveManager.TryGetUint(array, 15, out SaveManager.lastSkin, 0U);
			SaveManager.TryGetUint(array, 16, out SaveManager.lastPet, 0U);
			SaveManager.TryGetBool(array, 17, out SaveManager.censorChat, true);
			SaveManager.TryGetUint(array, 18, out SaveManager.lastLanguage, uint.MaxValue);
			SaveManager.TryGetBool(array, 19, out SaveManager.vsync, false);
			SaveManager.TryGetByte(array, 20, out SaveManager.showAdsScreen);
			SaveManager.TryGetInt(array, 21, out SaveManager.privacyPolicyVersion);
			if (array.Length > 25)
			{
				SaveManager.TryGetInt(array, 23, out SaveManager.birthDateMonth);
				SaveManager.TryGetInt(array, 24, out SaveManager.birthDateDay);
				SaveManager.TryGetInt(array, 25, out SaveManager.birthDateYear);
			}
			if (array.Length > 26)
			{
				SaveManager.birthDateSetDate = array[26];
			}
			SaveManager.TryGetInt(array, 27, out SaveManager.chatModeType);
			SaveManager.TryGetBool(array, 28, out SaveManager.isGuest, false);
			if (array.Length > 29)
			{
				SaveManager.guardianEmail = array[29];
			}
			SaveManager.TryGetBool(array, 30, out SaveManager.hasLoggedIn, false);
			SaveManager.TryGetInt(array, 31, out SaveManager.accountLoginStatus);
			SaveManager.TryGetInt(array, 32, out SaveManager.lastSchemaVersion);
			if (SaveManager.RunMigrations())
			{
				SaveManager.SavePlayerPrefs(true);
			}
		}
	}

	public static void SavePlayerPrefs(bool saveNow = false)
	{
		SaveManager.LoadPlayerPrefs(false);
		string path = Path.Combine(PlatformPaths.persistentDataPath, SaveManager.GetPrefsName());
		try
		{
			SaveManager.lastSchemaVersion = 1;
			FileIO.WriteAllText(path, string.Join(",", new object[]
			{
				SaveManager.lastPlayerName ?? string.Empty,
				(int)SaveManager.touchConfig,
				SaveManager.colorConfig,
				1,
				false,
				false,
				false,
				0,
				SaveManager.showMinPlayerWarning,
				SaveManager.showOnlineHelp,
				SaveManager.lastHat,
				SaveManager.sfxVolume,
				SaveManager.musicVolume,
				SaveManager.joyStickSize.ToString(CultureInfo.InvariantCulture),
				0L,
				SaveManager.lastSkin,
				SaveManager.lastPet,
				SaveManager.censorChat,
				SaveManager.lastLanguage,
				SaveManager.vsync,
				SaveManager.showAdsScreen,
				SaveManager.privacyPolicyVersion,
				SaveManager.epicAccountId,
				SaveManager.birthDateMonth,
				SaveManager.birthDateDay,
				SaveManager.birthDateYear,
				SaveManager.birthDateSetDate,
				SaveManager.chatModeType,
				SaveManager.isGuest,
				SaveManager.guardianEmail,
				SaveManager.hasLoggedIn,
				SaveManager.accountLoginStatus,
				SaveManager.lastSchemaVersion
			}));
		}
		catch (IOException ex)
		{
			Debug.LogError(ex.Message);
		}
	}

	public static string[] QuickChatFavorites
	{
		get
		{
			SaveManager.LoadQuickChatFavorites();
			return SaveManager.quickChatFavorites;
		}
		set
		{
			SaveManager.quickChatFavorites = value;
			SaveManager.SaveQuickChatFavorites(false);
		}
	}

	public static void LoadQuickChatFavorites()
	{
		if (SaveManager.loadedQCFavorites)
		{
			return;
		}
		SaveManager.loadedQCFavorites = true;
		string path = Path.Combine(PlatformPaths.persistentDataPath, "quickChatFavorites");
		if (FileIO.Exists(path))
		{
			SaveManager.quickChatFavorites = FileIO.ReadAllText(path).Split(new char[]
			{
				'\n'
			});
			return;
		}
		if (SaveManager.quickChatFavorites == null)
		{
			SaveManager.quickChatFavorites = new string[20];
		}
		if (SaveManager.quickChatFavorites[0] == null)
		{
			for (int i = 0; i < 20; i++)
			{
				SaveManager.quickChatFavorites[i] = "";
			}
		}
	}

	public static void SaveQuickChatFavorites(bool saveNow = false)
	{
		Debug.LogError("SaveQuickChatFavorites");
		FileIO.WriteAllText(Path.Combine(PlatformPaths.persistentDataPath, "quickChatFavorites"), string.Join("\n", SaveManager.quickChatFavorites));
	}

	private static void TryGetBool(string[] parts, int index, out bool value, bool @default = false)
	{
		value = @default;
		if (index < parts.Length)
		{
			bool.TryParse(parts[index], out value);
		}
	}

	private static void TryGetByte(string[] parts, int index, out byte value)
	{
		value = 0;
		if (index < parts.Length)
		{
			byte.TryParse(parts[index], out value);
		}
	}

	private static void TryGetFloat(string[] parts, int index, out float value, float @default = 0f)
	{
		value = @default;
		if (index < parts.Length)
		{
			float.TryParse(parts[index], NumberStyles.Number, CultureInfo.InvariantCulture, out value);
		}
	}

	private static void TryGetDateTime(string[] parts, int index, out DateTime value)
	{
		value = default(DateTime);
		if (index < parts.Length)
		{
			DateTime.TryParse(parts[index], DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal, out value);
		}
	}

	private static void TryGetInt(string[] parts, int index, out int value)
	{
		value = 0;
		if (index < parts.Length)
		{
			int.TryParse(parts[index], out value);
		}
	}

	private static void TryGetUint(string[] parts, int index, out uint value, uint @default = 0U)
	{
		value = @default;
		if (index < parts.Length)
		{
			uint.TryParse(parts[index], out value);
		}
	}

	private static bool RunMigrations()
	{
		if (SaveManager.lastSchemaVersion < 1)
		{
			if (SaveManager.touchConfig <= ControlTypes.ScreenJoystick)
			{
				SaveManager.touchConfig++;
			}
			return true;
		}
		return false;
	}
}
