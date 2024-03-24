using HarmonyLib;
using Submerged.Map;

namespace Submerged.ExileCutscene.Patches;

[HarmonyPatch]
public static class BaseGameResolvingPatches
{
    public static string LastExileText { get; private set; }

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
    [HarmonyPostfix, HarmonyPriority(Priority.Last)]
    public static void GetTextPatch(ExileController __instance)
    {
        LastExileText = __instance.completeString;
        Message("Grabbing " + __instance + " from ExileController");
    }

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
