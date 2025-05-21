using UnityEngine;

namespace Submerged.BaseGame.Extensions;

public static class MinigameExtensions
{
    [BaseGameCode(LastChecked.v2025_5_20, "Entire method is copied from base game because we can't call it from the base pointer since that causes an infinite loop.")]
    public static void BaseClose(this Minigame self)
    {
        if (self.amClosing != Minigame.CloseState.Closing)
        {
            if (self.CloseSound && Constants.ShouldPlaySfx())
            {
                SoundManager.Instance.PlaySound(self.CloseSound, false);
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
            self.logger.Info("Closing minigame " + self.GetType().Name);
            IAnalyticsReporter analytics = DebugAnalytics.Instance.Analytics;
            NetworkedPlayerInfo data = PlayerControl.LocalPlayer.Data;
            TaskTypes taskType = self.TaskType;
            float num = Time.realtimeSinceStartup - self.timeOpened;
            PlayerTask myTask = self.MyTask;
            analytics.MinigameClosed(data, taskType, num, myTask != null && myTask.IsComplete);
            self.StartCoroutine(self.CoDestroySelf());
            return;
        }
        UnityObject.Destroy(self.gameObject);
    }
}
