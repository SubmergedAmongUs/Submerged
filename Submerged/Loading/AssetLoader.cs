using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using InnerNet;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using Submerged.Resources;
using Submerged.Localization.Strings;
using UnityEngine;

namespace Submerged.Loading;

[RegisterInIl2Cpp]
public sealed class AssetLoader(nint ptr) : MonoBehaviour(ptr)
{
    private static AssetLoader _instance;

    private bool _errored;
    private GameObject _submerged;

    public static GameObject Submerged => _instance._submerged;

    public static bool Errored => _instance._errored;

    private void Awake()
    {
        if (_instance) return;
        _instance = this;

        this.StartCoroutine(Load());
    }

    [HideFromIl2Cpp]
    private void Error(Exception e)
    {
        _errored = true;
        Fatal(e);
        this.StartCoroutine(ShowError());
    }

    [HideFromIl2Cpp]
    private IEnumerator Load()
    {
        while (!AmongUsClient.Instance)
        {
            yield return null;
        }

        AssetBundleCreateRequest req;

        try
        {
            req = AssetBundle.LoadFromMemoryAsync(ResourceManager.GetEmbeddedBytes("submerged"));

            if (req == null) throw new NullReferenceException();
        }
        catch (Exception e)
        {
            Error(e);

            yield break;
        }

        while (!req.WasCollected && !req.isDone) yield return null;

        AssetBundleRequest bundleReq;

        try
        {
            AssetBundle bundle = req.assetBundle;
            bundleReq = bundle.LoadAssetAsync<GameObject>("Submerged.prefab");

            if (bundleReq == null) throw new NullReferenceException();
        }
        catch (Exception e)
        {
            Error(e);

            yield break;
        }

        while (!bundleReq.WasCollected && !bundleReq.isDone) yield return null;

        try
        {
            _submerged = bundleReq.asset.TryCast<GameObject>()!.DontDestroy().DontUnload();

            List<InnerNetObject> nonAddrList = AmongUsClient.Instance.NonAddressableSpawnableObjects.ToList();
            nonAddrList.Add(Submerged.GetComponent<ShipStatus>());
            AmongUsClient.Instance.NonAddressableSpawnableObjects = nonAddrList.ToArray();

            LoadingManager.DoneLoading(nameof(AssetLoader));
        }
        catch (Exception e)
        {
            Error(e);
        }
    }

    [HideFromIl2Cpp]
    private static IEnumerator ShowError()
    {
        while (!DiscordManager.InstanceExists || !DiscordManager.Instance.discordPopup) yield return null;
        while (!FindObjectOfType<MainMenuManager>()) yield return null;

        GenericPopup popup = Instantiate(DiscordManager.Instance.discordPopup, null, true);
        SpriteRenderer background = popup.transform.Find("Background").GetComponent<SpriteRenderer>();
        background.size *= new Vector2(2.5f, 1f);
        popup.TextAreaTMP.fontSizeMin = 2;
        popup.Show(General.Error_AssetsNotLoaded);

        LoadingManager.DoneLoading(nameof(AssetLoader));
    }
}
