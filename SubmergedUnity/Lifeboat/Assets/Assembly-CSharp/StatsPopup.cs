using System;
using System.Text;
using TMPro;
using UnityEngine;

public class StatsPopup : MonoBehaviour
{
	public TextMeshPro StatsText;

	public TextMeshPro NumbersText;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(base.name);
	}

	private void OnEnable()
	{
		StringBuilder stringBuilder = new StringBuilder(1024);
		StringBuilder stringBuilder2 = new StringBuilder(256);
		StatsPopup.AppendStat(stringBuilder, stringBuilder2, StringNames.StatsBodiesReported, StatsManager.Instance.BodiesReported);
		StatsPopup.AppendStat(stringBuilder, stringBuilder2, StringNames.StatsEmergenciesCalled, StatsManager.Instance.EmergenciesCalled);
		StatsPopup.AppendStat(stringBuilder, stringBuilder2, StringNames.StatsTasksCompleted, StatsManager.Instance.TasksCompleted);
		StatsPopup.AppendStat(stringBuilder, stringBuilder2, StringNames.StatsAllTasksCompleted, StatsManager.Instance.CompletedAllTasks);
		StatsPopup.AppendStat(stringBuilder, stringBuilder2, StringNames.StatsSabotagesFixed, StatsManager.Instance.SabsFixed);
		StatsPopup.AppendStat(stringBuilder, stringBuilder2, StringNames.StatsImpostorKills, StatsManager.Instance.ImpostorKills);
		StatsPopup.AppendStat(stringBuilder, stringBuilder2, StringNames.StatsTimesMurdered, StatsManager.Instance.TimesMurdered);
		StatsPopup.AppendStat(stringBuilder, stringBuilder2, StringNames.StatsTimesEjected, StatsManager.Instance.TimesEjected);
		StatsPopup.AppendStat(stringBuilder, stringBuilder2, StringNames.StatsCrewmateStreak, StatsManager.Instance.CrewmateStreak);
		StatsPopup.AppendStat(stringBuilder, stringBuilder2, StringNames.StatsGamesImpostor, StatsManager.Instance.TimesImpostor);
		StatsPopup.AppendStat(stringBuilder, stringBuilder2, StringNames.StatsGamesCrewmate, StatsManager.Instance.TimesCrewmate);
		StatsPopup.AppendStat(stringBuilder, stringBuilder2, StringNames.StatsGamesStarted, StatsManager.Instance.GamesStarted);
		StatsPopup.AppendStat(stringBuilder, stringBuilder2, StringNames.StatsGamesFinished, StatsManager.Instance.GamesFinished);
		StatsPopup.AppendStat(stringBuilder, stringBuilder2, StringNames.StatsImpostorVoteWins, StatsManager.Instance.GetWinReason(GameOverReason.ImpostorByVote));
		StatsPopup.AppendStat(stringBuilder, stringBuilder2, StringNames.StatsImpostorKillsWins, StatsManager.Instance.GetWinReason(GameOverReason.ImpostorByKill));
		StatsPopup.AppendStat(stringBuilder, stringBuilder2, StringNames.StatsImpostorSabotageWins, StatsManager.Instance.GetWinReason(GameOverReason.ImpostorBySabotage));
		StatsPopup.AppendStat(stringBuilder, stringBuilder2, StringNames.StatsCrewmateVoteWins, StatsManager.Instance.GetWinReason(GameOverReason.HumansByVote));
		StatsPopup.AppendStat(stringBuilder, stringBuilder2, StringNames.StatsCrewmateTaskWins, StatsManager.Instance.GetWinReason(GameOverReason.HumansByTask));
		this.StatsText.text = stringBuilder.ToString();
		this.NumbersText.text = stringBuilder2.ToString();
		ControllerManager.Instance.OpenOverlayMenu(base.name, this.BackButton);
	}

	private static void AppendStat(StringBuilder str, StringBuilder strNums, StringNames statName, object stat)
	{
		str.AppendLine(DestroyableSingleton<TranslationController>.Instance.GetString(statName, Array.Empty<object>()));
		strNums.Append(stat);
		strNums.AppendLine();
	}
}
