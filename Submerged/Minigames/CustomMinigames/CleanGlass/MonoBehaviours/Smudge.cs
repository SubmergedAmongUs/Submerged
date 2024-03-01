using System;
using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using Submerged.Extensions;
using Submerged.Minigames.MonoBehaviours;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.CleanGlass.MonoBehaviours;

[RegisterInIl2Cpp]
public sealed class Smudge(nint ptr) : MonoBehaviour(ptr)
{
    public Draggable draggable;

    public float health = 50;
    public bool wiping;

    public AudioClip[] squeakSounds;
    private Collider2D _collider2D;
    private bool _finished;

    private Vector2 _previousPosition;

    private SpriteRenderer _spriteRenderer;

    private float _squeakThreshold = 10;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider2D = GetComponent<Collider2D>();
    }

    private void LateUpdate()
    {
        if (_finished) return;

        if (health <= 0)
        {
            _finished = true;
            this.StartCoroutine(FinishAnimation());

            return;
        }

        if (!draggable.dragging) return;
        Vector2 mousePosition = Camera.main!.ScreenToWorldPoint(Input.mousePosition);
        bool mouseInCollider = _collider2D.OverlapPoint(mousePosition);
        if (!wiping) _previousPosition = mousePosition;

        if (!mouseInCollider) return;
        wiping = true;

        float delta = (mousePosition - _previousPosition).SqrMagnitude() * 9f;
        _previousPosition = mousePosition;
        health -= delta;
        _squeakThreshold -= delta;

        if (_squeakThreshold <= 0)
        {
            _squeakThreshold = 45;
            SoundManager.Instance.PlaySound(squeakSounds.Random(), false, 0.33f);
        }

        _spriteRenderer.color = new Color(1, 1, 1, Mathf.Clamp(health / 100f + 0.3f, 0, 1));
    }

    [HideFromIl2Cpp]
    private IEnumerator FinishAnimation()
    {
        const float DURATION = 0.3f;

        yield return new WaitForLerp(DURATION, (Action<float>) (f => { _spriteRenderer.color = new Color(1, 1, 1, (DURATION - f) / DURATION * 0.3f); }));
    }
}
