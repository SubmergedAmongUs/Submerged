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
using Submerged.UI;
using UnityEngine;

namespace Submerged.Loading;

[RegisterInIl2Cpp]
public sealed class AssetLoader(nint ptr) : MonoBehaviour(ptr)
{
    private class Result<T>
    {
        private T _value;
        private Exception _error;

        public void SetValue(T value)
        {
            _value = value;
            _error = null;
        }

        public void SetError(Exception error)
        {
            _value = default;
            _error = error;
        }

        public static implicit operator T(Result<T> result)
        {
            if (result._error != null) throw result._error;
            return result._value;
        }
    }

    private static AssetLoader _instance;

    private bool _errored;

    public static GameObject Submerged { get; private set; }
    public static GameObject Credits { get; private set; }

    public static bool Errored => _instance._errored;

    private void Awake()
    {
        if (_instance) return;
        _instance = this;

        this.StartCoroutine(Load());
    }

    [HideFromIl2Cpp]
    private void ShowDialogFromException(Exception e)
    {
        _errored = true;
        Fatal(e);
        this.StartCoroutine(ShowError());
    }

    [HideFromIl2Cpp]
    private IEnumerator Load()
    {
        while (!AmongUsClient.Instance) yield return null;

        AssetBundleCreateRequest req;
        try
        {
            req = AssetBundle.LoadFromMemoryAsync(ResourceManager.GetEmbeddedBytes("submerged"));
            if (req == null) throw new NullReferenceException();
        }
        catch (Exception e)
        {
            ShowDialogFromException(e);
            yield break;
        }

        while (!req.WasCollected && !req.isDone) yield return null;

        AssetBundle bundle = req.assetBundle;

        Result<GameObject> submerged = new();
        yield return LoadAsset(bundle, "Submerged", submerged);

        try
        {
            Submerged = submerged;

            List<InnerNetObject> nonAddrList = AmongUsClient.Instance.NonAddressableSpawnableObjects.ToList();
            nonAddrList.Add(Submerged.GetComponent<ShipStatus>());
            AmongUsClient.Instance.NonAddressableSpawnableObjects = nonAddrList.ToArray();
        }
        catch (Exception e)
        {
            ShowDialogFromException(e);
            yield break;
        }

        Result<GameObject> credits = new();
        yield return LoadAsset(bundle, "CreditsScreen", credits);

        try
        {
            Credits = credits;
            Credits.SetActive(false);
            Credits.AddComponent<CreditsScreenManager>();
        }
        catch (Exception e)
        {
            ShowDialogFromException(e);
            yield break;
        }

        LoadingManager.DoneLoading(nameof(AssetLoader));
    }

    [HideFromIl2Cpp]
    private static IEnumerator LoadAsset<T>(AssetBundle bundle, string objectName, Result<T> result) where T : UnityObject
    {
        AssetBundleRequest bundleReq;

        try
        {
            bundleReq = bundle.LoadAssetAsync<T>(objectName);
            if (bundleReq == null) throw new NullReferenceException();
        }
        catch (Exception e)
        {
            result.SetError(e);
            yield break;
        }

        while (!bundleReq.WasCollected && !bundleReq.isDone) yield return null;

        try
        {
            result.SetValue(bundleReq.asset.TryCast<T>()!.DontDestroy().DontUnload());
        }
        catch (Exception e)
        {
            result.SetError(e);
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
