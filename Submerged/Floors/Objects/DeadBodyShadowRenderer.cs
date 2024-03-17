using Reactor.Utilities.Attributes;
using UnityEngine;

namespace Submerged.Floors.Objects;

[RegisterInIl2Cpp]
public class DeadBodyShadowRenderer(nint ptr) : RelativeShadowRenderer(ptr)
{
    protected override void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Players");
    }
}
