using Submerged.Loading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Submerged.Map;

// TODO: Cache what we need and unload the rest
public static class MapLoader
{
    private class Out<T>
    {
        public T Value { get; set; }
    }

    public static ShipStatus Skeld { get; private set; }
    public static AirshipStatus Airship { get; private set; }
    public static FungleShipStatus Fungle { get; private set; }

    public static IEnumerator LoadMaps()
    {
        while (AmongUsClient.Instance == null) yield return null;

        if (!Skeld)
        {
            Out<ShipStatus> o = new();
            yield return LoadMap(MapNames.Skeld, o);
            Skeld = o.Value;
        }

        if (!Airship)
        {
            Out<AirshipStatus> o = new();
            yield return LoadMap(MapNames.Airship, o);
            Airship = o.Value;
        }

        if (!Fungle)
        {
            Out<FungleShipStatus> o = new();
            yield return LoadMap(MapNames.Fungle, o);
            Fungle = o.Value;
        }

        LoadingManager.DoneLoading(nameof(MapLoader));
    }

    private static IEnumerator LoadMap<T>(MapNames map, Out<T> shipStatus) where T : ShipStatus
    {
        AssetReference reference = AmongUsClient.Instance.ShipPrefabs._items[(int) map];

        AsyncOperationHandle<GameObject> handle;

        if (reference.OperationHandle.IsValid())
        {
            handle = reference.OperationHandle.Convert<GameObject>();

            if (!handle.IsDone)
                yield return handle;
        }
        else
        {
            handle = reference.LoadAsset<GameObject>();
            yield return handle;
        }

        if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
        {
            shipStatus.Value = handle.Result.GetComponent<T>();
        }
        else
        {
            UnityEngine.Debug.LogError("Failed to load map asset");
        }
    }
}
