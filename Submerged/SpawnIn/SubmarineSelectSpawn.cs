using System;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using Submerged.BaseGame;
using Submerged.BaseGame.Extensions;
using Submerged.Enums;
using Submerged.Floors;
using Submerged.Localization.Strings;
using Submerged.SpawnIn.Enums;
using TMPro;
using UnityEngine;

namespace Submerged.SpawnIn;

[RegisterInIl2Cpp]
public class SubmarineSelectSpawn(nint ptr) : Minigame(ptr)
{
    public TextMeshPro timerText;
    public TextMeshPro playersText;

    public bool spawnComplete;
    private readonly Color _highlightColor = Color.green;

    protected readonly Controller controller = new();
    private bool _coroutinePlaying;

    private Transform _telescope;
    protected bool clicked;
    protected BoxCollider2D lowerDeckCollider;

    protected Animator[] lowerDeckComponents;
    protected SpriteRenderer[] lowerDeckRenderers;
    protected TextMeshPro lowerDeckText;
    protected Transform lowerDeckTransform;

    protected BoxCollider2D upperDeckCollider;

    protected SpriteRenderer[] upperDeckRenderers;

    protected TextMeshPro upperDeckText;

    protected Transform upperDeckTransform;

    private void Awake()
    {
        Instance = this;

        transform.Find("Scale").localScale = new Vector3(0.85f, 0.85f, 1);

        timerText = transform.Find("Scale/Timer").GetComponentInChildren<TextMeshPro>();
        playersText = transform.Find("Scale/TimerPlayers").GetComponentInChildren<TextMeshPro>();

        upperDeckTransform = transform.Find("Scale/UpperDeck");
        lowerDeckTransform = transform.Find("Scale/LowerDeck");

        upperDeckCollider = upperDeckTransform.GetComponent<BoxCollider2D>();
        lowerDeckCollider = lowerDeckTransform.GetComponent<BoxCollider2D>();

        upperDeckRenderers = transform.Find("Scale/UpperDeck").GetComponentsInChildren<SpriteRenderer>();
        lowerDeckRenderers = transform.Find("Scale/LowerDeck").GetComponentsInChildren<SpriteRenderer>();

        upperDeckText = transform.Find("Scale/UpperDeck").GetComponentInChildren<TextMeshPro>();
        lowerDeckText = transform.Find("Scale/LowerDeck").GetComponentInChildren<TextMeshPro>();

        lowerDeckComponents = transform.Find("Scale/LowerDeck").GetComponentsInChildren<Animator>();

        _telescope = transform.Find("Scale/UpperDeck/Telescope");

        upperDeckText.SetText(Locations.Deck_Upper);
        lowerDeckText.SetText(Locations.Deck_Lower);

        transform.Find("Scale/DontOpenSettings").position = new Vector3(0, 0, HudManager.Instance.FullScreen.transform.position.z - 1);

        // This fixes meeting called during venting animation making the player stuck and unable to vent for the rest of the game
        FloorHandler.LocalPlayer.ClearOverrides();
    }

    private void Start()
    {
        HudManager.Instance.FullScreen.enabled = true;
        HudManager.Instance.FullScreen.color = Color.black;
        StartCoroutine(CoAnimateOpen());

        if (SubmarineSpawnInSystem.Instance.currentState == SpawnInState.Loading)
        {
            ShipStatus.Instance.RpcUpdateSystem(CustomSystemTypes.SpawnIn, 0);
        }
    }

