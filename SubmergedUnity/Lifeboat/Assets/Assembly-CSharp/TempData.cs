using System;
using System.Collections.Generic;

public static class TempData
{
	public static DeathReason LastDeathReason;

	public static GameOverReason EndReason = GameOverReason.HumansByTask;

	public static bool showAd;

	public static List<WinningPlayerData> winners = new List<WinningPlayerData>
	{
		new WinningPlayerData
		{
			Name = "WWWWWWWWWW",
			ColorId = 0,
			SkinId = 0U,
			IsDead = true
		},
		new WinningPlayerData
		{
			Name = "WWWWWWWWWW",
			ColorId = 1,
			SkinId = 1U,
			IsDead = true
		},
		new WinningPlayerData
		{
			Name = "WWWWWWWWWW",
			ColorId = 2,
			SkinId = 2U,
			IsDead = true
		},
		new WinningPlayerData
		{
			Name = "WWWWWWWWWW",
			ColorId = 3,
			SkinId = 0U
		},
		new WinningPlayerData
		{
			Name = "WWWWWWWWWW",
			ColorId = 4,
			SkinId = 1U
		},
		new WinningPlayerData
		{
			Name = "WWWWWWWWWW",
			ColorId = 5,
			SkinId = 2U
		},
		new WinningPlayerData
		{
			Name = "WWWWWWWWWW",
			ColorId = 6
		},
		new WinningPlayerData
		{
			Name = "WWWWWWWWWW",
			ColorId = 7
		},
		new WinningPlayerData
		{
			Name = "WWWWWWWWWW",
			ColorId = 8
		}
	};

	public static bool DidHumansWin(GameOverReason reason)
	{
		return reason == GameOverReason.HumansByTask || reason == GameOverReason.HumansByVote;
	}

	public static bool DidImpostorsWin(GameOverReason reason)
	{
		return reason == GameOverReason.ImpostorByKill || reason == GameOverReason.ImpostorBySabotage || reason == GameOverReason.ImpostorByVote;
	}
}
