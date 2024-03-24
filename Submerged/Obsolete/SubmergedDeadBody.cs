using System;
using System.Linq;
using PowerTools;
using Reactor.Utilities.Attributes;
using UnityEngine;

// ReSharper disable all

namespace Submerged.Floors.Objects;

[RegisterInIl2Cpp]
[Obsolete("This component is no longer used in Submerged and might be removed in a future update. Please use GenericShadowBehaviour instead.")]
public sealed class SubmergedDeadBody(nint ptr) : MonoBehaviour(ptr)
{
    public DeadBody parent;
    public SpriteRenderer shadowRenderer;
    public SpriteAnim bodyAnim;

    private void Awake()
    {
        Warning("SubmergedDeadBody is no longer used in Submerged and might be removed in a future update. Please use GenericShadowBehaviour instead!");
        Warning("SubmergedDeadBody is no longer used in Submerged and might be removed in a future update. Please use GenericShadowBehaviour instead!");
        Warning("SubmergedDeadBody is no longer used in Submerged and might be removed in a future update. Please use GenericShadowBehaviour instead!");
        Warning("SubmergedDeadBody is no longer used in Submerged and might be removed in a future update. Please use GenericShadowBehaviour instead!");
        Warning("SubmergedDeadBody is no longer used in Submerged and might be removed in a future update. Please use GenericShadowBehaviour instead!");

        parent = GetComponent<DeadBody>();
        bodyAnim = parent.bodyRenderers.First().GetComponent<SpriteAnim>();
    }

    private void Start()
    {
        parent.bodyRenderers.First().gameObject.layer = LayerMask.NameToLayer("Players");

        shadowRenderer = new GameObject("Submerged Shadow") { layer = 4 }.AddComponent<SpriteRenderer>();
        Transform shadowRendererTransform = shadowRenderer.transform;
        shadowRendererTransform.parent = transform;
        shadowRendererTransform.localPosition = bodyAnim.transform.localPosition;
        shadowRendererTransform.localScale = new Vector3(0.5f, 0.5f, 1f);
    }

    private void Update()
    {
        if (bodyAnim.IsPlaying())
        {
            shadowRenderer.sprite = parent.bodyRenderers.First().sprite;
        }
    }
}
