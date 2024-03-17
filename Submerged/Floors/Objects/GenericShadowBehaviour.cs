using Reactor.Utilities.Attributes;
using Submerged.Extensions;
using Submerged.Map;
using UnityEngine;

namespace Submerged.Floors.Objects;

[RegisterInIl2Cpp]
public class GenericShadowBehaviour(nint ptr) : MonoBehaviour(ptr)
{
    protected virtual void Start()
    {
        // Without this, the fake shadow objects on the "Water" layer (4) will be seen by the main camera.
        Camera.main!.cullingMask = 1073969927; // yay magic numbers

        GameObject shadowParent = new($"Submerged Shadow ({name})");
        CreateShadowsRecursively(transform, shadowParent.transform);

        shadowParent.transform.SetParent(transform);
    }

    protected virtual void CreateShadowsRecursively(Transform currentObject, Transform currentShadow)
    {
        LongBoiPlayerBody longBoi = currentObject.gameObject.GetComponentInParent<LongBoiPlayerBody>();

        RelativeShadowRenderer rend = !longBoi
            ? currentShadow.gameObject.AddComponent<RelativeShadowRenderer>()
            : currentShadow.gameObject.AddComponent<LongPlayerRelativeShadowRenderer>();

        rend.target = currentObject;
        rend.replacementSprites = SubmarineStatus.instance.GetReplacementShadowSprites(currentObject.name);

        foreach (Transform child in currentObject.GetChildren())
        {
            GameObject shadowObj = new($"Submerged Shadow ({child.name})");
            shadowObj.transform.SetParent(currentShadow);
            CreateShadowsRecursively(child, shadowObj.transform);
        }
    }
}
