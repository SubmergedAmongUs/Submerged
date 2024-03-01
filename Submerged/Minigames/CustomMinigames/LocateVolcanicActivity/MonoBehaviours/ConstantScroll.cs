using Reactor.Utilities.Attributes;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.LocateVolcanicActivity.MonoBehaviours;

[RegisterInIl2Cpp]
public sealed class ConstantScroll(nint ptr) : MonoBehaviour(ptr)
{
    public Transform top;
    public Transform bottom;
    public float speed = -0.2f;

    private float _offset;

    private void Start()
    {
        top = transform.Find("TopLines");
        bottom = transform.Find("BottomLines");

        _offset = top.localPosition.y - bottom.localPosition.y;
    }

    private void FixedUpdate()
    {
        if (bottom.localPosition.y > _offset) bottom.localPosition -= new Vector3(0, 2 * _offset, 0);
        if (bottom.localPosition.y < -_offset) bottom.localPosition += new Vector3(0, 2 * _offset, 0);

        bottom.localPosition += new Vector3(0, 0.1f * speed, 0);

        if (top.localPosition.y > _offset) top.localPosition -= new Vector3(0, 2 * _offset, 0);
        if (top.localPosition.y < -_offset) top.localPosition += new Vector3(0, 2 * _offset, 0);

        top.localPosition += new Vector3(0, 0.1f * speed, 0);
    }
}
