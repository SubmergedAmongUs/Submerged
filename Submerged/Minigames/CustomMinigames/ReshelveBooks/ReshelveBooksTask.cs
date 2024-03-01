using System.Linq;
using Reactor.Utilities.Attributes;
using Submerged.Enums;

namespace Submerged.Minigames.CustomMinigames.ReshelveBooks;

[RegisterInIl2Cpp]
public sealed class ReshelveBooksTask(nint ptr) : NormalPlayerTask(ptr)
{
    public byte[] customData;

    private void Awake()
    {
        Arrow = GetComponentInChildren<ArrowBehaviour>(true);
        ShowTaskStep = true;
        MaxStep = 3;
        HasLocation = true;
    }

    public override void Initialize()
    {
        customData = new byte[4];
        customData[0] = (byte) UnityRandom.Range(0, 3);
        customData[1] = (byte) UnityRandom.Range(0, 3);
        customData[2] = 0;
        customData[3] = 0;
    }

    public override void UpdateArrowAndLocation()
    {
        if (!Arrow)
        {
            return;
        }

        if (!Owner.AmOwner)
        {
            Arrow.gameObject.SetActive(false);

            return;
        }

        if (!IsComplete)
        {
            if (PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
            {
                arrowSuspended = true;
            }
            else
            {
                Arrow.gameObject.SetActive(true);
            }

            Console nextConsole = GetNextConsole();

            Arrow.target = nextConsole.transform.position;
            StartAt = nextConsole.Room;
            LocationDirty = true;
        }
        else
        {
            Arrow.gameObject.SetActive(false);
        }
    }

    public override bool ValidConsole(Console console)
    {
        if (!console.TaskTypes.Contains(CustomTaskTypes.ReshelveBooks)) return false;
        if (console.ConsoleId == 2) return taskStep == 2;

        return customData[console.ConsoleId + 2] == 0;
    }

    public Console GetNextConsole()
    {
        if (taskStep == 2) return ShipStatus.Instance.AllConsoles.FirstOrDefault(c => c.ConsoleId == 2 && c.TaskTypes.Contains(CustomTaskTypes.ReshelveBooks));

        return ShipStatus.Instance.AllConsoles.Last(ValidConsole);
        // if (CustomData[2] == 0) return ShipStatusCached.Instance.AllConsoles.FirstOrDefault(c => c.ConsoleId == 0 && c.TaskTypes.Contains(CustomTaskTypes.ReshelveBooks));
        // return ShipStatusCached.Instance.AllConsoles.FirstOrDefault(c => c.ConsoleId == 1 && c.TaskTypes.Contains(CustomTaskTypes.ReshelveBooks));
    }
}