    public virtual void Update()
    {
        if (PlayerControl.LocalPlayer.Data.IsDead && !clicked)
        {
            clicked = true;
            SetColors(upperDeckRenderers, new Color(1, 1, 1, 0.2f));
            SetColors(lowerDeckRenderers, new Color(1, 1, 1, 0.2f));
            upperDeckText.color = new Color(1, 1, 1, 0.2f);
            lowerDeckText.color = new Color(1, 1, 1, 0.2f);
            this.StartCoroutine(CoWaitForOthers());
        }

        switch (SubmarineSpawnInSystem.Instance.currentState)
        {
            case SpawnInState.Loading when !clicked:
                timerText.text = "";
                playersText.text = string.Format(Tasks.SpawnIn_Bottom_Loading,
                                                 SubmarineSpawnInSystem.Instance.GetReadyPlayerAmount(),
                                                 SubmarineSpawnInSystem.Instance.GetTotalPlayerAmount());

                break;

            case SpawnInState.Loading when clicked:
                timerText.text = Tasks.SpawnIn_Top_LoadingClicked;
                playersText.text = string.Format(Tasks.SpawnIn_Bottom_Loading,
                                                 SubmarineSpawnInSystem.Instance.GetReadyPlayerAmount(),
                                                 SubmarineSpawnInSystem.Instance.GetTotalPlayerAmount());

                break;

            case SpawnInState.Spawning when !clicked && SubmarineSpawnInSystem.Instance.timer > 0:
                timerText.text = string.Format(Tasks.SpawnIn_Top_SpawningNotClicked, Mathf.RoundToInt(SubmarineSpawnInSystem.Instance.timer));
                playersText.text = string.Format(Tasks.SpawnIn_Bottom_Spawning,
                                                 SubmarineSpawnInSystem.Instance.GetReadyPlayerAmount(),
                                                 SubmarineSpawnInSystem.Instance.GetTotalPlayerAmount());

                break;

            case SpawnInState.Spawning when !clicked && SubmarineSpawnInSystem.Instance.timer <= 0:
                timerText.text = "";
                playersText.text = "";
                clicked = true;
                ResetHover();
                this.StartCoroutine(CoSelectLevel(UnityRandom.Range(0, 1f) < 0.5f));

                break;

            case SpawnInState.Spawning when clicked && SubmarineSpawnInSystem.Instance.timer > 0:
                timerText.text = string.Format(Tasks.SpawnIn_Top_SpawningClicked, Mathf.RoundToInt(SubmarineSpawnInSystem.Instance.timer));
                playersText.text = string.Format(Tasks.SpawnIn_Bottom_Spawning,
                                                 SubmarineSpawnInSystem.Instance.GetReadyPlayerAmount(),
                                                 SubmarineSpawnInSystem.Instance.GetTotalPlayerAmount());

                break;

            case SpawnInState.Spawning when clicked && SubmarineSpawnInSystem.Instance.timer <= 0:
            // https://youtrack.jetbrains.com/issue/RSRP-492231/
            // ReSharper disable once PatternIsRedundant
            case >= SpawnInState.Done:
                timerText.text = "";
                playersText.text = "";

                break;
        }

        if (clicked) return;
        controller.Update();

        if (!HudManager.Instance.Chat.IsOpenOrOpening && controller.CheckHover(upperDeckCollider))
            Hover(true);
        else if (!HudManager.Instance.Chat.IsOpenOrOpening && controller.CheckHover(lowerDeckCollider))
            Hover(false);
        else
            ResetHover();
    }

    private void OnDestroy()
    {
        Instance = null;
        if (PlayerControl.LocalPlayer.Data.Role.TeamType == RoleTeamTypes.Crewmate)
        {
            GameManager.Instance.LogicMinigame.OnMinigameClose();
        }
    }

    [HideFromIl2Cpp]
    public IEnumerator WaitForFinish()
    {
        yield return null;
        while (!spawnComplete) yield return null;
    }

    protected void ResetHover()
    {
        foreach (SpriteRenderer renderer in upperDeckRenderers) renderer.color = Color.white;
        foreach (SpriteRenderer renderer in lowerDeckRenderers) renderer.color = Color.white;
        upperDeckText.color = Color.white;
        lowerDeckText.color = Color.white;

        foreach (Animator behaviour in lowerDeckComponents) behaviour.enabled = false;

        _coroutinePlaying = false;
    }

    protected void Hover(bool upperSelected)
    {
        foreach (SpriteRenderer renderer in upperDeckRenderers) renderer.color = upperSelected ? _highlightColor : Color.white;
        foreach (SpriteRenderer renderer in lowerDeckRenderers) renderer.color = !upperSelected ? _highlightColor : Color.white;
        upperDeckText.color = upperSelected ? _highlightColor : Color.white;
        lowerDeckText.color = !upperSelected ? _highlightColor : Color.white;

        foreach (Animator behaviour in lowerDeckComponents) behaviour.enabled = !upperSelected;

        if (upperSelected && !_coroutinePlaying)
        {
            this.StartCoroutine(CoHoverUpper());
        }

        if (Input.GetMouseButtonUp(0))
        {
            clicked = true;
            ResetHover();
            this.StartCoroutine(CoSelectLevel(upperSelected));
        }
    }

    [HideFromIl2Cpp]
    private IEnumerator CoHoverUpper()
    {
        _coroutinePlaying = true;

        const float DURATION = 0.1f;

        for (float i = 0; _coroutinePlaying; i += Time.deltaTime)
        {
            _telescope.localEulerAngles = Vector3.Lerp(Vector3.zero, new Vector3(0, 0, -10f), i / DURATION);

            yield return null;
        }

        _telescope.localEulerAngles = Vector3.zero;
    }

