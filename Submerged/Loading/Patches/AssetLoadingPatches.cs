using System.Linq;
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using Submerged.Map;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using ResourceManager = UnityEngine.ResourceManagement.ResourceManager;

namespace Submerged.Loading.Patches;

[HarmonyPatch]
public static class AssetLoadingPatches
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Awake))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPostfix]
    public static void AddMapToAmongUsClientPatch(AmongUsClient __instance)
    {
        __instance.StartCoroutine(MapLoader.LoadMaps());

        if (__instance.ShipPrefabs.Count < 7)
        {
            AssetReference assetReference = new("Submerged");
            AmongUsClient.Instance.ShipPrefabs.Add(assetReference);
            AmongUsClient.Instance.SpawnableObjects = AmongUsClient.Instance.SpawnableObjects.AddItem(assetReference).ToArray();
        }

        if (!Constants.MapNames.Contains("Submerged")) Constants.MapNames = Constants.MapNames.AddItem("Submerged").ToArray();
    }

    [HarmonyPatch(typeof(AssetReference), nameof(AssetReference.InstantiateAsync), typeof(Transform), typeof(bool))]
    [HarmonyPrefix]
    public static bool InstantiateMapPrefabPatch(AssetReference __instance, ref AsyncOperationHandle<GameObject> __result)
    {
        if (__instance.m_AssetGUID != "Submerged") return true;

        ResourceManager.CompletedOperation<GameObject> operation = new()
        {
            HasExecuted = true,
            Result = UnityObject.Instantiate(AssetLoader.Submerged),
            m_Version = 123,
            m_Success = true,
            m_ReleaseDependenciesOnFailure = false,
            m_Status = AsyncOperationStatus.Succeeded
        };

        __result = new AsyncOperationHandle<GameObject>
        {
            m_InternalOp = operation,
            m_Version = operation.m_Version,
            m_LocationName = null,
            m_UnloadSceneOpExcludeReleaseCallback = false
        };

        return false;
    }
}
