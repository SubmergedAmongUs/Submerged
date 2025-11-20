using System;
using HarmonyLib;
using Submerged.BaseGame;
using Submerged.Elevators.Objects;
using Submerged.Extensions;
using UnityEngine;

namespace Submerged.Minigames.Patches;

[BaseGameCode(LastChecked.v17_0_0, "UpdatePosition is inlined in IL2CPP, we must check to make sure it is still inlined across versions.")]
[HarmonyPatch]
public static class ArrowBehaviourUpdatePositionPatch
{
    [HarmonyPatch(typeof(ArrowBehaviour), nameof(ArrowBehaviour.Awake))]
    [HarmonyPrefix]
    public static void AwakePatch(ArrowBehaviour __instance)
    {
        if (ShipStatus.Instance.IsSubmerged())
        {
            __instance.gameObject.AddComponent<MultifloorArrowBehaviour>();
        }
    }

    [Obsolete("Use MultifloorArrowBehaviour.GetElevatorPosition instead. This method will be removed in a future release.")]
    public static Vector3 GetElevatorPosition(ArrowBehaviour __instance) => MultifloorArrowBehaviour.GetElevatorPosition(__instance.target);
}
