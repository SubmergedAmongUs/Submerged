using Reactor.Utilities.Attributes;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.StartSubmersible.MonoBehaviours;

[RegisterInIl2Cpp]
public sealed class SubmersibleSlider(nint ptr) : MonoBehaviour(ptr)
{
    public bool complete;

    public SpriteRenderer targetLine;

    public GameObject sliderHandle;
    public Collider2D handleCollider;

    public Transform max;
    public Transform min;

    private readonly Controller _controller = new();
    private bool _active;
    private Vector2 _mouseOffset;

    private void Awake()
    {
        sliderHandle = transform.Find("SliderOutline/SliderDot").gameObject;
        handleCollider = sliderHandle.GetComponent<Collider2D>();

        max = transform.Find("SliderOutline/Max");
        min = transform.Find("SliderOutline/Min");

        targetLine = transform.Find("TargetLine").GetComponent<SpriteRenderer>();

        float lineRandom = UnityRandom.Range(0, 1f);
        float sliderRandom = UnityRandom.Range(0, 1f);

        if (Mathf.Abs(lineRandom - sliderRandom) < 0.05f) sliderRandom = (sliderRandom + 0.1f) % 1f;
        Vector3 position = min.transform.position;
        Vector3 position1 = max.transform.position;
        targetLine.transform.position = Vector3.Lerp(position, position1, lineRandom);

        sliderHandle.transform.position = Vector3.Lerp(position, position1, sliderRandom);
    }

    private void Update()
    {
        _controller.Update();

        Vector3 handlePosition = sliderHandle.transform.position;
        DragState dragState = _controller.CheckDrag(handleCollider);

        if (dragState is DragState.Dragging or DragState.Holding)
        {
            if (!_active)
            {
                _active = true;
                _mouseOffset = _controller.mainCam.ScreenToWorldPoint(Input.mousePosition) - sliderHandle.transform.position;
            }

            handlePosition = (Vector2) _controller.mainCam.ScreenToWorldPoint(Input.mousePosition) - _mouseOffset;
            _controller.ResetDragPosition();
        }
        else
        {
            _active = false;

            if (complete)
            {
                handleCollider.GetComponent<SpriteRenderer>().material.SetFloat("_Outline", 0);
                enabled = false;
            }
        }

        Vector3 position = max.transform.position;
        handlePosition.x = position.x;
        handlePosition.z = position.z;
        Vector3 position1 = min.transform.position;
        handlePosition.y = Mathf.Clamp(handlePosition.y, position1.y, position.y);
        sliderHandle.transform.position = handlePosition;

        float tolerance = (position.y - position1.y) * 0.001f;

        if (((Vector2) sliderHandle.transform.position - (Vector2) targetLine.transform.position).SqrMagnitude() < tolerance)
        {
            if (!_controller.amTouching) complete = true;
            targetLine.color = Color.green;
        }
        else
        {
            complete = false;
            targetLine.color = Color.white;
        }
    }
}
