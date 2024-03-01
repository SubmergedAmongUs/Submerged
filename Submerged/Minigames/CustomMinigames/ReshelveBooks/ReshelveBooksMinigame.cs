using Reactor.Utilities.Attributes;
using Submerged.Minigames.MonoBehaviours;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.ReshelveBooks;

[RegisterInIl2Cpp]
public sealed class ReshelveBooksMinigame(nint ptr) : Minigame(ptr)
{
    public int loungeBook;
    public int medicalBook;

    public MinigameProperties minigameProperties;

    public ReshelveBooksTask Task => MyNormTask.Cast<ReshelveBooksTask>();

    public void Start()
    {
        minigameProperties = GetComponent<MinigameProperties>();

        loungeBook = Task.customData[0];
        medicalBook = Task.customData[1];

        GameObject lounge = transform.Find("Part1_Lounge").gameObject;
        GameObject medical = transform.Find("Part1_Medical").gameObject;
        GameObject part2 = transform.Find("Part2").gameObject;

        lounge.SetActive(ConsoleId == 0);
        medical.SetActive(ConsoleId == 1);
        part2.SetActive(ConsoleId == 2);

        switch (ConsoleId)
        {
            case 0:
                ReshelvePart1 loungeTask = lounge.AddComponent<ReshelvePart1>();
                loungeTask.minigame = this;

                break;
            case 1:
                ReshelvePart1 medicalTask = medical.AddComponent<ReshelvePart1>();
                medicalTask.minigame = this;

                break;
            case 2:
                ReshelvePart2 part2Task = part2.AddComponent<ReshelvePart2>();
                part2Task.minigame = this;

                break;
        }
    }
}
