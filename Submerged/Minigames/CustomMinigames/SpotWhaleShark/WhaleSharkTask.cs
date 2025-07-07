using Il2CppSystem.Text;
using Reactor.Utilities.Attributes;
using Submerged.BaseGame;
using Submerged.Localization.Strings;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.SpotWhaleShark;

[RegisterInIl2Cpp]
public sealed class WhaleSharkTask(nint ptr) : NormalPlayerTask(ptr)
{
    public static bool CanComplete(PlayerControl player) => !player.Data.Role.IsImpostor;

    public bool visible;
    public float timer;

    public float visibleDuration;
    public float notVisibleDuration;

    private void Start()
    {
        visibleDuration = UnityRandom.Range(20f, 40f);
        notVisibleDuration = GameManager.Instance.IsHideAndSeek() ? UnityRandom.Range(20f, 30f) : UnityRandom.Range(60f, 90f);

        timer = UnityRandom.Range(0f, visibleDuration - 5f);
        MaxStep = 1;
    }

    public override void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;

        if (!visible)
        {
            if (timer >= notVisibleDuration)
            {
                visible = !visible;
                timer = 0;
            }
        }
        else
        {
            if (timer >= visibleDuration)
            {
                visible = !visible;
                timer = 0;
            }
        }
    }

    [BaseGameCode(LastChecked.v2025_5_20, "Part of this method comes from NormalPlayerTask.AppendTaskText")]
    public override void AppendTaskText(StringBuilder sb)
    {
        bool canComplete = CanComplete(PlayerControl.LocalPlayer);

        bool needsColor = IsComplete || (visible && canComplete);

        if (needsColor)
        {
            sb.Append(IsComplete ? "<color=#00DD00FF>" : "<color=#FFFF00FF>");
        }

        sb.Append(TranslationController.Instance.GetString(StartAt));
        sb.Append(": ");
        sb.Append(TranslationController.Instance.GetString(TaskType));

        if (!IsComplete && canComplete)
        {
            sb.Append(" (");
            sb.Append(visible
                ? string.Format(Tasks.SpotWhaleShark_InRange, (int) (visibleDuration - timer))
                : string.Format(Tasks.SpotWhaleShark_NotVisible, (int) (notVisibleDuration - timer)));
            sb.Append(")");
        }

        if (needsColor) sb.Append("</color>");
        sb.AppendLine();
    }
}
