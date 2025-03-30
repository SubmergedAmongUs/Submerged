using Reactor.Utilities.Attributes;
using Submerged.Extensions;
using Submerged.Minigames.MonoBehaviours;
using Submerged.Localization.Strings;
using TMPro;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.IdentifySpecimen;

[RegisterInIl2Cpp]
public sealed class IdentifySpecimenMinigame(nint ptr) : Minigame(ptr)
{
    private Transform[] _jars;

    private void Start()
    {
        _jars = transform.Find("Jars").GetChildren();
        Transform selectedJar = _jars.Random();

        foreach (Transform c in transform.Find("ClipboardIcons").GetChildren())
        {
            c.gameObject.SetActive(false);
        }

        transform.Find($"ClipboardIcons/{selectedJar.name}").gameObject.SetActive(true);
        selectedJar.gameObject.AddComponent<ClickableSprite>().onDown += CompleteTask;

        transform.Find("ClipboardIcons/Puffer/Text").GetComponent<TextMeshPro>().text = Tasks.IdentifySpecimen_Pufferfish;
    }

    private void CompleteTask()
    {
        if (amClosing != CloseState.None) return;
        if (MyNormTask != null)
        {
            MyNormTask.NextStep();
        }
        StartCoroutine(CoStartClose());
    }
}
