using Il2CppSystem.Text;
using Reactor.Utilities.Attributes;
using Submerged.Localization.Strings;

namespace Submerged.Minigames.CustomMinigames.MicrowaveLunch;

[RegisterInIl2Cpp]
public sealed class MicrowaveLunchTask(nint ptr) : NormalPlayerTask(ptr)
{
    public int[] customData;

    private void Awake()
    {
        MaxStep = 1;
    }

    public override void Initialize()
    {
        HasLocation = true;

        customData = new int[2];
        customData[0] = UnityRandom.Range(0, 5);
        customData[1] = GameManager.Instance.IsHideAndSeek() ? UnityRandom.Range(10, 40) : UnityRandom.Range(100, 150);
    }

    public override void AppendTaskText(StringBuilder sb)
    {
        bool yellowText = ShouldYellowText();
        if (yellowText) sb.Append(IsComplete ? "<color=#00DD00FF>" : "<color=#FFFF00FF>");

        sb.Append(TranslationController.Instance.GetString(StartAt));
        sb.Append(": ");
        sb.Append(TranslationController.Instance.GetString(TaskType));

        if (TimerStarted == TimerState.Started)
        {
            sb.Append(" (");
            sb.Append(string.Format(Tasks.MicrowaveLunch_TaskTimer, (int) TaskTimer / 60, $"{(int) TaskTimer % 60:00}"));
            sb.Append(")");
        }

        if (yellowText) sb.Append("</color>");
        sb.AppendLine();
    }
}
