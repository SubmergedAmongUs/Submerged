using HarmonyLib;
using UnityEngine;

namespace Submerged.Map.Patches;

[HarmonyPatch]
public static class DetectiveRolePatches
{
    [HarmonyPatch(typeof(DetectiveRole), nameof(DetectiveRole.CreateMapLocations))]
    [HarmonyPostfix]
    public static void CreateMapLocationsPatch(DetectiveRole __instance)
    {
        if (SubmarineStatus.instance && !DetectiveLocationsController.Instance)
        {
            GameObject.Instantiate(SubmarineStatus.instance.detectiveMapLocationsPrefab);
        }
    }
}
