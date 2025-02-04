using System.Collections.Generic;
using System.Linq;
using Il2CppSystem.Text;
using Reactor.Utilities.Attributes;
using Submerged.Enums;
using Submerged.Floors;
using Submerged.Systems.Oxygen;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.RetrieveOxygenMask;

[RegisterInIl2Cpp]
public sealed class OxygenSabotageTask(nint ptr) : PlayerTask(ptr)
{
    public bool isComplete;
    public bool taskTextYellow;

    public ArrowBehaviour[] arrows;

    public SubmarineOxygenSystem system;

    public override bool IsComplete => isComplete;
    public override int TaskStep => 0;

    private void Awake()
    {
        arrows = GetComponentsInChildren<ArrowBehaviour>(true);
    }

    public void FixedUpdate()
    {
        if (isComplete) return;

        if (!system.IsActive)
        {
            Complete();

            return;
        }

        List<Console> list = ShipStatus.Instance.AllConsoles.Where(ValidConsole).ToList();

        foreach (ArrowBehaviour t in arrows)
        {
            t.gameObject.SetActive(false);
        }

        ArrowBehaviour arrow = arrows[0];
        Console console = list.FirstOrDefault(c => FloorHandler.LocalPlayer.onUpper ? c.name.Contains("UpperOxygenConsole") : c.name.Contains("LowerOxygenConsole"));

        if (console)
        {
            arrow.gameObject.SetActive(!system.playersWithMask.Contains(PlayerControl.LocalPlayer.PlayerId));
            arrow.target = console.transform.position;
        }
    }

    public override void Initialize()
    {
        system = SubmarineOxygenSystem.Instance;
        HudManager.Instance.StartOxyFlash();
    }

    public override bool ValidConsole(Console console)
    {
        return console.TaskTypes.Any(t => t == CustomTaskTypes.RetrieveOxygenMask) ||
               console.ValidTasks.Any(t => t.taskType == CustomTaskTypes.RetrieveOxygenMask);
    }

    public override void AppendTaskText(StringBuilder sb)
    {
        taskTextYellow = !taskTextYellow;
        Color color = taskTextYellow || system.playersWithMask.Contains(PlayerControl.LocalPlayer.PlayerId) ? Color.yellow : Color.red;

        sb.Append(color.ToTextColor());
        sb.Append(TranslationController.Instance.GetString(CustomTaskTypes.RetrieveOxygenMask));
        sb.Append(" ");
        sb.Append(Mathf.CeilToInt(system.countdown));
        sb.AppendLine(Color.white.ToTextColor());

        foreach (ArrowBehaviour t in arrows)
        {
            try
            {
                t.image.color = color;
            }
            catch
            {
                // ignore
            }
        }
    }

    public override void Complete()
    {
        isComplete = true;
        PlayerControl.LocalPlayer.RemoveTask(this);
        OxygenSabotageMinigame minigame = FindObjectOfType<OxygenSabotageMinigame>();
        if (minigame != null)
        {
            minigame.Close();
        }
    }

    public override void OnRemove() { }
}
