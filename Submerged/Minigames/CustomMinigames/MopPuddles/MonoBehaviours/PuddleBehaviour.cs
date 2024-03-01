using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using Submerged.Minigames.MonoBehaviours;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.MopPuddles.MonoBehaviours;

[RegisterInIl2Cpp]
public sealed class PuddleBehaviour(nint ptr) : MonoBehaviour(ptr)
{
    public MopPuddlesMinigame minigame;
    public Draggable draggable;

    public float health = 2f;
    public bool wiping;
    private Collider2D _collider2D;
    private bool _finished;

    private Vector2 _previousPosition;

    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider2D = gameObject.AddComponent<PolygonCollider2D>();

        if (name.Contains('4')) transform.localScale = Vector3.one;
    }

    private void LateUpdate()
    {
        if (_finished || minigame.mopDirty) return;

        if (health <= 0)
        {
            _finished = true;
            this.StartCoroutine(FinishAnimation());

            return;
        }

        if (!draggable.dragging) return;
        // Vector2 mousePosition = Camera.main!.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dragPos = draggable.transform.position;

        bool mouseInCollider = _collider2D.OverlapPoint(new Vector2(dragPos.x, dragPos.y - 2f)); // Collider2D.OverlapPoint(mousePosition) ||
        if (!wiping) _previousPosition = dragPos;

        if (!mouseInCollider) return;
        wiping = true;

        float delta = (dragPos - _previousPosition).magnitude;
        _previousPosition = dragPos;
        if (name.Contains('3')) delta *= 3f;
        health -= delta;
        _spriteRenderer.color = new Color(1, 1, 1, Mathf.Clamp(health / 15 + 0.25f, 0, 1));
        // transform.localScale = Vector3.one * Mathf.Clamp(Health / 100 + 0.5f, 0, 1);
    }

    [HideFromIl2Cpp]
    private IEnumerator FinishAnimation()
    {
        minigame.SetMopDirty();
        const float DURATION = 0.3f;

        for (float t = 0; t < DURATION; t += Time.deltaTime)
        {
            float invAmount = (DURATION - t) / DURATION;
            _spriteRenderer.color = new Color(1, 1, 1, invAmount * 0.25f);

            yield return null;
        }

        _spriteRenderer.color = new Color(1, 1, 1, 0);
    }
}
