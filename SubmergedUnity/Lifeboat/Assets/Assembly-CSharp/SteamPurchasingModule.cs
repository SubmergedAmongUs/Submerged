using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Steamworks;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using Random = UnityEngine.Random;

internal class SteamPurchasingModule : IPurchasingModule, IStore
{
	public const string Name = "Steam";

	public Dictionary<string, ISteamBuyable> IdTranslator = new Dictionary<string, ISteamBuyable>(StringComparer.OrdinalIgnoreCase);

	private IStoreCallback storeCallback;

	private bool steamOverlayOpen;

	private StoreMenu parent;

	protected Callback<GameOverlayActivated_t> m_GameOverlayActivated;

	public SteamPurchasingModule(StoreMenu parent)
	{
		this.parent = parent;
	}

	public void Configure(IPurchasingBinder binder)
	{
		binder.RegisterStore("Steam", this);
	}

	public void FinishTransaction(ProductDefinition product, string transactionId)
	{
	}

	public void Initialize(IStoreCallback callback)
	{
		this.storeCallback = callback;
		if (!SteamManager.Initialized || !SteamUtils.IsOverlayEnabled())
		{
			this.storeCallback.OnSetupFailed(0);
		}
		this.m_GameOverlayActivated = Callback<GameOverlayActivated_t>.Create(new Callback<GameOverlayActivated_t>.DispatchDelegate(this.HandleOverlayActivate));
	}

	private void HandleOverlayActivate(GameOverlayActivated_t param)
	{
		this.steamOverlayOpen = (param.m_bActive > 0);
	}

	public void Purchase(ProductDefinition product, string developerPayload)
	{
		if (!SteamUtils.IsOverlayEnabled())
		{
			this.storeCallback.OnPurchaseFailed(new PurchaseFailureDescription(product.storeSpecificId, 0, "Steam overlay is disabled, but required for in-game purchasing."));
			return;
		}
		ISteamBuyable steamBuyable;
		if (this.IdTranslator.TryGetValue(product.id, out steamBuyable))
		{
			AppId_t appId_t = new AppId_t(steamBuyable.SteamAppId);
			SteamFriends.ActivateGameOverlayToStore(appId_t, EOverlayToStoreFlag.k_EOverlayToStoreFlag_AddToCartAndShow);
			this.parent.StartCoroutine(this.WaitForDlcPurchase(product, appId_t));
			return;
		}
		this.storeCallback.OnPurchaseFailed(new PurchaseFailureDescription(product.storeSpecificId, PurchaseFailureReason.ProductUnavailable, "Couldn't find Product Id for " + product.id));
	}

	private IEnumerator WaitForDlcPurchase(ProductDefinition product, AppId_t appId)
	{
		while (!this.steamOverlayOpen)
		{
			SteamAPI.RunCallbacks();
			yield return null;
		}
		while (this.steamOverlayOpen)
		{
			SteamAPI.RunCallbacks();
			yield return null;
		}
		ulong num;
		while (SteamApps.GetDlcDownloadProgress(appId, out num, out num))
		{
			yield return null;
		}
		if (SteamApps.BIsDlcInstalled(appId))
		{
			this.storeCallback.OnPurchaseSucceeded(product.id, "FakeReceipt", Random.value.ToString());
		}
		else
		{
			this.storeCallback.OnPurchaseFailed(new PurchaseFailureDescription(product.id, PurchaseFailureReason.UserCancelled, "Steam overlay closed without purchase completing"));
		}
		yield break;
	}

	public void RetrieveProducts(ReadOnlyCollection<ProductDefinition> products)
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		List<ProductDescription> list = new List<ProductDescription>(products.Count);
		for (int i = 0; i < products.Count; i++)
		{
			ProductDefinition productDefinition = products[i];
			ISteamBuyable steamBuyable;
			if (this.IdTranslator.TryGetValue(productDefinition.id, out steamBuyable))
			{
				bool flag = SteamApps.BIsDlcInstalled(new AppId_t(steamBuyable.SteamAppId));
				list.Add(new ProductDescription(productDefinition.id, new ProductMetadata(steamBuyable.SteamPrice, null, null, "USD", 1m), flag ? "Bought" : null, null));
			}
		}
		this.storeCallback.OnProductsRetrieved(list);
	}
}
