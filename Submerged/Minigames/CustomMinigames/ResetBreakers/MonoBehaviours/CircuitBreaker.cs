﻿using Reactor.Utilities.Attributes;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.ResetBreakers.MonoBehaviours;

[RegisterInIl2Cpp]
public sealed class CircuitBreaker(nint ptr) : MonoBehaviour(ptr)
{
    public GameObject on;
    public GameObject off;
    public SpriteRenderer character;

    public AudioClip breakerClick;

    public char targetChar;

    public bool complete;

    public void Awake()
    {
        complete = UnityRandom.Range(0, 1f) > 0.5f;

        on = transform.Find("On").gameObject;
        off = transform.Find("Off").gameObject;
        character = transform.Find("Character").GetComponent<SpriteRenderer>();

        on.SetActive(complete);
        off.SetActive(!complete);
    }

    public void Update()
    {
        if (Input.GetKeyDown(targetChar >= 'A' && targetChar <= 'Z' ? KeyCode.A + (targetChar - 'A') : targetChar >= 'a' && targetChar <= 'z' ? KeyCode.A + (targetChar - 'a') : targetChar >= '0' && targetChar <= '9' ? KeyCode.Alpha0 + (targetChar - '0') : KeyCode.None))
        {
            SoundManager.Instance.PlaySound(breakerClick, false);
            SetState(!complete);
        }
    }

    public void SetState(bool state)
    {
        complete = state;
        on.SetActive(complete);
        off.SetActive(!complete);
    }
}
