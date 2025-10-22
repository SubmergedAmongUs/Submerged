using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Submerged.Extensions;
using Submerged.Map;
using UnityEngine;
using ShipStatus_PrespawnStep = ShipStatus._PrespawnStep_d__94;

namespace Submerged.SpawnIn.Patches;

[HarmonyPatch]
public static class DisplayPrespawnStepPatches
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.PrespawnStep))]
    [HarmonyPostfix]
    public static void Postfix(ShipStatus __instance, ref CppIEnumerator __result)
    {
        if (!__instance.IsSubmerged()) return;
        __result = CustomPrespawnStep().WrapToIl2Cpp();
    }

    private static int _lastShip = int.MinValue;

    private static IEnumerator CustomPrespawnStep()
    {
        if (GameManager.Instance.IsHideAndSeek() && _lastShip == ShipStatus.Instance.GetInstanceID()) yield break;
        _lastShip = ShipStatus.Instance.GetInstanceID();

        GameObject spawnInObject = UnityObject.Instantiate(SubmarineStatus.instance.minigameProperties.gameObjects[0]);
        SubmarineSelectSpawn spawnInMinigame = GameManager.Instance.IsNormal()
            ? spawnInObject.AddComponent<SubmarineSelectSpawn>()
            : spawnInObject.AddComponent<SubmarineSelectSpawnHnS>();
        spawnInMinigame.transform.SetParent(Camera.main!.transform, false);
        spawnInMinigame.transform.localPosition = new Vector3(0f, 0f, -600f); // -610 z pos
        spawnInMinigame.Begin(null);
        yield return spawnInMinigame.WaitForFinish();
    }
}
