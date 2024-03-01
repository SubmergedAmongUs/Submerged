using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using Submerged.Extensions;
using Submerged.Minigames.MonoBehaviours;
using Submerged.Localization.Strings;
using TMPro;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.ReshelveBooks;

[RegisterInIl2Cpp]
public sealed class ReshelvePart1(nint ptr) : MonoBehaviour(ptr)
{
    public ReshelveBooksMinigame minigame;
    private bool _clicked;

    private void Start()
    {
        Transform[] books = transform.Find("Books").GetChildren();
        int bookIndex = minigame.ConsoleId == 0 ? minigame.loungeBook : minigame.medicalBook;

        transform.Find("paper").GetComponentInChildren<TextMeshPro>().text = (bookIndex switch
        {
            0 when minigame.ConsoleId == 0 => Tasks.ReshelveBooks_Book02_Lounge,
            1 when minigame.ConsoleId == 0 => Tasks.ReshelveBooks_Book06_Lounge,
            2 when minigame.ConsoleId == 0 => Tasks.ReshelveBooks_Book11_Lounge,
            0 when minigame.ConsoleId != 0 => Tasks.ReshelveBooks_Book08_Medical,
            1 when minigame.ConsoleId != 0 => Tasks.ReshelveBooks_Book10_Medical,
            2 when minigame.ConsoleId != 0 => Tasks.ReshelveBooks_Book13_Medical,
            _                              => ""
        }).Replace("<size=125%>", "<size=100%>");

        ClickableSprite clickableSprite = books[bookIndex].gameObject.AddComponent<ClickableSprite>();
        clickableSprite.onDown += () =>
        {
            if (_clicked) return;
            _clicked = true;
            this.StartCoroutine(CoOnMouseDown(clickableSprite.gameObject));
        };

        transform.Find("Books/Benthic Beasts")?.GetComponentInChildren<TextMeshPro>().SetText(Tasks.ReshelveBooks_Book02_Lounge);
        transform.Find("Books/Ichthyologist Weekly")?.GetComponentInChildren<TextMeshPro>().SetText(Tasks.ReshelveBooks_Book06_Lounge);
        transform.Find("Books/Octopus Digest")?.GetComponentInChildren<TextMeshPro>().SetText(Tasks.ReshelveBooks_Book11_Lounge);
        transform.Find("Books/Kelper Worms")?.GetComponentInChildren<TextMeshPro>().SetText(Tasks.ReshelveBooks_Book08_Medical);
        transform.Find("Books/Nautical Nonsense")?.GetComponentInChildren<TextMeshPro>().SetText(Tasks.ReshelveBooks_Book10_Medical);
        transform.Find("Books/Sea Slugs & You!")?.GetComponentInChildren<TextMeshPro>().SetText(Tasks.ReshelveBooks_Book13_Medical);
    }

    [HideFromIl2Cpp]
    private IEnumerator CoOnMouseDown(GameObject obj)
    {
        minigame.Task.customData[minigame.ConsoleId + 2] = 1;
        minigame.Task!?.NextStep();

        SpriteRenderer rend = obj.GetComponent<SpriteRenderer>();
        TextMeshPro text = obj.GetComponentInChildren<TextMeshPro>();

        for (float t = 0; t <= 0.5f; t += Time.deltaTime)
        {
            rend.color = text.color = new Color(1, 1, 1, Mathf.Lerp(1, 0, t * 2));

            yield return null;
        }

        minigame.minigameProperties.CloseTask();
    }
}
