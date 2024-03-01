using System.Collections.Generic;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using Submerged.Localization.Strings;
using TMPro;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.RecordNavBeaconData;

[RegisterInIl2Cpp]
public sealed class SetNavBeaconPart1(nint ptr) : MonoBehaviour(ptr)
{
    private List<TextMeshPro> _bottomTexts;
    private List<TextMeshPro> _codeTexts;

    private SetNavBeaconMinigame _parent;
    private Transform _speechBubble;

    private static string BottomText => Tasks.RecordNavBeaconData_Wait;

    private void Awake()
    {
        _parent = GetComponentInParent<SetNavBeaconMinigame>();
        _codeTexts = transform.Find("Code").GetComponentsInChildren<TextMeshPro>().ToList();
        _bottomTexts = transform.Find("Bottom text").GetComponentsInChildren<TextMeshPro>().ToList();
        _speechBubble = transform.Find("Top speech bubble");
    }

    private void OnEnable()
    {
        if (_parent.MyNormTask.TaskStep == 0)
        {
            this.StartCoroutine(CoShowCode());
        }
        else
        {
            foreach (TextMeshPro textMeshPro in _bottomTexts)
            {
                textMeshPro.text = Tasks.RecordNavBeaconData_Done;
            }

            _speechBubble.localScale = Vector3.one;

            foreach (TextMeshPro textMeshPro in _codeTexts)
            {
                textMeshPro.text = _parent.code;
            }
        }
    }

    [HideFromIl2Cpp]
    private IEnumerator CoShowCode()
    {
        yield return new WaitForSeconds(0.75f);

        while (_bottomTexts[0].text.Length < BottomText.Length)
        {
            foreach (TextMeshPro textMeshPro in _bottomTexts)
            {
                textMeshPro.text += BottomText[textMeshPro.text.Length];
            }

            yield return new WaitForSeconds(0.05f);
        }

        while (_speechBubble.localScale.x < 1)
        {
            yield return null;
            _speechBubble.localScale = new Vector3(_speechBubble.localScale.x + 0.15f, _speechBubble.localScale.y + 0.15f, _speechBubble.localScale.z);
        }

        _speechBubble.localScale = Vector3.one;

        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < _parent.code.Length; i++)
        {
            foreach (TextMeshPro textMeshPro in _codeTexts)
            {
                char[] array = textMeshPro.text.ToCharArray();
                array[i] = _parent.code[i];
                textMeshPro.text = string.Join("", array);
            }

            if (i + 1 == _parent.code.Length && _parent.MyNormTask.taskStep == 0)
            {
                _parent.MyNormTask.NextStep();

                foreach (TextMeshPro textMeshPro in _bottomTexts)
                {
                    textMeshPro.text = Tasks.RecordNavBeaconData_Done;
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
    }
}
