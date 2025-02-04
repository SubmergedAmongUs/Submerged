using System.Collections.Generic;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using Submerged.Extensions;
using Submerged.Minigames.CustomMinigames.LocateVolcanicActivity.MonoBehaviours;
using Submerged.Minigames.MonoBehaviours;
using Submerged.Localization.Strings;
using TMPro;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.LocateVolcanicActivity;

[RegisterInIl2Cpp]
public sealed class LocateVolcanicActivityMinigame(nint ptr) : Minigame(ptr)
{
    private AudioClip _failClick;
    private int _failures;
    private SpriteRenderer _fullScreen;
    private Transform _mask;

    private MinigameProperties _minigameProperties;
    private int _targetAmount;
    private AudioClip _targetClick;
    private List<SpriteRenderer> _targets;

    public void Reset()
    {
        _failures += 3;

        foreach (SpriteRenderer target in _targets)
        {
            target.gameObject.SetActive(false);
            target.transform.localScale = Vector3.one * 0.25f;
        }

        _targets.Shuffle();
        int count = Mathf.Min(_targetAmount + _failures, _targets.Count);
        MyNormTask.Data[0] = (byte) (_targetAmount + _failures);

        foreach (SpriteRenderer target in _targets.Take(count).ToList())
        {
            target.gameObject.SetActive(true);
        }
    }

    public void Start()
    {
        transform.Find("ScanLines").gameObject.AddComponent<ConstantScroll>();

        if (MyNormTask.Data == null || MyNormTask.Data.Length == 0)
        {
            MyNormTask.Data = new byte[] { 7 };
        }

        _targetAmount = MyNormTask.Data[0];
        _minigameProperties = GetComponent<MinigameProperties>();
        _targetClick = _minigameProperties.audioClips[0];
        _failClick = _minigameProperties.audioClips[1];

        SetupTask();

        #region Setup Fields

        _mask = transform.Find("Lava/EnabledMask");
        _fullScreen = transform.Find("ColorOverlay").GetComponent<SpriteRenderer>();

        _targets.Shuffle();

        foreach (SpriteRenderer target in _targets.Take(_targetAmount).ToList())
        {
            target.gameObject.SetActive(true);
        }

        #endregion

        #region Make Buttons

        GameObject screenBackground = transform.Find("Lava/BackgroundButton").gameObject;
        ClickableSprite screenBackgroundButton = screenBackground.AddComponent<ClickableSprite>();
        screenBackgroundButton.onDown += () =>
        {
            if (amClosing != CloseState.None) return;
            SoundManager.Instance.PlaySound(_failClick, false, 0.5f);
            this.StartCoroutine(CoFlashScreen());
            Reset();
        };

        #endregion

        transform.Find("NewText/EnabledText").GetComponent<TextMeshPro>().text = Tasks.LocateVolcanicActivity_Incomplete;
        transform.Find("NewText/DisabledText").GetComponent<TextMeshPro>().text = Tasks.LocateVolcanicActivity_Complete;
    }

    public void SetupTask()
    {
        // Add buttons to targets
        _targets = transform.Find("Targets").GetComponentsInChildren<SpriteRenderer>().ToList();

        foreach (SpriteRenderer target in _targets)
        {
            target.gameObject.SetActive(false);
            ClickableSprite clickableSprite = target.gameObject.AddComponent<ClickableSprite>();
            clickableSprite.onDown += () =>
            {
                if (amClosing != CloseState.None) return;
                SoundManager.Instance.PlaySound(_targetClick, false, 0.7f);
                this.StartCoroutine(CoClickTarget(target));
            };
        }
    }

    [HideFromIl2Cpp]
    public IEnumerator CoClickTarget(SpriteRenderer target)
    {
        // Hiding the target
        yield return CoScaleInwards(target.transform, 0.25f, 0, 0.1f);
        target.gameObject.SetActive(false);

        // End Game Checking
        if (_targets.All(t => !t.gameObject.activeInHierarchy))
        {
            _failures = 0;

            yield return CoWipeScreen();
        }
    }

    [HideFromIl2Cpp]
    public IEnumerator CoScaleInwards(Transform self, float source, float target, float duration)
    {
        if (!self)
        {
            yield break;
        }

        Vector3 localScale;

        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            localScale.x = localScale.y = localScale.z = Mathf.SmoothStep(source, target, t / duration);
            self.localScale = localScale;

            yield return null;
        }

        localScale.z = target;
        localScale.y = target;
        localScale.x = target;
        self.localScale = localScale;
    }

    [HideFromIl2Cpp]
    public IEnumerator CoWipeScreen()
    {
        if (MyNormTask != null)
        {
            MyNormTask.NextStep();
        }
        StartCoroutine(CoStartClose());

        transform.Find("NewText/EnabledText").gameObject.SetActive(false);
        transform.Find("NewText/DisabledText").gameObject.SetActive(true);

        const float DURATION = 0.5f;
        Vector3 originalPosition = _mask.localPosition;

        for (float t = 0; t < DURATION; t += Time.deltaTime)
        {
            float lerpFloat = Mathf.Lerp(originalPosition.y, 0, t / DURATION);
            _mask.localPosition = new Vector3(0, lerpFloat, 0);

            yield return null;
        }
    }

    [HideFromIl2Cpp]
    public IEnumerator CoFlashScreen()
    {
        _fullScreen.color = new Color(1f, 0f, 0f, 0.37254903f);
        _fullScreen.enabled = !_fullScreen.enabled;

        yield return new WaitForSeconds(0.2f);
        _fullScreen.enabled = !_fullScreen.enabled;
    }
}
