using System;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using Submerged.Localization.Strings;
using Submerged.Map;
using TMPro;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.RecordNavBeaconData;

[RegisterInIl2Cpp]
public sealed class SetNavBeaconPart2(nint ptr) : MonoBehaviour(ptr)
{
    private static readonly KeyCode[] _validKeyCodes = Enum.GetValues(typeof(KeyCode))
                                                           .OfType<KeyCode>()
                                                           .Where(k => k is >= KeyCode.A and <= KeyCode.Z or >= KeyCode.Alpha0 and <= KeyCode.Alpha9)
                                                           .ToArray();

    private Transform _blinkyBar;

    private bool _failing;

    private SetNavBeaconMinigame _parent;
    private TextMeshPro _text;

    private void Awake()
    {
        _parent = GetComponentInParent<SetNavBeaconMinigame>();
        _blinkyBar = transform.Find("Blinking bar");
        _text = transform.Find("Text").GetComponent<TextMeshPro>();
        transform.Find("Input boxes");

        TextMeshPro loreText = transform.Find("TranslationText").GetComponent<TextMeshPro>();
        loreText.text = Tasks.RecordNavBeaconData_Enter;
        TextMeshPro basegameText = MapLoader.Skeld.MapPrefab.GetComponentInChildren<TextMeshPro>();
        loreText.font = basegameText.font;
        loreText.fontMaterial = basegameText.fontMaterial;
    }

    private void Update()
    {
        if (_failing) return;

        if (_text.text.Length > 0 && Input.GetKeyDown(KeyCode.Backspace))
        {
            _text.text = _text.text.Substring(0, _text.text.Length - 1);
        }

        foreach (KeyCode keyCode in _validKeyCodes)
        {
            if (_text.text.Length < 3 && Input.GetKeyDown(keyCode))
            {
                _text.text += keyCode.ToString().Length > 1 ? keyCode.ToString().Substring(5) : keyCode.ToString();

                if (_text.text.Length == 3)
                {
                    if (_text.text == _parent.code)
                    {
                        _parent.MyNormTask.taskStep = 1;
                        _parent.MyNormTask.NextStep();
                        _parent.StartCoroutine(_parent.CoStartClose());
                    }
                    else
                    {
                        this.StartCoroutine(Fail());
                    }
                }
            }
        }

        _blinkyBar.localPosition = new Vector3(Mathf.Clamp(_text.text.Length, 0, 2) * 4.21f, 0, _blinkyBar.localPosition.z);
    }

    private void OnEnable()
    {
        this.StartCoroutine(CoBlink());
    }

    [HideFromIl2Cpp]
    private IEnumerator Fail()
    {
        string ogText = _text.text;
        _failing = true;
        _text.text = $"<color=red>{ogText}</color>";

        yield return new WaitForSeconds(0.1f);
        _text.text = $"<color=black>{ogText}</color>";

        yield return new WaitForSeconds(0.1f);
        _text.text = $"<color=red>{ogText}</color>";

        yield return new WaitForSeconds(0.1f);
        _text.text = $"{ogText}";
        _failing = false;
    }

    [HideFromIl2Cpp]
    private IEnumerator CoBlink()
    {
        while (true)
        {
            _blinkyBar.gameObject.SetActive(false);

            yield return new WaitForSecondsRealtime(0.5f);
            _blinkyBar.gameObject.SetActive(true);

            yield return new WaitForSecondsRealtime(0.5f);
        }
    }
}
