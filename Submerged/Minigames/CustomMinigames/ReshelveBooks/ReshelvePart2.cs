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

        SetText(books, "Benthic Beasts", Tasks.ReshelveBooks_Book02_Lounge);
        SetText(books, "Ichthyologist Weekly", Tasks.ReshelveBooks_Book06_Lounge);
        SetText(books, "Octopus Digest", Tasks.ReshelveBooks_Book11_Lounge);
        SetText(books, "Kelper Worms", Tasks.ReshelveBooks_Book08_Medical);
        SetText(books, "Nautical Nonsense", Tasks.ReshelveBooks_Book10_Medical);
        SetText(books, "Sea Slugs & You!", Tasks.ReshelveBooks_Book13_Medical);

        Transform drop = transform.Find("DropZoneIndicators");
        SetText(drop, "Benthic Beasts", Tasks.ReshelveBooks_Book02_Lounge);
        SetText(drop, "Ichthyologist Weekly", Tasks.ReshelveBooks_Book06_Lounge);
        SetText(drop, "Octopus Digest", Tasks.ReshelveBooks_Book11_Lounge);
        SetText(drop, "Kelper Worms", Tasks.ReshelveBooks_Book08_Medical);
        SetText(drop, "Nautical Nonsense", Tasks.ReshelveBooks_Book10_Medical);
        SetText(drop, "Sea Slugs & You!", Tasks.ReshelveBooks_Book13_Medical);

        SetText(transform, "Background/A", Tasks.ReshelveBooks_Book01);
        SetText(transform, "Background/C", Tasks.ReshelveBooks_Book03);
        SetText(transform, "Background/E", Tasks.ReshelveBooks_Book04);
        SetText(transform, "Background/F", Tasks.ReshelveBooks_Book05);
        SetText(transform, "Background/J", Tasks.ReshelveBooks_Book07);
        SetText(transform, "Background/M", Tasks.ReshelveBooks_Book09);
        SetText(transform, "Background/P", Tasks.ReshelveBooks_Book12);
        SetText(transform, "Background/V", Tasks.ReshelveBooks_Book14);
        SetText(transform, "Background/W", Tasks.ReshelveBooks_Book15);
    }

    private void Update()
    {
        if (remainingBooks == 0)
        {
            if (minigame.Task != null)
            {
                minigame.Task.NextStep();
            }
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

    [UsedImplicitly]
    private void TryClose()
    {
        if (_stopClose) return;
        minigame.minigameProperties.CloseTask();
    }

    private void SetText(Transform search, string objName, string targetText)
    {
        Transform targetTrans = search.Find(objName);
        if (targetTrans == null)
        {
            return;
        }
        targetTrans.GetComponentInChildren<TextMeshPro>().SetText(targetText);
    }
}
