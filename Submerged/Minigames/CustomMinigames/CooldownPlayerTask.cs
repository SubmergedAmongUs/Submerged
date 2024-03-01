using System;
using System.Linq;
using Il2CppSystem.Text;
using Reactor.Utilities.Attributes;
using Submerged.BaseGame;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames;

// TODO: change this to cooldown consoles or something idk
[RegisterInIl2Cpp]
public sealed class CooldownPlayerTask(nint ptr) : NormalPlayerTask(ptr)
{
    public float timer;

    private void Awake()
    {
        MaxStep = 1;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer < 0) timer = 0;

        HasLocation = timer <= 0;
    }

    public override bool ValidConsole(Console console) => BaseValidConsole(console) && timer <= 0;

    public override void AppendTaskText(StringBuilder sb)
    {
        if (timer <= 0 || IsComplete)
        {
            BaseAppendTaskText(sb);
            return;
        }

        sb.Append("<color=#ffa500ff>");
        BaseAppendTaskText(sb);
        sb.Insert(sb.Length - Environment.NewLine.Length, $" ({Mathf.Floor(timer)}s)");
        sb.Append("</color>");
    }

    [BaseGameCode(LastChecked.v2023_10_24, "Default switch case of NormalPlayerTask.ValidConsole, none of the other cases apply")]
    private bool BaseValidConsole(Console console)
    {
        return console.TaskTypes.Any(tt => tt == TaskType) ||
            console.ValidTasks.Any(set => TaskType == set.taskType && set.taskStep.Contains(taskStep));
    }

    [BaseGameCode(LastChecked.v2023_10_24, "NormalPlayerTask.AppendTaskText with steps and timer conditions removed because they are never met")]
    private void BaseAppendTaskText(StringBuilder sb)
    {
        if (IsComplete) sb.Append("<color=#00DD00FF>");

        sb.Append(TranslationController.Instance.GetString(StartAt));
        sb.Append(": ");
        sb.Append(TranslationController.Instance.GetString(TaskType));

        if (IsComplete) sb.Append("</color>");

        sb.AppendLine();
    }
}
