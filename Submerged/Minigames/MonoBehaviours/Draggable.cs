using Reactor.Utilities.Attributes;
using UnityEngine;

namespace Submerged.Minigames.MonoBehaviours;

[RegisterInIl2Cpp]
public class Draggable(nint ptr) : ClickableSprite(ptr)
{
    public bool forceStop;

    public bool dragging;
    public Vector3 initialPosition;

    private Camera _mainCam;

    private Vector2 _offset;

    private void Awake()
    {
        _mainCam = Camera.main;
        initialPosition = transform.position;
    }

    private void Update()
    {
        if (dragging && !Input.GetMouseButton(0))
        {
            dragging = false;
            onUp?.Invoke();
        }
    }

    public override void OnMouseDown()
    {
        _offset = (Vector2) transform.position - (Vector2) _mainCam.ScreenToWorldPoint(Input.mousePosition);
        dragging = true;
        onDown?.Invoke();
    }

    public override void OnMouseDrag()
    {
        if (forceStop) return;
        if (!dragging) return;

        Vector2 newPos = (Vector2) _mainCam.ScreenToWorldPoint(Input.mousePosition) + _offset;
        transform.position = new Vector3(newPos.x, newPos.y, initialPosition.z);

        onDrag?.Invoke();
    }

    public override void OnMouseUp() { } // Not used for draggable update used instead
}
