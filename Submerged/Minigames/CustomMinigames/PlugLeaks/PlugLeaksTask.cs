using System.Collections.Generic;
using System.Linq;
using Reactor.Utilities.Attributes;
using Submerged.Enums;
using Submerged.Extensions;
using Submerged.Map;

namespace Submerged.Minigames.CustomMinigames.PlugLeaks;

[RegisterInIl2Cpp]
public sealed class PlugLeaksTask(nint ptr) : NormalPlayerTask(ptr)
{
    public List<int> validConsoleIds;

    private void Awake()
    {
        Arrow = GetComponentInChildren<ArrowBehaviour>(true);
        ShowTaskStep = true;
        HasLocation = true;
        LocationDirty = true;

        // Upper Deck only has 2 plug leaks consoles
        if (GameManager.Instance.IsHideAndSeek() && SubmergedHnSManager.CurrentGameIsOnUpperDeck)
            MaxStep = 2;
        else
            MaxStep = 3;
    }

    public override void Initialize()
    {
        List<Console> consoles = ShipStatus.Instance.AllConsoles.Where(c => c.TaskTypes.Contains(CustomTaskTypes.PlugLeaks)).ToList();

        if (GameManager.Instance.IsHideAndSeek())
        {
            if (SubmergedHnSManager.CurrentGameIsOnUpperDeck)
            {
                consoles = consoles.Where(c =>
                                              c.Room == CustomSystemTypes.Research ||
                                              c.Room == CustomSystemTypes.UpperLobby)
                                   .ToList();
                MaxStep = 2;
            }
            else
            {
                consoles = consoles.Where(c =>
                                              c.Room is SystemTypes.Hallway or SystemTypes.Engine ||
                                              c.Room == CustomSystemTypes.Ballast)
                                   .ToList();
                MaxStep = 3;
            }
        }

        validConsoleIds = consoles.Select(c => c.ConsoleId).ToArray().Shuffle().Take(MaxStep).ToList();
        validConsoleIds.Sort();
        StartAt = consoles.First(c => c.ConsoleId == validConsoleIds[0]).Room;
        HasLocation = true;
        LocationDirty = true;
    }

    public override bool ValidConsole(Console console) => console.TaskTypes.Contains(CustomTaskTypes.PlugLeaks) && validConsoleIds.Contains(console.ConsoleId);

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
            HasLocation = true;
            LocationDirty = true;
        }
        else
        {
            Arrow.gameObject.SetActive(false);
        }
    }
}
