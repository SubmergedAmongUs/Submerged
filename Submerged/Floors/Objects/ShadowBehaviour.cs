using Reactor.Utilities.Attributes;
using Submerged.Extensions;
using UnityEngine;

namespace Submerged.Floors.Objects;

[RegisterInIl2Cpp]
public class ShadowBehaviour(nint ptr) : MonoBehaviour(ptr)
{
    protected virtual void Start()
    {
        GameObject shadowParent = new($"Submerged Shadow ({name})");
        CreateShadowsRecursively(transform, shadowParent.transform);

        shadowParent.transform.SetParent(transform);
    }

    protected virtual void CreateShadowsRecursively(Transform currentObject, Transform currentShadow)
    {
        RelativeShadowRenderer rend = currentObject.gameObject.AddComponent<RelativeShadowRenderer>();
        rend.target = currentObject;

        foreach (Transform child in currentObject.GetChildren())
        {
            GameObject shadowObj = new($"Submerged Shadow ({child.name})");
            shadowObj.transform.SetParent(currentShadow);
            CreateShadowsRecursively(child, shadowObj.transform);
        }
    }
}
