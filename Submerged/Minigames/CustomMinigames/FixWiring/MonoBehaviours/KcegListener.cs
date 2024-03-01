using System.Collections.Generic;
using Il2CppInterop.Runtime;
using Reactor.Utilities.Attributes;
using Submerged.Map;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.FixWiring.MonoBehaviours;

[RegisterInIl2Cpp]
public sealed class KcegListener : MonoBehaviour
{
    public const string PLAYER_PREFS_KEY = "HasDoneKCEG";

    public static readonly List<KeyCode[]> requiredInputs = new()
    {
        new[] { KeyCode.UpArrow, KeyCode.W, KeyCode.Keypad8 },
        new[] { KeyCode.UpArrow, KeyCode.W, KeyCode.Keypad8 },
        new[] { KeyCode.DownArrow, KeyCode.S, KeyCode.Keypad2 },
        new[] { KeyCode.DownArrow, KeyCode.S, KeyCode.Keypad2 },
        new[] { KeyCode.LeftArrow, KeyCode.A, KeyCode.Keypad4 },
        new[] { KeyCode.RightArrow, KeyCode.D, KeyCode.Keypad6 },
        new[] { KeyCode.LeftArrow, KeyCode.A, KeyCode.Keypad4 },
        new[] { KeyCode.RightArrow, KeyCode.D, KeyCode.Keypad6 },
        new[] { KeyCode.B, KeyCode.Mouse1 },
        new[] { KeyCode.A, KeyCode.Mouse0 }
    };

    public static KcegListener instance;

    public int step;
    public float nextKeyTimer;
    public bool triggered;

    public KcegListener(IntPtr ptr) : base(ptr) { }

    public void Awake()
    {
        try
        {
            if (instance != null)
                DestroyImmediate(this);
            else
                instance = this;
        }
        catch (ObjectCollectedException)
        {
            instance = this;
        }
    }

    public void Start()
    {
        int status = PlayerPrefs.GetInt(PLAYER_PREFS_KEY, 0);

        switch (status)
        {
            case 1: // Did the easter egg, but didn't see straight wires
                PlayerPrefs.SetInt(PLAYER_PREFS_KEY, 0);

                break;

            case 2: // Did the easter egg and saw straight wires
                instance = null;
                DestroyImmediate(this);

                break;
        }
    }

    public void Update()
    {
        if (triggered) return;

        foreach (KeyCode keyCode in requiredInputs[step])
        {
            if (Input.GetKeyDown(keyCode))
            {
                step++;
                nextKeyTimer = 1 + Time.deltaTime;

                break;
            }
        }

        if (step == requiredInputs.Count)
        {
            PlayerPrefs.SetInt(PLAYER_PREFS_KEY, 1);
            triggered = true;
            SoundManager.Instance.PlaySound(SubmarineStatus.instance.minigameProperties.audioClips[5], false, 1.5f);

            return;
        }

        nextKeyTimer -= Time.deltaTime;

        if (nextKeyTimer <= 0)
        {
            nextKeyTimer = 0;
            step = 0;
        }
    }

    public void OnDestroy()
    {
        instance = null;
    }
}
