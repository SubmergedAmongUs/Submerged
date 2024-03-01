using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using Submerged.Extensions;
using Submerged.Minigames.CustomMinigames.DiagnoseElevators.Enums;
using Submerged.Minigames.MonoBehaviours;
using Submerged.Localization.Strings;
using TMPro;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.DiagnoseElevators;

[RegisterInIl2Cpp]
public sealed class DiagnoseElevatorsMinigame(nint ptr) : Minigame(ptr)
{
    public ClickableSprite ticketButton;

    public ClickableSprite doorButton;
    public ClickableSprite diagnoseButton;
    public Animator doorAnimator;

    public Transform ticketTransform;
    public Transform ticketFinalTransform;

    public DiagnoseDoorState diagnoseDoorState = DiagnoseDoorState.Closed;
    public bool ticketPrinted;

    public MinigameProperties minigameProperties;

    private bool _clickedDiagnose;
    private bool _ticketClicked;

    private void Start()
    {
        minigameProperties = GetComponent<MinigameProperties>();

        doorAnimator = transform.Find("Door").GetComponent<Animator>();
        doorButton = transform.Find("Door/DoorButton").gameObject.AddComponent<ClickableSprite>();
        doorButton.onDown += ClickDoor;

        diagnoseButton = transform.Find("DiagnoseButton").gameObject.AddComponent<ClickableSprite>();
        diagnoseButton.onDown += ClickDiagnose;

        ticketTransform = transform.Find("TicketDispenser/Ticket");
        ticketFinalTransform = transform.Find("TicketDispenser/TicketEnd");
        ticketButton = ticketTransform.gameObject.AddComponent<ClickableSprite>();
        ticketButton.onDown += ClickTicket;

        if (Random.RandomRangeInt(0, PlayerControl.LocalPlayer.name is "slepy" or "sleepy" ? 5 : 25) == 0)
        {
            ticketTransform.GetComponent<SpriteRenderer>().sprite = minigameProperties.sprites[0];
        }

        GetComponentInChildren<TextMeshPro>().SetText(Tasks.DiagnoseElevators_Diagnose);
    }

    public void ClickDoor()
    {
        if (diagnoseDoorState != DiagnoseDoorState.Closed) return;
        this.StartCoroutine(DoorAnimation());
    }

    [HideFromIl2Cpp]
    public IEnumerator DoorAnimation()
    {
        SoundManager.Instance.PlaySound(minigameProperties.audioClips[0], false);
        diagnoseDoorState = DiagnoseDoorState.Opening;
        doorButton.gameObject.SetActive(false);

        yield return doorAnimator.PlayAndWaitForAnimation("DoorAnimation");
        yield return new WaitForSeconds(0.1f);
        diagnoseDoorState = DiagnoseDoorState.Open;

        yield return null;
    }

    public void ClickDiagnose()
    {
        if (_clickedDiagnose || diagnoseDoorState != DiagnoseDoorState.Open || ticketPrinted) return;
        _clickedDiagnose = true;
        this.StartCoroutine(PrintTicket());
    }

    [HideFromIl2Cpp]
    public IEnumerator PrintTicket()
    {
        SoundManager.Instance.PlaySound(minigameProperties.audioClips[1], false);

        yield return new WaitForSeconds(minigameProperties.audioClips[1].length);
        const float DURATION = 0.75f;
        Vector3 initialPosition = ticketTransform.transform.position;
        Vector3 finalPosition = ticketFinalTransform.transform.position;

        for (float t = 0; t < DURATION;)
        {
            SoundManager.Instance.PlaySound(minigameProperties.audioClips[2], false, 0.33f);
            ticketTransform.transform.position = Vector3.Lerp(initialPosition, finalPosition, t / DURATION);

            if (UnityRandom.Range(0, 1f) > 0.9f) yield return new WaitForSeconds(UnityRandom.Range(0, 0.2f));
            t += Time.deltaTime;

            yield return null;
        }

        SoundManager.Instance.StopSound(minigameProperties.audioClips[2]);
        ticketPrinted = true;
        // TicketTransform.GetComponent<PolygonCollider2D>().enabled = true;
        ClickTicket();
    }

    public void ClickTicket()
    {
        if (!ticketPrinted || _ticketClicked) return;
        this.StartCoroutine(RipTicket());
    }

    [HideFromIl2Cpp]
    public IEnumerator RipTicket()
    {
        if (_ticketClicked) yield break;
        _ticketClicked = true;
        SoundManager.Instance.PlaySound(minigameProperties.audioClips[3], false);

        yield return new WaitForSeconds(minigameProperties.audioClips[3].length * 0.8f);
        ticketTransform.gameObject.SetActive(false);
        MyNormTask!.NextStep();
        MyNormTask!.Data[ConsoleId] = byte.MaxValue;
        StartCoroutine(CoStartClose());
    }
}
