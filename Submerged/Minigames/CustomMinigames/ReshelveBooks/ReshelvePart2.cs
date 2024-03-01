using Reactor.Utilities.Attributes;
using Submerged.Extensions;
using Submerged.Minigames.MonoBehaviours;
using Submerged.Localization.Strings;
using TMPro;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.ReshelveBooks;

[RegisterInIl2Cpp]
public sealed class ReshelvePart2(nint ptr) : MonoBehaviour(ptr)
{
    public ReshelveBooksMinigame minigame;

    public SpriteRenderer dragging;
    public int remainingBooks = 2;

    public Transform book1;
    public Transform book2;

    private bool _stopClose;

    private void Start()
    {
        Transform books = transform.Find("Books");

        for (int i = 0; i < books.childCount; i++)
        {
            Transform t = books.GetChild(i);

            bool isLoungeBook = i < 3 && i == minigame.loungeBook;
            bool isMedicalBook = i >= 3 && i - 3 == minigame.medicalBook;

            if (!isLoungeBook && !isMedicalBook)
            {
                t.gameObject.SetActive(false);

                continue;
            }

            if (isLoungeBook) book1 = t;
            if (isMedicalBook) book2 = t;

            t.gameObject.SetActive(true);
            Draggable draggable = t.gameObject.AddComponent<Draggable>();
            SpriteRenderer rend = draggable.GetComponent<SpriteRenderer>();
            draggable.onEnter += () => OnHoverEnter(rend);
            draggable.onExit += () => OnHoverExit(rend);
            draggable.onDown += () => OnDragStart(rend);
            draggable.onUp += () => OnDragEnd(rend);
        }

        books.Find("Benthic Beasts")?.GetComponentInChildren<TextMeshPro>().SetText(Tasks.ReshelveBooks_Book02_Lounge);
        books.Find("Ichthyologist Weekly")?.GetComponentInChildren<TextMeshPro>().SetText(Tasks.ReshelveBooks_Book06_Lounge);
        books.Find("Octopus Digest")?.GetComponentInChildren<TextMeshPro>().SetText(Tasks.ReshelveBooks_Book11_Lounge);
        books.Find("Kelper Worms")?.GetComponentInChildren<TextMeshPro>().SetText(Tasks.ReshelveBooks_Book08_Medical);
        books.Find("Nautical Nonsense")?.GetComponentInChildren<TextMeshPro>().SetText(Tasks.ReshelveBooks_Book10_Medical);
        books.Find("Sea Slugs & You!")?.GetComponentInChildren<TextMeshPro>().SetText(Tasks.ReshelveBooks_Book13_Medical);

        Transform drop = transform.Find("DropZoneIndicators");
        drop.Find("Benthic Beasts")?.GetComponentInChildren<TextMeshPro>().SetText(Tasks.ReshelveBooks_Book02_Lounge);
        drop.Find("Ichthyologist Weekly")?.GetComponentInChildren<TextMeshPro>().SetText(Tasks.ReshelveBooks_Book06_Lounge);
        drop.Find("Octopus Digest")?.GetComponentInChildren<TextMeshPro>().SetText(Tasks.ReshelveBooks_Book11_Lounge);
        drop.Find("Kelper Worms")?.GetComponentInChildren<TextMeshPro>().SetText(Tasks.ReshelveBooks_Book08_Medical);
        drop.Find("Nautical Nonsense")?.GetComponentInChildren<TextMeshPro>().SetText(Tasks.ReshelveBooks_Book10_Medical);
        drop.Find("Sea Slugs & You!")?.GetComponentInChildren<TextMeshPro>().SetText(Tasks.ReshelveBooks_Book13_Medical);

        transform.Find("Background/A")?.GetComponentInChildren<TextMeshPro>().SetText(Tasks.ReshelveBooks_Book01);
        transform.Find("Background/C")?.GetComponentInChildren<TextMeshPro>().SetText(Tasks.ReshelveBooks_Book03);
        transform.Find("Background/E")?.GetComponentInChildren<TextMeshPro>().SetText(Tasks.ReshelveBooks_Book04);
        transform.Find("Background/F")?.GetComponentInChildren<TextMeshPro>().SetText(Tasks.ReshelveBooks_Book05);
        transform.Find("Background/J")?.GetComponentInChildren<TextMeshPro>().SetText(Tasks.ReshelveBooks_Book07);
        transform.Find("Background/M")?.GetComponentInChildren<TextMeshPro>().SetText(Tasks.ReshelveBooks_Book09);
        transform.Find("Background/P")?.GetComponentInChildren<TextMeshPro>().SetText(Tasks.ReshelveBooks_Book12);
        transform.Find("Background/V")?.GetComponentInChildren<TextMeshPro>().SetText(Tasks.ReshelveBooks_Book14);
        transform.Find("Background/W")?.GetComponentInChildren<TextMeshPro>().SetText(Tasks.ReshelveBooks_Book15);
    }

    private void Update()
    {
        if (remainingBooks == 0)
        {
            minigame.Task!?.NextStep();
            minigame.StartCoroutine(minigame.CoStartClose());
            remainingBooks = -1;
        }

        if (dragging && dragging.transform.localPosition.magnitude <= 3)
        {
            transform.Find("DropZoneIndicators/" + dragging.name).gameObject.SetActive(true);
        }
        else
        {
            foreach (Transform c in transform.Find("DropZoneIndicators").GetChildren())
            {
                c.gameObject.SetActive(false);
            }
        }
    }

    private void OnHoverEnter(SpriteRenderer rend)
    {
        _stopClose = true;
        rend.material.SetFloat("_Outline", !dragging || dragging == rend ? 1 : 0);
    }

    private void OnHoverExit(SpriteRenderer rend)
    {
        _stopClose = false;
        if (!dragging) rend.material.SetFloat("_Outline", 0);
    }

    private void OnDragStart(SpriteRenderer rend)
    {
        if (rend.transform.name == book1.name)
        {
            book2.SetZLocalPos(book1.transform.localPosition.z + 0.01f);
        }
        else
        {
            book1.SetZLocalPos(book2.transform.localPosition.z + 0.01f);
        }

        dragging = rend;
        rend.material.SetColor("_AddColor", Color.yellow);
    }

    private void OnDragEnd(SpriteRenderer rend)
    {
        dragging = null;
        rend.material.SetColor("_AddColor", Color.black);
        Transform rendTransform = rend.transform;

        if (rendTransform.localPosition.magnitude <= 3)
        {
            rendTransform.localPosition = new Vector3(0, 0, rendTransform.localPosition.z);
            rendTransform.localEulerAngles = Vector3.zero;
            rend.GetComponent<Collider2D>().enabled = false;
            SoundManager.Instance.PlaySound(minigame.minigameProperties.audioClips[0], false);
            remainingBooks--;
        }
    }

    private void TryClose()
    {
        if (_stopClose) return;
        minigame.minigameProperties.CloseTask();
    }
}
