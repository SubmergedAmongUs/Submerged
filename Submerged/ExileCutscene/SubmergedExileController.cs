using System;
using BepInEx.Unity.IL2CPP.Utils;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Attributes;
using PowerTools;
using Reactor.Utilities.Attributes;
using Submerged.BaseGame;
using Submerged.ExileCutscene.Patches;
using Submerged.Systems.BoxCat;
using UnityEngine;

namespace Submerged.ExileCutscene;

[RegisterInIl2Cpp]
public sealed class SubmergedExileController(nint ptr) : ExileController(ptr)
{
    private const float MULTIPLIER = 6;
    public Transform textTransform;

    public FishController fish;

    public bool started;
    private Transform _bubbles;

    private Transform _darkness;
    private Vector3 _finalPosition;

    private Transform _leftCliff;

    private Camera _mainCam;

    private Vector3 _originalPosition;
    private Transform _rightCliff;
    private bool _textRoutine;

    private float _timer;

    private new void Awake()
    {
        base.Awake();

        _mainCam = HudManager.Instance.UICamera;

        _originalPosition = transform.localPosition;
        _finalPosition = new Vector3(_originalPosition.x, _originalPosition.y + 6f, _originalPosition.z);

        _leftCliff = transform.Find("Cliffs/LeftCliff");
        _rightCliff = transform.Find("Cliffs/RightCliff");

        float width = _mainCam.orthographicSize * _mainCam.aspect;
        _leftCliff.transform.localPosition = new Vector3(-width, 3, 0);
        _rightCliff.transform.localPosition = new Vector3(width, 3, 0);

        _darkness = transform.Find("Darkness");
        textTransform = transform.Find("Text");

        _bubbles = transform.Find("BubbleSystem");
    }

    private void Start()
    {
        SubmarineBoxCatSystem.Instance.MoveCat();

        transform.Find("Sea").gameObject.AddComponent<ExileParallax>();

        fish = transform.Find("Fish").gameObject.AddComponent<FishController>();
        fish.anim = fish.GetComponent<SpriteAnim>();
        fish.bubbles = _bubbles.GetComponent<ParticleSystem>();
        fish.swim = fish.transform.GetComponentInChildren<PetBehaviour>(true).walkClip;
        fish.bite = fish.transform.GetComponentInChildren<PetBehaviour>(true).scaredClip;
        fish.targetObj = Player.transform;

        transform.localPosition = Vector3.Lerp(_originalPosition, _finalPosition, _timer / (2 * Duration));
    }

    private void Update()
    {
        if (!started) return;

        _timer += Time.deltaTime;

        Vector3 darkPos = _darkness.localPosition;
        darkPos.y = MULTIPLIER * _timer;
        _darkness.localPosition = darkPos;

        transform.localPosition = Vector3.Lerp(_originalPosition, _finalPosition, _timer / (2 * Duration));
        MovePlayer();

        float timerAmount = !initData.networkedPlayer ? 4.25f : 6f;

        if (_timer > timerAmount && !_textRoutine)
        {
            _textRoutine = true;
            this.StartCoroutine(HandleText());
        }
    }

    private void LateUpdate()
    {
        if (!started) return;

        textTransform.position = HudManager.Instance.UICamera.transform.position + new Vector3(0, 0, -100);
        _bubbles.transform.position = Player.transform.position;
    }

    [HideFromIl2Cpp]
    private IEnumerator WaitForFadeAndAnimate()
    {
        bool isPlayerEjected = initData != null && initData.networkedPlayer;
        if (!isPlayerEjected)
        {
            Player.gameObject.SetActive(false);
            _bubbles.gameObject.SetActive(false);
            _timer = 3f;
        }

        yield return HudManager.Instance.CoFadeFullScreen(Color.black, Color.clear);

        if (isPlayerEjected) fish.started = true;
        started = true;
    }

