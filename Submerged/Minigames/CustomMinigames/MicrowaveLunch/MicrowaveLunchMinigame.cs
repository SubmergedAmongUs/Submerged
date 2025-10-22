using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using Submerged.BaseGame.Extensions;
using Submerged.Extensions;
using Submerged.Minigames.MonoBehaviours;
using Submerged.Localization.Strings;
using TMPro;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.MicrowaveLunch;

[RegisterInIl2Cpp]
public sealed class MicrowaveLunchMinigame(nint ptr) : Minigame(ptr)
{
    private readonly List<int> _enteredNumber = [];

    private AudioSource _audio;
    private GameObject _background;

    private AudioClip _beep;
    private Transform[] _buttons;

    private Transform _controlPanel;
    private int _cookTimeSeconds;
    private TextMeshPro _cookTimeText;

    private bool _dontUpdate;
    private int _foodItem;

    private GameObject[] _foodItems;
    // private float Timer;

    private GameObject _loreTextGreen;
    private GameObject _loreTextPink;

    private MinigameProperties _minigameProperties;
    private GameObject _notesMask;
    private GameObject _openDoor;
    private GameObject _screen;
    private GameObject _screenLight;

    private TextMeshPro _timerText;

    private void Start()
    {
        _minigameProperties = GetComponent<MinigameProperties>();
        _foodItems = transform.Find("InsideTheMicrowave/Food").GetChildren().Select(t => t.gameObject).ToArray();

        _loreTextGreen = transform.Find("StickyNotes/LoreTextGreen").gameObject;
        _loreTextPink = transform.Find("StickyNotes/LoreTextPink").gameObject;
        _notesMask = transform.Find("StickyNotes/NotesMask").gameObject;

        _screen = transform.Find("MicrowaveGlass").gameObject;
        _screenLight = _screen.transform.Find("Light").gameObject;

        _background = transform.Find("Background").gameObject;
        _openDoor = transform.Find("OpenDoor").gameObject;

        _beep = _minigameProperties.audioClips[2];
        _audio = SoundManager.Instance.PlayDynamicSound("Hum",
                                                        _minigameProperties.audioClips[0],
                                                        true,
                                                        new Action<AudioSource, float>((source, _) => { source.volume = Mathf.Lerp(0, 0.5f, (_cookTimeSeconds - MyNormTask.TaskTimer) / 4f); }),
                                                        SoundManager.Instance.SfxChannel);
        _audio.Pause();

        MicrowaveLunchTask task = MyNormTask.Cast<MicrowaveLunchTask>();

        _foodItem = task.customData[0];
        _foodItems[_foodItem].SetActive(true);

        _cookTimeSeconds = task.customData[1];
        _cookTimeText = transform.Find("StickyNotes/CookTime").GetComponent<TextMeshPro>();
        _cookTimeText.text = string.Format(Tasks.MicrowaveLunch_CookTime, _cookTimeSeconds);

        _controlPanel = transform.Find("ControlPanel");
        _timerText = _controlPanel.Find("TimerText").GetComponent<TextMeshPro>();
        _buttons = _controlPanel.Find("Buttons").GetChildren();

        foreach (Transform button in _buttons)
        {
            ClickableSprite clickableSprite = button.gameObject.AddComponent<ClickableSprite>();

            switch (button.name.Length)
            {
                case 1:
                    clickableSprite.onDown += () => NumberButtonPress(int.Parse(button.name));

                    break;
                case 4:
                    clickableSprite.onDown += CookButtonPress;

                    break;
                case 5:
                    clickableSprite.onDown += ClearButtonPress;

                    break;
            }
        }

        transform.Find("StickyNotes/LoreTextPink").GetComponent<TextMeshPro>().text = Tasks.MicrowaveLunch_PostItPink;
        transform.Find("StickyNotes/LoreTextGreen").GetComponent<TextMeshPro>().text = Tasks.MicrowaveLunch_PostItGreen;
    }

