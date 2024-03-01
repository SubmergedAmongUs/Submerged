using HarmonyLib;
using Submerged.Map;

namespace Submerged.ExileCutscene.Patches;

[HarmonyPatch]
public static class BaseGameResolvingPatches
{
    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
    [HarmonyPrefix]
    public static void ResolvePlayerPatch(ExileController __instance)
    {
        SubmergedExileController submergedExile = __instance.GetComponent<SubmergedExileController>();
        if (!submergedExile) return;

        submergedExile.exileHatPosition = MapLoader.Airship.ExileCutscenePrefab.exileHatPosition;
        submergedExile.exileVisorPosition = MapLoader.Airship.ExileCutscenePrefab.exileVisorPosition;
        submergedExile.Player = UnityObject.Instantiate(MapLoader.Airship.ExileCutscenePrefab.Player, submergedExile.transform, false);
        submergedExile.Player.transform.Find("HandSlot").gameObject.SetActive(false);
    }
}