    private void MovePlayer()
    {
        float distance = _mainCam.orthographicSize + 1f;

        Vector2 top = Vector2.up * distance;
        Vector2 bottom = Vector2.down * distance;

        float amountDone = _timer / Duration;
        float amountDoneFuture = amountDone + 0.15f;

        top.x = Mathf.Lerp(0.25f, 0.15f, amountDone) * Mathf.Sin(_timer * Mathf.Lerp(1f, 2f, amountDoneFuture)) * 1.33f;
        bottom.x = Mathf.Lerp(0.25f, 0.15f, amountDone) * Mathf.Sin(_timer * Mathf.Lerp(1f, 2f, amountDoneFuture)) * 1.33f;
        Vector2 anchorPos = Vector2.Lerp(top, bottom, amountDoneFuture);

        top.x = Mathf.Lerp(0.75f, 0.4f, amountDone) * Mathf.Sin(_timer * Mathf.Lerp(1f, 2f, amountDone)) * 1.33f;
        bottom.x = Mathf.Lerp(0.75f, 0.4f, amountDone) * Mathf.Sin(_timer * Mathf.Lerp(1f, 2f, amountDone)) * 1.33f;
        Vector2 playerPos = Vector2.Lerp(top, bottom, amountDone);

        Vector3 difference = anchorPos - playerPos;
        float angle = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;

        Player.transform.rotation = Quaternion.AngleAxis(angle + 90, Vector3.forward);
        Player.transform.localPosition = playerPos;
    }

    [HideFromIl2Cpp]
    private IEnumerator HandleText()
    {
        const float TEXT_DUR = 2f;
        int previousValue = 0;

        string completeStr = BaseGameResolvingPatches.LastExileText;

        Warning("Displaying SubmergedExileController text: " + completeStr);

        for (float t = 0; t < TEXT_DUR; t += Time.deltaTime)
        {
            if (!string.IsNullOrWhiteSpace(completeStr))
            {
                int amountOfText = Math.Clamp(Mathf.FloorToInt(completeStr.Length * t / TEXT_DUR), 0, completeStr.Length);
                Text.text = completeStr.Substring(0, amountOfText);
                Text.gameObject.SetActive(true);

                if (previousValue != amountOfText)
                {
                    previousValue = amountOfText;

                    if (completeStr[Math.Clamp(amountOfText - 1, 0, completeStr.Length - 1)] != ' ')
                    {
                        SoundManager.Instance.PlaySoundImmediate(TextSound, false, 0.8f);
                    }
                }
            }

            yield return null;
        }

        Text.text = completeStr;

        yield return new WaitForSeconds(0.5f);

        if (GameManager.Instance.LogicOptions.GetConfirmImpostor())
        {
            // ImpostorText.text = ExileController_Begin_Patch.ImpostorText;
            ImpostorText.gameObject.SetActive(true);
        }

        yield return Effects.Bloop(0f, ImpostorText.transform);
        yield return new WaitForSeconds(1);
        yield return HudManager.Instance.CoFadeFullScreen(Color.clear, Color.black);
        yield return WrapUpAndSpawn();
    }

    // Cleanup this WrapUpAndSpawn method
    [HideFromIl2Cpp]
    [BaseGameCode(LastChecked.v2025_5_20, "Similar to AirshipExileController.WrapUpAndSpawn")]
    public IEnumerator WrapUpAndSpawn()
    {
        if (initData != null && initData.networkedPlayer)
        {
            PlayerControl @object = initData.networkedPlayer.Object;

            if (@object)
            {
                @object.Exiled();
            }

            initData.networkedPlayer.IsDead = true;
        }

        if (TutorialManager.InstanceExists || (GameManager.Instance && !GameManager.Instance.LogicFlow.IsGameOverDueToDeath()))
        {
            ShipStatus.Instance.StartCoroutine(ShipStatus.Instance.PrespawnStep());

            // We can't use ReEnableGameplay because it fades the screen to clear and we don't want that
            PlayerControl.LocalPlayer.SetKillTimer(GameManager.Instance.LogicOptions.GetKillCooldown());
            ShipStatus.Instance.EmergencyCooldown = GameManager.Instance.LogicOptions.GetEmergencyCooldown();
            HudManager.Instance.PlayerCam.Locked = false;
            HudManager.Instance.SetMapButtonEnabled(true);
            HudManager.Instance.SetHudActive(true);
            ControllerManager.Instance.CloseAndResetAll();
        }

        Destroy(gameObject);

        yield break;
    }

    public override CppIEnumerator Animate() => WaitForFadeAndAnimate().WrapToIl2Cpp();
}
