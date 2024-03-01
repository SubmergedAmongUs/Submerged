using System.Collections.Generic;
using System.Linq;
using Reactor.Utilities.Attributes;
using Submerged.Enums;
using Submerged.Extensions;

namespace Submerged.Minigames.CustomMinigames.MopPuddles;

[RegisterInIl2Cpp]
public sealed class MopPuddlesTask(nint ptr) : NormalPlayerTask(ptr)
{
    public List<int> validConsoleIds;

    private void Awake()
    {
        Arrow = GetComponentInChildren<ArrowBehaviour>(true);
        ShowTaskStep = true;
        MaxStep = 3;
        HasLocation = true;
        LocationDirty = true;
    }

    public override void Initialize()
    {
        List<Console> consoles = ShipStatus.Instance.AllConsoles.Where(c => c.TaskTypes.Contains(CustomTaskTypes.MopPuddles)).ToList();
        if (GameManager.Instance.IsHideAndSeek()) // In HnS, this task can only appear on the lower deck, so we only want lower deck consoles to be selected.
            consoles = consoles.Where(c =>
                                          c.Room is SystemTypes.Hallway or SystemTypes.Storage ||
                                          c.Room == CustomSystemTypes.Filtration)
                               .ToList();

        validConsoleIds = consoles.Select(t => t.ConsoleId).ToArray().Shuffle().Take(MaxStep).ToList();
        validConsoleIds.Sort();

        StartAt = consoles.First(c => c.ConsoleId == validConsoleIds[0]).Room;
        LocationDirty = true;
    }

    public override bool ValidConsole(Console console) => console.TaskTypes.Contains(CustomTaskTypes.MopPuddles) && validConsoleIds.Contains(console.ConsoleId);

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

            Console nextConsole = ShipStatus.Instance.AllConsoles.Where(ValidConsole).First(c => c.ConsoleId == validConsoleIds[0]);

            Arrow.target = nextConsole.transform.position;
            StartAt = nextConsole.Room;
            LocationDirty = true;
            HasLocation = true;
        }
        else
        {
            Arrow.gameObject.SetActive(false);
        }
    }
}
