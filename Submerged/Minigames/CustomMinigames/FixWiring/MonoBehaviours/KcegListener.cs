using System.Collections.Generic;
using Il2CppInterop.Runtime;
using Reactor.Utilities.Attributes;
using Submerged.Map;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.FixWiring.MonoBehaviours;

[RegisterInIl2Cpp]
public sealed class KcegListener(nint ptr) : MonoBehaviour(ptr)
{
    public const string PLAYER_PREFS_KEY = "HasDoneKCEG";

    private static readonly List<KeyCode[]> _requiredInputs =
    [
        [KeyCode.UpArrow, KeyCode.W, KeyCode.Keypad8],
        [KeyCode.UpArrow, KeyCode.W, KeyCode.Keypad8],
        [KeyCode.DownArrow, KeyCode.S, KeyCode.Keypad2],
        [KeyCode.DownArrow, KeyCode.S, KeyCode.Keypad2],
        [KeyCode.LeftArrow, KeyCode.A, KeyCode.Keypad4],
        [KeyCode.RightArrow, KeyCode.D, KeyCode.Keypad6],
        [KeyCode.LeftArrow, KeyCode.A, KeyCode.Keypad4],
        [KeyCode.RightArrow, KeyCode.D, KeyCode.Keypad6],
        [KeyCode.B, KeyCode.Mouse1],
        [KeyCode.A, KeyCode.Mouse0]
    ];

    public static KcegListener Instance { get; private set; }

    public int step;
    public float nextKeyTimer;
    public bool triggered;

    public void Awake()
    {
        try
        {
            if (Instance != null)
                DestroyImmediate(this);
            else
                Instance = this;
        }
        catch (ObjectCollectedException)
        {
            Instance = this;
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
                Instance = null;
                DestroyImmediate(this);

                break;
        }
    }

    public void Update()
    {
        if (triggered) return;

        foreach (KeyCode keyCode in _requiredInputs[step])
        {
            if (Input.GetKeyDown(keyCode))
            {
                step++;
                nextKeyTimer = 1 + Time.deltaTime;

                break;
            }
        }

        if (step == _requiredInputs.Count)
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
        Instance = null;
    }
}
