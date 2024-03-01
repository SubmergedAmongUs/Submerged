using System.Linq;
using PowerTools;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace Submerged.Floors.Objects;

[RegisterInIl2Cpp]
public sealed class SubmergedDeadBody(nint ptr) : MonoBehaviour(ptr)
{
    public DeadBody parent;
    public SpriteRenderer shadowRenderer;
    public SpriteAnim bodyAnim;

    private void Awake()
    {
        parent = GetComponent<DeadBody>();
        bodyAnim = parent.bodyRenderers.First().GetComponent<SpriteAnim>();
    }

    private void Start()
    {
        parent.bodyRenderers.First().gameObject.layer = 8;

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
