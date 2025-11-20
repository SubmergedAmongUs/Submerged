using System.Linq;
using Reactor.Utilities.Attributes;
using Submerged.Enums;

namespace Submerged.Minigames.CustomMinigames.RecordNavBeaconData;

[RegisterInIl2Cpp]
public sealed class SetNavBeaconTask(nint ptr) : NormalPlayerTask(ptr)
{
    private void Awake()
    {
        Arrow = GetComponentInChildren<ArrowBehaviour>(true);
        ShowTaskStep = true;
        MaxStep = 2;
        HasLocation = true;
        LocationDirty = true;
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

            Console nextConsole = ShipStatus.Instance.AllConsoles
                .FirstOrDefault(c => c.ConsoleId == 1 && c.TaskTypes.Contains(CustomTaskTypes.RecordNavBeaconData));

            if (!nextConsole)
            {
                Warning("Could not find second console for RecordNavBeaconData task!");
                Arrow.gameObject.SetActive(false);
                return;
            }

            Arrow.target = nextConsole.transform.position;
            StartAt = nextConsole.Room;
            LocationDirty = true;
        }
        else
        {
            Arrow.gameObject.SetActive(false);
        }
    }
}
