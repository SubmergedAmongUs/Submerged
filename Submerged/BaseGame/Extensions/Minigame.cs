using UnityEngine;

namespace Submerged.BaseGame.Extensions;

public static class MinigameExtensions
{
    [BaseGameCode(LastChecked.v2024_3_5, "Entire method is copied from base game because we can't call it from the base pointer since that causes an infinite loop.")]
    public static void BaseClose(this Minigame self)
    {
        bool isComplete;
        if (self.amClosing == Minigame.CloseState.Closing)
        {
            UnityEngine.Object.Destroy(self.gameObject);
            return;
        }
        if (self.CloseSound && Constants.ShouldPlaySfx())
        {
            SoundManager.Instance.PlaySound(self.CloseSound, false, 1f, null);
        }
        if (PlayerControl.LocalPlayer.Data.Role.TeamType == RoleTeamTypes.Crewmate)
        {
            GameManager.Instance.LogicMinigame.OnMinigameClose();
        }
        if (PlayerControl.LocalPlayer)
        {
            PlayerControl.HideCursorTemporarily();
        }
        self.amClosing = Minigame.CloseState.Closing;
        self.logger.Info(string.Concat("Closing minigame ", self.GetType().Name));
        IAnalyticsReporter analytics = DestroyableSingleton<DebugAnalytics>.Instance.Analytics;
        GameData.PlayerInfo data = PlayerControl.LocalPlayer.Data;
        TaskTypes taskType = self.TaskType;
        float realtimeSinceStartup = Time.realtimeSinceStartup - self.timeOpened;
        PlayerTask myTask = self.MyTask;
        if (myTask != null)
        {
            isComplete = myTask.IsComplete;
        }
        else
        {
            isComplete = false;
        }
        analytics.MinigameClosed(data, taskType, realtimeSinceStartup, isComplete);
        self.StartCoroutine(self.CoDestroySelf());
    }
}