    [HideFromIl2Cpp]
    protected IEnumerator CoSelectLevel(bool upperSelected)
    {
        PlayerControl.LocalPlayer.moveable = false;

        for (int i = 0; i < GameData.Instance.PlayerCount; i++)
        {
            PlayerControl @object = GameData.Instance.AllPlayers._items[i].Object;

            if (@object)
            {
                @object.moveable = true;
                @object.NetTransform.enabled = true;
                @object.MyPhysics.enabled = true;
                @object.MyPhysics.Awake();
                @object.MyPhysics.ResetMoveState();
                @object.Collider.enabled = true;
                ShipStatus.Instance.SpawnPlayer(@object, GameData.Instance.PlayerCount, true);
            }
        }

        FloorHandler.LocalPlayer.RpcRequestChangeFloor(upperSelected);

        HudManager.Instance.PlayerCam.Locked = false;
        HudManager.Instance.PlayerCam.SnapToTarget();

        yield return DoPreAnimationLogicWhenFloorClicked();

        yield return PlaySelectAnimation(upperSelected);

        if (SubmarineSpawnInSystem.Instance.currentState != SpawnInState.Done)
        {
            while (SubmarineSpawnInSystem.Instance.currentState != SpawnInState.Spawning) yield return null;
            ShipStatus.Instance.RpcUpdateSystem(CustomSystemTypes.SpawnIn, (byte) (upperSelected ? 1 : 0));
        }

        while (SubmarineSpawnInSystem.Instance.currentState != SpawnInState.Done) yield return null;

        yield return PlayFadeOutAnimation(upperSelected);

        ControllerManager.Instance.CloseAndResetAll();
        HudManager.Instance.PlayerCam.Locked = false;
        HudManager.Instance.PlayerCam.SnapToTarget();

        spawnComplete = true;

        yield return null;

        Cleanup(GameManager.Instance.IsNormal());
    }

    [HideFromIl2Cpp]
    protected IEnumerator CoWaitForOthers()
    {
        for (int i = 0; i < GameData.Instance.PlayerCount; i++)
        {
            PlayerControl @object = GameData.Instance.AllPlayers._items[i].Object;

            if (@object)
            {
                @object.moveable = true;
                @object.NetTransform.enabled = true;
                @object.MyPhysics.enabled = true;
                @object.MyPhysics.Awake();
                @object.MyPhysics.ResetMoveState();
                @object.Collider.enabled = true;
                ShipStatus.Instance.SpawnPlayer(@object, GameData.Instance.PlayerCount, true);
            }
        }

        HudManager.Instance.PlayerCam.Locked = false;
        HudManager.Instance.PlayerCam.SnapToTarget();

        while (SubmarineSpawnInSystem.Instance.currentState != SpawnInState.Done) yield return null;

        yield return PlayFinishAnimationWhenWaitingForOthers();

        HudManager.Instance.PlayerCam.Locked = false;
        HudManager.Instance.PlayerCam.SnapToTarget();

        spawnComplete = true;

        yield return null;

        Cleanup(GameManager.Instance.IsNormal());
    }

    [HideFromIl2Cpp]
    protected IEnumerator PlaySelectAnimation(bool upperSelected, Color? baseColorArg = null)
    {
        Color baseColor = baseColorArg ?? Color.white;

        foreach (Animator behaviour in lowerDeckComponents) behaviour.enabled = !upperSelected;

        const float ALPHA_DURATION = 0.15f;

        for (float t = 0; t < ALPHA_DURATION; t += Time.deltaTime)
        {
            Color color = new(baseColor.r, baseColor.g, baseColor.b, (ALPHA_DURATION - t) / ALPHA_DURATION * baseColor.a);
            SetColors(upperDeckRenderers, upperSelected ? baseColor : color);
            SetColors(lowerDeckRenderers, !upperSelected ? baseColor : color);
            upperDeckText.color = upperSelected ? baseColor : color;
            lowerDeckText.color = !upperSelected ? baseColor : color;

            yield return null;
        }

        SetColors(upperDeckRenderers, upperSelected ? baseColor : Color.clear);
        SetColors(lowerDeckRenderers, !upperSelected ? baseColor : Color.clear);
        upperDeckText.color = upperSelected ? baseColor : Color.clear;
        lowerDeckText.color = !upperSelected ? baseColor : Color.clear;

        yield return new WaitForSeconds(0.25f);

        // -6.16 Right
        // 5.74 Left

        Vector3 originalPos = Vector3.zero;
        Vector3 targetPosition = originalPos;
        targetPosition.x = upperSelected ? -6.16f : 5.74f;
        const float MOVE_DURATION = 0.20f;

        for (float t = 0; t < MOVE_DURATION; t += Time.deltaTime)
        {
            Vector3 lerpPos = Vector3.Lerp(originalPos, targetPosition, t / MOVE_DURATION);
            upperDeckTransform.localPosition = lerpPos;
            lowerDeckTransform.localPosition = lerpPos;

            yield return null;
        }

        upperDeckTransform.localPosition = targetPosition;
        lowerDeckTransform.localPosition = targetPosition;
    }

