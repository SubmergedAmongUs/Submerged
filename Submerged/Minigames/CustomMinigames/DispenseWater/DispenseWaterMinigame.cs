using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using Submerged.BaseGame.Extensions;
using Submerged.Extensions;
using Submerged.Minigames.MonoBehaviours;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.DispenseWater;

[RegisterInIl2Cpp]
public sealed class DispenseWaterMinigame(nint ptr) : Minigame(ptr)
{
    public MinigameProperties minigameProperties;
    public Animator waterAnimation;
    public ClickableSprite button;
    public GameObject onButton;
    public GameObject offButton;

    public Transform cap;
    public Transform capTarget;
    public Draggable capDraggable;
    private AudioSource _audioSource;

    private bool _filling;

    private Coroutine _fillRoutine;

    private SpriteRenderer _renderer;
    private float _timer;

    private void Start()
    {
        minigameProperties = GetComponent<MinigameProperties>();
        _audioSource = SoundManager.Instance.PlaySound(minigameProperties.audioClips[0], false);
        _audioSource.Pause();

        waterAnimation = transform.Find("Water").GetComponent<Animator>();
        button = transform.Find("Front/Button").gameObject.AddComponent<ClickableSprite>();
        onButton = button.transform.Find("OnButton").gameObject;
        offButton = button.transform.Find("OffButton").gameObject;

        cap = transform.Find("Cap");
        capTarget = transform.Find("CapTarget");
        capDraggable = cap.gameObject.AddComponent<Draggable>();

        waterAnimation.StopPlayback();
        waterAnimation.gameObject.GetComponent<SpriteRenderer>().sprite = null;

        _renderer = waterAnimation.GetComponent<SpriteRenderer>();
        _renderer.enabled = false;

        button.onDown += () =>
        {
            _renderer.enabled = true;
            _filling = true;
            _fillRoutine = this.StartCoroutine(Fill());
            onButton.SetActive(true);
            offButton.SetActive(false);
        };

        button.onUp += () =>
        {
            onButton.SetActive(false);
            offButton.SetActive(true);
            _filling = false;
            StopCoroutine(_fillRoutine);
            if (waterAnimation.GetCurrentStateName(0) != "EndWater") waterAnimation.Play("EndWater");
        };
    }

    private void Update()
    {
        if (_filling)
        {
            if (_audioSource != null)
            {
                _audioSource.UnPause();
            }
            _timer += Time.deltaTime;

            if (_timer > 5f)
            {
                button.onUp.Invoke();
                if (_audioSource != null)
                {
                    _audioSource.Stop();
                }
                Destroy(button);
            }
        }
        else
        {
            if (_audioSource != null)
            {
                _audioSource.Pause();
            }
            if (waterAnimation.GetCurrentStateName(0) != "EndWater") waterAnimation.Play("EndWater");
        }

        if (_timer > 5f && amClosing == CloseState.None)
        {
            if (((Vector2) cap.position - (Vector2) capTarget.position).sqrMagnitude < 0.5)
            {
                cap.position = capTarget.position;
                capDraggable.forceStop = true;
                SoundManager.Instance.PlaySound(minigameProperties.audioClips[1], false);
                if (MyNormTask != null)
                {
                    MyNormTask.NextStep();
                }
                StartCoroutine(CoStartClose());
            }
        }
    }

    public void CustomClose()
    {
        if (!capDraggable.dragging)
        {
            _audioSource.Stop();
            GetComponent<MinigameProperties>().CloseTask();
        }
    }

    [HideFromIl2Cpp]
    private IEnumerator Fill()
    {
        yield return waterAnimation.PlayAndWaitForAnimation("StartWater");
        waterAnimation.Play("MidWater");
    }

    public override void Close()
    {
        try
        {
            if (_audioSource) _audioSource.Stop();
        }
        catch (ObjectCollectedException)
        {
            // Ignore
        }

        this.BaseClose();
    }
}