    private void Update()
    {
        if (_dontUpdate) return;

        _loreTextGreen.SetActive(MyNormTask.TimerStarted != NormalPlayerTask.TimerState.Finished);
        _loreTextPink.SetActive(MyNormTask.TimerStarted != NormalPlayerTask.TimerState.Finished);
        _screen.SetActive(MyNormTask.TimerStarted != NormalPlayerTask.TimerState.Finished);
        _screenLight.SetActive(MyNormTask.TimerStarted == NormalPlayerTask.TimerState.Started);
        _background.SetActive(MyNormTask.TimerStarted != NormalPlayerTask.TimerState.Finished);
        _openDoor.SetActive(MyNormTask.TimerStarted == NormalPlayerTask.TimerState.Finished);
        _notesMask.SetActive(MyNormTask.TimerStarted == NormalPlayerTask.TimerState.Finished);

        if (MyNormTask.TimerStarted == NormalPlayerTask.TimerState.NotStarted)
        {
            string timerText = "";
            foreach (int num in _enteredNumber) timerText += num;
            while (timerText.Length < 4) timerText = 0 + timerText;
            _timerText.text = timerText.Insert(2, ":");
        }
        else
        {
            _audio.UnPause();
            int timeRemaining = (int) MyNormTask.TaskTimer;
            timeRemaining = timeRemaining < 0 ? 0 : timeRemaining;
            string timerText = $"{timeRemaining / 60:00}:{timeRemaining % 60:00}";
            _timerText.text = timerText;

            if (MyNormTask.TimerStarted == NormalPlayerTask.TimerState.Finished)
            {
                this.StartCoroutine(Complete());
            }
        }
    }

    private void NumberButtonPress(int number)
    {
        _enteredNumber.Add(number);

        if (_enteredNumber.Count < 5)
        {
            SoundManager.Instance.PlaySound(_beep, false, 0.5f);

            return;
        }

        this.StartCoroutine(Error());
    }

    private void ClearButtonPress()
    {
        SoundManager.Instance.PlaySound(_beep, false, 0.5f);
        _enteredNumber.Clear();
    }

    private void CookButtonPress()
    {
        while (_enteredNumber.Count < 4) _enteredNumber.Insert(0, 0);
        int mins = _enteredNumber[0] * 10 + _enteredNumber[1];
        int secs = _enteredNumber[2] * 10 + _enteredNumber[3];

        int totalSecs = secs + 60 * mins;

        if (totalSecs != _cookTimeSeconds)
        {
            this.StartCoroutine(Error());

            return;
        }

        SoundManager.Instance.PlaySound(_beep, false, 0.5f);

        MyNormTask.TaskTimer = totalSecs;
        MyNormTask.ShowTaskStep = false;
        MyNormTask.ShowTaskTimer = true;
        MyNormTask.TimerStarted = NormalPlayerTask.TimerState.Started;
    }

    [HideFromIl2Cpp]
    private IEnumerator Error()
    {
        _dontUpdate = true;
        _timerText.text = "00:00";
        SoundManager.Instance.PlaySound(_beep, false, 0.5f);

        yield return new WaitForSeconds(0.1f);
        _timerText.text = "";

        yield return new WaitForSeconds(0.1f);
        SoundManager.Instance.PlaySound(_beep, false, 0.5f);
        _timerText.text = "00:00";

        yield return new WaitForSeconds(0.1f);
        _timerText.text = "";

        yield return new WaitForSeconds(0.1f);
        SoundManager.Instance.PlaySound(_beep, false, 0.5f);
        _dontUpdate = false;
        ClearButtonPress();
    }

    [HideFromIl2Cpp]
    private IEnumerator Complete()
    {
        _audio.Stop();
        _dontUpdate = true;
        _foodItems[_foodItem].AddComponent<ClickableSprite>().onDown += () => this.StartCoroutine(CompleteTask(_foodItems[_foodItem]));
        _dontUpdate = true;
        _timerText.text = "00:00";
        SoundManager.Instance.PlaySound(_beep, false, 0.5f);

        yield return new WaitForSeconds(0.25f);
        _timerText.text = "";

        yield return new WaitForSeconds(0.25f);
        SoundManager.Instance.PlaySound(_beep, false, 0.5f);
        _timerText.text = "00:00";

        yield return new WaitForSeconds(0.25f);
        _timerText.text = "";

        yield return new WaitForSeconds(0.25f);
        SoundManager.Instance.PlaySound(_beep, false, 0.5f);
        _dontUpdate = false;
    }

    [HideFromIl2Cpp]
    private IEnumerator CompleteTask(GameObject item)
    {
        MyNormTask.NextStep();
        StartCoroutine(CoStartClose());

        SpriteRenderer rend = item.GetComponent<SpriteRenderer>();

        for (float t = 0; t < 0.5f; t += Time.deltaTime)
        {
            rend.color = new Color(1, 1, 1, (0.5f - t) * 2);

            yield return null;
        }
    }

    public override void Close()
    {
        _audio.Stop();
        this.BaseClose();
    }
}