    [HideFromIl2Cpp]
    protected IEnumerator PlayFadeOutAnimation(bool upperSelected, Color? baseColorArg = null)
    {
        Color baseColor = baseColorArg ?? Color.white;

        const float ALPHA_DURATION = 0.15f;

        for (float t = 0; t < ALPHA_DURATION; t += Time.deltaTime)
        {
            Color color = new(baseColor.r, baseColor.g, baseColor.b, (ALPHA_DURATION - t) / ALPHA_DURATION * baseColor.a);

            SetColors(upperDeckRenderers, upperSelected ? color : Color.clear);
            SetColors(lowerDeckRenderers, !upperSelected ? color : Color.clear);
            upperDeckText.color = upperSelected ? color : Color.clear;
            lowerDeckText.color = !upperSelected ? color : Color.clear;

            yield return null;
        }

        SetColors(upperDeckRenderers, Color.clear);
        SetColors(lowerDeckRenderers, Color.clear);
        upperDeckText.color = Color.clear;
        lowerDeckText.color = Color.clear;
    }

    [HideFromIl2Cpp]
    protected virtual IEnumerator PlayFinishAnimationWhenWaitingForOthers()
    {
        yield break;
    }

    [HideFromIl2Cpp]
    protected virtual IEnumerator DoPreAnimationLogicWhenFloorClicked()
    {
        yield break;
    }

    [BaseGameCode(LastChecked.v2024_3_5, "Part of this method is from ExileController.ReEnableGameplay")]
    private void Cleanup(bool unfade = true)
    {
        if (unfade) HudManager.Instance.StartCoroutine(HudManager.Instance.CoFadeFullScreen(Color.black, Color.clear));

        PlayerControl.LocalPlayer.moveable = true;
        PlayerControl.LocalPlayer.SetKillTimer(GameManager.Instance.LogicOptions.GetKillCooldown());
        ShipStatus.Instance.EmergencyCooldown = GameManager.Instance.LogicOptions.GetEmergencyCooldown();
        HudManager.Instance.PlayerCam.Locked = false;
        HudManager.Instance.SetHudActive(true);

        int emergencyCooldown = GameManager.Instance.LogicOptions.GetEmergencyCooldown();
        SetSabotageTimers(Math.Clamp(emergencyCooldown - 5, 0, 15));

        Destroy(gameObject);
    }

    private static void SetSabotageTimers(float timer)
    {
        // Base game maps don't have any sabotage cooldown after meetings
        // Apparently submerged does. Idk why

        SabotageSystemType saboSystem = ShipStatus.Instance.Systems[SystemTypes.Sabotage].Cast<SabotageSystemType>();
        saboSystem.IsDirty = true;
        // AccessTools.PropertySetter(typeof(SabotageSystemType), nameof(ISystemType.IsDirty)).Invoke(saboSystem, new object[] { true });
        // saboSystem.ForceSabTime(0);
        saboSystem.Timer = timer;

        DoorsSystemType doorSystem = ShipStatus.Instance.Systems[SystemTypes.Doors].Cast<DoorsSystemType>();
        doorSystem.IsDirty = true;
        // AccessTools.PropertySetter(typeof(DoorsSystemType), nameof(ISystemType.IsDirty)).Invoke(doorSystem, new object[] { true });
        doorSystem.timers[CustomSystemTypes.Observatory] = timer;
        doorSystem.timers[CustomSystemTypes.Research] = timer;
        doorSystem.timers[CustomSystemTypes.UpperLobby] = timer;
        doorSystem.timers[SystemTypes.Admin] = timer;
        doorSystem.timers[SystemTypes.Comms] = timer;
        doorSystem.timers[SystemTypes.Electrical] = timer;
        doorSystem.timers[SystemTypes.Engine] = timer;
        doorSystem.timers[SystemTypes.Medical] = timer;
        doorSystem.timers[SystemTypes.MeetingRoom] = timer;
        doorSystem.timers[SystemTypes.Security] = timer;
        doorSystem.timers[SystemTypes.Storage] = timer;
    }

    public override void Close()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) this.BaseClose();
    }

    [HideFromIl2Cpp]
    protected void SetColors(IEnumerable<SpriteRenderer> renderers, Color color)
    {
        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.color = color;
        }
    }
}
