using System;
using UnityEngine;

public static class Constants
{
	public const int MinNumPlayers = 4;

	public const int MaxNumPlayers = 15;

	public const string LocalNetAddress = "127.0.0.1";

	public const ushort GamePlayPort = 22023;

	public const ushort AnnouncementPort = 22024;

	public const string InfinitySymbol = "∞";

	public static readonly int ShipOnlyMask = LayerMask.GetMask(new string[]
	{
		"Ship"
	});

	public static readonly int ShipAndObjectsMask = LayerMask.GetMask(new string[]
	{
		"Ship",
		"Objects"
	});

	public static readonly int ShipAndAllObjectsMask = LayerMask.GetMask(new string[]
	{
		"Ship",
		"Objects",
		"ShortObjects"
	});

	public static readonly int NotShipMask = ~LayerMask.GetMask(new string[]
	{
		"Ship"
	});

	public static readonly int Usables = ~LayerMask.GetMask(new string[]
	{
		"Ship",
		"UI"
	});

	public static readonly int PlayersOnlyMask = LayerMask.GetMask(new string[]
	{
		"Players",
		"Ghost"
	});

	public static readonly int ShadowMask = LayerMask.GetMask(new string[]
	{
		"Shadow",
		"Objects",
		"IlluminatedBlocking"
	});

	public static readonly int[] CompatVersions = new int[]
	{
		Constants.GetBroadcastVersion()
	};

	public const int Year = 2021;

	public const int Month = 6;

	public const int Day = 30;

	public const int Revision = 0;

	public const int VisualRevision = 0;

	public const int PrivacyPolicyVersion = 2;

	public const string extraBuildVersionInfo = "";

	public const int pipelineBuildNumber = 0;

	internal static int GetBroadcastVersion()
	{
		return 50537300;
	}

	internal static int GetVersion(int year, int month, int day, int rev)
	{
		return year * 25000 + month * 1800 + day * 50 + rev;
	}

	internal static byte[] GetBroadcastVersionBytes()
	{
		return BitConverter.GetBytes(Constants.GetBroadcastVersion());
	}

	public static bool ShouldPlaySfx()
	{
		return !AmongUsClient.Instance || AmongUsClient.Instance.GameMode != GameModes.LocalGame || DetectHeadset.Detect();
	}

	internal static bool ShouldFlipSkeld()
	{
		try
		{
			DateTime utcNow = DateTime.UtcNow;
			DateTime t = new DateTime(utcNow.Year, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			DateTime t2 = t.AddDays(1.0);
			if (utcNow >= t && utcNow <= t2)
			{
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	public static Platforms GetPlatformType()
	{
		return Platforms.StandaloneSteamPC;
	}
}
