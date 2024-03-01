using Reactor.Utilities.Attributes;
using UnityEngine;

namespace Submerged.ExileCutscene;

[RegisterInIl2Cpp]
public sealed class ExileParallax(nint ptr) : MonoBehaviour(ptr)
{
    private const float SCALE = 0.5f;
    private ParallaxChild[] _children;

    private Vector3 _initialCamPos;

    private void Awake()
    {
        _children = GetComponentsInChildren<ParallaxChild>();
        _initialCamPos = transform.parent.localPosition;
    }

    private void LateUpdate()
    {
        Vector3 delta = transform.parent.localPosition - _initialCamPos;
        delta *= -1f;

        foreach (ParallaxChild child in _children)
        {
            float deltaY = child.BasePosition.y + delta.y * SCALE * Mathf.Abs(child.BasePosition.z);
            child.transform.localPosition = child.BasePosition + new Vector3(0, deltaY, 0);
        }
    }
}
