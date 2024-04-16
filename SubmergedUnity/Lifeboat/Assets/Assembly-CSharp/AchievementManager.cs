using System;
using Assets.CoreScripts;
using Steamworks;
using UnityEngine;

public class AchievementManager : DestroyableSingleton<AchievementManager>
{
	private bool gameStarted;

	private RoleTypes myRole;

	private const string TasksCompleteEasyKey = "task_complete_easy";

	private const int TasksCompleteEasy = 10;

	private const string TasksCompleteMediumKey = "task_complete_medium";

	private const int TasksCompleteMedium = 100;

	private const string TasksCompleteHardKey = "task_complete_hard";

	private const int TasksCompleteHard = 500;

	private int cardSwipesThisMatch;

	private const string CardSwipeFirstTryKey = "card_first_try";

	private const string KillDuringLightsKey = "kill_during_lights";

	private int ventsUsedThisMatch;

	private const string NoVentsImpostorWinKey = "no_vents_impostor_win";

	public void UpdateAchievementProgress(string key, int progress, int total)
	{
	}

	public void FlushAchievementProgress()
	{
	}

	public void UnlockAchievement(string key)
	{
		if (SteamUserStats.SetAchievement(key))
		{
			Debug.Log("Achieved " + key);
			return;
		}
		Debug.LogError("Failed to achieve " + key);
	}

	public void OnMatchStart(RoleTypes myRole)
	{
		this.gameStarted = true;
		this.myRole = myRole;
		this.cardSwipesThisMatch = 0;
	}

	public void OnMatchEnd(GameOverReason reason)
	{
		if (!this.gameStarted)
		{
			return;
		}
		this.gameStarted = false;
		if (this.ventsUsedThisMatch == 0 && TempData.DidImpostorsWin(reason))
		{
			GameData.PlayerInfo data = PlayerControl.LocalPlayer.Data;
			if (data != null && data.IsImpostor)
			{
				this.UnlockAchievement("no_vents_impostor_win");
			}
		}
		this.FlushAchievementProgress();
	}

	public void OnConsoleUse(IUsable console)
	{
		if (console is Vent)
		{
			this.ventsUsedThisMatch++;
		}
	}

	public void OnMurder(bool amKiller, bool amVictim)
	{
		if (!this.gameStarted)
		{
			return;
		}
		SwitchSystem switchSystem;
		if (amKiller && this.TryGetSystemOfType<SwitchSystem>(SystemTypes.Electrical, out switchSystem) && switchSystem.IsActive)
		{
			this.UnlockAchievement("kill_during_lights");
		}
	}

	public void OnMeetingVote(GameData.PlayerInfo self, GameData.PlayerInfo target)
	{
	}

	public void OnTaskComplete(TaskTypes taskType)
	{
		if (!this.gameStarted)
		{
			return;
		}
		int tasksCompleted = (int)StatsManager.Instance.TasksCompleted;
		if (tasksCompleted >= 10)
		{
			this.UnlockAchievement("task_complete_easy");
		}
		else
		{
			this.UpdateAchievementProgress("task_complete_easy", tasksCompleted, 10);
		}
		if (tasksCompleted >= 100)
		{
			this.UnlockAchievement("task_complete_medium");
		}
		else
		{
			this.UpdateAchievementProgress("task_complete_medium", tasksCompleted, 100);
		}
		if (tasksCompleted >= 500)
		{
			this.UnlockAchievement("task_complete_hard");
		}
		else
		{
			this.UpdateAchievementProgress("task_complete_hard", tasksCompleted, 500);
		}
		if (taskType == TaskTypes.SwipeCard)
		{
			DestroyableSingleton<Telemetry>.Instance.CardSwipeComplete(this.cardSwipesThisMatch);
			if (this.cardSwipesThisMatch == 0)
			{
				this.UnlockAchievement("card_first_try");
				return;
			}
		}
		else
		{
			DestroyableSingleton<Telemetry>.Instance.WriteCompleteTask(taskType);
		}
	}

	public void OnTaskFailure(TaskTypes taskType)
	{
		if (!this.gameStarted)
		{
			return;
		}
		if (taskType == TaskTypes.SwipeCard)
		{
			this.cardSwipesThisMatch++;
		}
	}

	private bool IsTaskOpen(TaskTypes taskType)
	{
		return Minigame.Instance && Minigame.Instance.TaskType == taskType;
	}

	private bool TryGetSystemOfType<T>(SystemTypes sysType, out T output) where T : class, ISystemType
	{
		output = default(T);
		if (!ShipStatus.Instance)
		{
			return false;
		}
		ISystemType systemType;
		if (!ShipStatus.Instance.Systems.TryGetValue(sysType, out systemType))
		{
			output = (systemType as T);
			return output != null;
		}
		return false;
	}
}
