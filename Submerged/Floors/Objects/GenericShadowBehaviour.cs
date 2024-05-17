using System;
using System.Collections.Generic;
using System.Linq;
using Il2CppInterop.Runtime;
using Reactor.Utilities.Attributes;
using Submerged.Extensions;
using Submerged.Map;
using UnityEngine;

namespace Submerged.Floors.Objects;

[RegisterInIl2Cpp]
public sealed class GenericShadowBehaviour(nint ptr) : MonoBehaviour(ptr)
{
    public static List<Func<Transform, Type>> ShadowTypeOverrides { get; set; } = [];

    static GenericShadowBehaviour()
    {
        // Overrides added earlier have higher priority (smaller index goes first)
        ShadowTypeOverrides.Add(obj => obj.gameObject.GetComponentInParent<LongBoiPlayerBody>(true) ? typeof(LongPlayerShadowRenderer) : null);
        ShadowTypeOverrides.Add(obj => obj.gameObject.GetComponentInParent<PlayerControl>() ? typeof(PlayerShadowRenderer) : null);
        ShadowTypeOverrides.Add(obj => obj.gameObject.GetComponent<DeadBody>() ? typeof(DeadBodyShadowRenderer) : null);
    }

    private void Start()
    {
        // Without this, the fake shadow objects on the "Water" layer (4) will be seen by the main camera.
        Camera.main!.cullingMask = 1073969927; // yay magic numbers

        GameObject shadowParent = new($"Submerged Shadow ({name})");
        CreateShadowsRecursively(transform, shadowParent.transform);

        shadowParent.transform.SetParent(transform);
    }

    private static void CreateShadowsRecursively(Transform currentObject, Transform currentShadow, bool isRoot = true)
    {
        Type targetRendererType = ShadowTypeOverrides.Select(func => func(currentObject)).FirstOrDefault(type => type != null, defaultValue: typeof(RelativeShadowRenderer));
        RelativeShadowRenderer rend = currentShadow.gameObject.AddComponent(Il2CppType.From(targetRendererType)).Cast<RelativeShadowRenderer>();

        rend.isRoot = isRoot;
        rend.target = currentObject;
        rend.replacementSprites = SubmarineStatus.instance.GetReplacementShadowSprites(currentObject.name);

        foreach (Transform child in currentObject.GetChildren())
        {
            GameObject shadowObj = new($"Submerged Shadow ({child.name})");
            shadowObj.transform.SetParent(currentShadow);
            CreateShadowsRecursively(child, shadowObj.transform, false);
        }
    }
}
