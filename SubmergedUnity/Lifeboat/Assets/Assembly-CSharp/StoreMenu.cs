using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PowerTools;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Security;

public class StoreMenu : DestroyableSingleton<StoreMenu>, IStoreListener
{
	public HatParent HatSlot;

	public SpriteRenderer SkinSlot;

	public SpriteAnim PetSlot;

	public TextMeshPro ItemName;

	public SpriteRenderer PurchaseBackground;

	public TextMeshPro PriceText;

	public PurchaseButton PurchasablePrefab;

	public SpriteRenderer HortLinePrefab;

	public TextMeshPro LoadingText;

	public TextMeshPro RestorePurchasesButton;

	public GameObject RestorePurchasesObj;

	public GameObject OpenAllInEShopButton;

	private UiElement eShopButton;

	public ShopBanner hatsBanner;

	public ShopBanner petsBanner;

	public ShopBanner skinsBanner;

	public ShopBanner holidayHatsBanner;

	public Sprite HolidayBanner;

	public SpriteRenderer TopArrow;

	public SpriteRenderer BottomArrow;

	public const string BoughtAdsProductId = "bought_ads";

	public TextMeshPro switchTaxText;

	private IStoreController controller;

	private IExtensionProvider extensions;

	public Scroller Scroller;

	public float ItemListStartY = 2f;

	public FloatRange XRange = new FloatRange(-1f, 1f);

	public int NumPerRow = 4;

	private PurchaseButton CurrentButton;

	private List<GameObject> AllObjects = new List<GameObject>();

	private ControllerNavMenu controllerNavMenu;

	private bool initialized;

	private const float NormalHeight = -0.45f;

	private const float BoxHeight = -0.75f;

	public PurchaseStates PurchaseState { get; private set; }

	public bool Initialized
	{
		get
		{
			return this.initialized;
		}
	}

	public void Start()
	{
		if (this.switchTaxText)
		{
			this.switchTaxText.gameObject.SetActive(false);
		}
		if (this.OpenAllInEShopButton)
		{
			this.OpenAllInEShopButton.SetActive(false);
		}
		this.Initialize();
	}

	public void Initialize()
	{
		if (this.initialized)
		{
			return;
		}
		this.initialized = true;
		this.controllerNavMenu = base.GetComponent<ControllerNavMenu>();
		base.gameObject.SetActive(false);
		this.PetSlot.gameObject.SetActive(false);
		SteamPurchasingModule steamPurchasingModule = new SteamPurchasingModule(this);
		foreach (PetBehaviour petBehaviour in DestroyableSingleton<HatManager>.Instance.AllPets)
		{
			if (petBehaviour.SteamId != 0U)
			{
				steamPurchasingModule.IdTranslator.Add(petBehaviour.ProdId, petBehaviour);
			}
		}
		foreach (MapBuyable mapBuyable in DestroyableSingleton<HatManager>.Instance.AllMaps)
		{
			if (mapBuyable.SteamId != 0U)
			{
				steamPurchasingModule.IdTranslator.Add(mapBuyable.ProdId, mapBuyable);
			}
		}
		ConfigurationBuilder configurationBuilder = ConfigurationBuilder.Instance(steamPurchasingModule, Array.Empty<IPurchasingModule>());
		foreach (PetBehaviour petBehaviour2 in DestroyableSingleton<HatManager>.Instance.AllPets)
		{
			if (!petBehaviour2.Free && !petBehaviour2.NotInStore && !string.IsNullOrEmpty(petBehaviour2.ProdId))
			{
				configurationBuilder.AddProduct(petBehaviour2.ProdId, (ProductType) 1);
			}
		}
		foreach (MapBuyable mapBuyable2 in DestroyableSingleton<HatManager>.Instance.AllMaps)
		{
			configurationBuilder.AddProduct(mapBuyable2.ProdId, (ProductType) 1);
		}
		UnityPurchasing.Initialize(this, configurationBuilder);
		this.PurchaseBackground.color = new Color(0.5f, 0.5f, 0.5f, 1f);
		this.PriceText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
		this.PriceText.text = "";
	}

	public void Update()
	{
		this.TopArrow.enabled = !this.Scroller.AtTop;
		this.BottomArrow.enabled = !this.Scroller.AtBottom;
		if (DestroyableSingleton<HudManager>.InstanceExists)
		{
			Vector3 position = DestroyableSingleton<HudManager>.Instance.transform.position;
			position.z -= 100f;
			base.transform.position = position;
			return;
		}
		base.transform.position = new Vector3(0f, 0f, -100f);
	}

	public void Open()
	{
		if (this.controller != null)
		{
			this.ShowAllButtons();
		}
		base.gameObject.SetActive(true);
		if (this.controllerNavMenu)
		{
			this.controllerNavMenu.OpenMenu(false);
		}
	}

	public void RestorePurchases()
	{
	}

	public void OpenGeneralShop()
	{
	}

	private void DestroySliderObjects()
	{
		for (int i = 0; i < this.AllObjects.Count; i++)
		{
			 UnityEngine.Object.Destroy(this.AllObjects[i]);
		}
		this.AllObjects.Clear();
		this.controllerNavMenu.DefaultButtonSelected = null;
		this.controllerNavMenu.ControllerSelectable.Clear();
	}

	private void FinishRestoring()
	{
		this.ShowAllButtons();
		this.RestorePurchasesButton.text = "Purchases Restored";
	}

	public void SetProduct(PurchaseButton button)
	{
		if (this.PurchaseState == PurchaseStates.Started)
		{
			return;
		}
		if (!button || button.Product == null)
		{
			return;
		}
		try
		{
			this.CurrentButton = button;
			if (this.CurrentButton.Product is HatBehaviour)
			{
				HatBehaviour hatBehaviour = (HatBehaviour)this.CurrentButton.Product;
				this.HatSlot.gameObject.SetActive(true);
				this.SkinSlot.gameObject.SetActive(false);
				this.PetSlot.gameObject.SetActive(false);
				this.HatSlot.SetHat(hatBehaviour, 0);
				StringNames stringNames;
				if (string.IsNullOrWhiteSpace(hatBehaviour.StoreName))
				{
					Enum.TryParse<StringNames>(hatBehaviour.name, out stringNames);
				}
				else
				{
					Enum.TryParse<StringNames>(hatBehaviour.StoreName, out stringNames);
				}
				this.ItemName.text = ((stringNames == StringNames.ExitButton) ? hatBehaviour.name.ToString() : DestroyableSingleton<TranslationController>.Instance.GetString(stringNames, Array.Empty<object>()));
				if (hatBehaviour.RelatedSkin)
				{
					TextMeshPro itemName = this.ItemName;
					itemName.text += " (Includes skin!)";
					this.SkinSlot.gameObject.SetActive(true);
					PlayerControl.SetSkinImage(hatBehaviour.RelatedSkin, this.SkinSlot);
				}
			}
			else if (this.CurrentButton.Product is SkinData)
			{
				SkinData skinData = (SkinData)this.CurrentButton.Product;
				this.SkinSlot.gameObject.SetActive(true);
				this.HatSlot.gameObject.SetActive(true);
				this.PetSlot.gameObject.SetActive(false);
				this.HatSlot.SetHat(skinData.RelatedHat, 0);
				PlayerControl.SetSkinImage(skinData, this.SkinSlot);
				StringNames stringNames2;
				if (string.IsNullOrWhiteSpace(skinData.StoreName))
				{
					Enum.TryParse<StringNames>(skinData.ProductId, out stringNames2);
				}
				else
				{
					Enum.TryParse<StringNames>(skinData.StoreName, out stringNames2);
				}
				this.ItemName.text = ((stringNames2 == StringNames.ExitButton) ? skinData.StoreName.ToString() : DestroyableSingleton<TranslationController>.Instance.GetString(stringNames2, Array.Empty<object>()));
			}
			else if (this.CurrentButton.Product is PetBehaviour)
			{
				PetBehaviour petBehaviour = (PetBehaviour)this.CurrentButton.Product;
				this.SkinSlot.gameObject.SetActive(false);
				HatBehaviour hatByProdId = DestroyableSingleton<HatManager>.Instance.GetHatByProdId(petBehaviour.ProdId);
				if (hatByProdId)
				{
					this.HatSlot.gameObject.SetActive(true);
					this.HatSlot.SetHat(hatByProdId, 0);
				}
				else
				{
					this.HatSlot.gameObject.SetActive(false);
				}
				this.PetSlot.gameObject.SetActive(true);
				SpriteRenderer component = this.PetSlot.GetComponent<SpriteRenderer>();
				component.material = new Material(petBehaviour.rend.sharedMaterial);
				PlayerControl.SetPlayerMaterialColors((int)SaveManager.BodyColor, component);
				this.PetSlot.Play(petBehaviour.idleClip, 1f);
				this.ItemName.text = DestroyableSingleton<TranslationController>.Instance.GetString(petBehaviour.StoreName, Array.Empty<object>());
			}
			else if (this.CurrentButton.Product is MapBuyable)
			{
				MapBuyable mapBuyable = (MapBuyable)this.CurrentButton.Product;
				this.SkinSlot.gameObject.SetActive(false);
				this.HatSlot.gameObject.SetActive(false);
				this.PetSlot.gameObject.SetActive(false);
				this.ItemName.text = DestroyableSingleton<TranslationController>.Instance.GetString(mapBuyable.StoreName, Array.Empty<object>());
			}
			else
			{
				this.HatSlot.gameObject.SetActive(false);
				this.SkinSlot.gameObject.SetActive(false);
				this.PetSlot.gameObject.SetActive(false);
				this.ItemName.text = "Remove All Ads";
			}
			if (button.Purchased)
			{
				this.PurchaseBackground.color = new Color(0.5f, 0.5f, 0.5f, 1f);
				this.PriceText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
				this.PriceText.text = "Owned";
			}
			else
			{
				this.PurchaseBackground.color = Color.white;
				this.PriceText.color = Color.white;
				this.PriceText.text = button.Price;
			}
		}
		catch (Exception ex)
		{
			Debug.Log("Could set product: " + button.ProductId);
			throw ex;
		}
	}

	public void BuyProduct()
	{
		if (!this.CurrentButton || this.CurrentButton.Purchased || this.PurchaseState == PurchaseStates.Started)
		{
			return;
		}
		base.StartCoroutine(this.WaitForPurchaseAds(this.CurrentButton));
	}

	public IEnumerator WaitForPurchaseAds(PurchaseButton button)
	{
		this.PurchaseState = PurchaseStates.Started;
		this.controller.InitiatePurchase(button.ProductId);
		while (this.PurchaseState == PurchaseStates.Started)
		{
			yield return null;
		}
		if (this.PurchaseState == PurchaseStates.Success)
		{
			foreach (PurchaseButton purchaseButton in from p in this.AllObjects
			select p.GetComponent<PurchaseButton>() into h
			where h && h.ProductId == button.ProductId
			select h)
			{
				purchaseButton.SetPurchased();
			}
		}
		this.SetProduct(button);
		yield break;
	}

	public void Close()
	{
		HatsTab hatsTab = UnityEngine.Object.FindObjectOfType<HatsTab>();
		if (hatsTab)
		{
			hatsTab.OnDisable();
			hatsTab.OnEnable();
		}
		base.gameObject.SetActive(false);
	}

	private void ShowAllButtons()
	{
		this.DestroySliderObjects();
		string text = "";
		try
		{
			text = "Couldn't fetch products";
			Product[] all = this.controller.products.all;
			text = "Couldn't validate products";
			for (int i = 0; i < all.Length; i++)
			{
				try
				{
					Product product = all[i];
					if (product != null && product.hasReceipt)
					{
						Debug.Log("Validating: " + product.definition.id);
						SaveManager.SetPurchased(product.definition.id);
					}
				}
				catch (InvalidSignatureException ex)
				{
					Debug.LogError("Invalid signature: " + ex.Message);
				}
			}
			text = "Couldn't place products";
			Vector3 vector = new Vector3(this.XRange.Lerp(0.5f), this.ItemListStartY);
			this.RestorePurchasesObj.gameObject.SetActive(false);
			if (this.OpenAllInEShopButton.activeSelf)
			{
				vector.y += -0.45f;
			}
			text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.PetFailFetchData, Array.Empty<object>());
			vector.y += -0.375f;
			List<MapBuyable> allMaps = DestroyableSingleton<HatManager>.Instance.AllMaps;
			vector = this.InsertMapsFromList(vector, all, allMaps);
			text = "Couldn't fetch pet data";
			vector.y += -0.375f;
			PetBehaviour[] array = (from h in DestroyableSingleton<HatManager>.Instance.AllPets
			where !h.Free && !h.NotInStore
			select h into p
			orderby p.StoreName
			select p).ToArray<PetBehaviour>();
			vector = this.InsertBanner(vector, this.petsBanner);
			Vector3 position = vector;
			Product[] allProducts = all;
			IBuyable[] hats = array;
			vector = this.InsertHatsFromList(position, allProducts, hats);
			text = "Couldn't finalize menu";
			this.Scroller.YBounds.max = Mathf.Max(0f, -vector.y - 2.5f);
			try
			{
				this.LoadingText.gameObject.SetActive(false);
			}
			catch
			{
			}
		}
		catch (Exception ex2)
		{
			string str = "Exception: ";
			string str2 = text;
			string str3 = ": ";
			Exception ex3 = ex2;
			Debug.Log(str + str2 + str3 + ((ex3 != null) ? ex3.ToString() : null));
			this.DestroySliderObjects();
			this.LoadingText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.LoadingFailed, Array.Empty<object>()) + "\r\n" + text;
			this.LoadingText.gameObject.SetActive(true);
		}
	}

	private Vector3 InsertHortLine(Vector3 position)
	{
		position.x = 1.2f;
		SpriteRenderer spriteRenderer = UnityEngine.Object.Instantiate<SpriteRenderer>(this.HortLinePrefab, this.Scroller.Inner);
		spriteRenderer.transform.localPosition = position;
		spriteRenderer.gameObject.SetActive(true);
		position.y += -0.33749998f;
		return position;
	}

	private Vector3 InsertMapsFromList(Vector3 position, Product[] allProducts, List<MapBuyable> maps)
	{
		position.y += -0.1875f;
		for (int i = maps.Count - 1; i >= 0; i--)
		{
			MapBuyable mapItem = maps[i];
			Product product = allProducts.FirstOrDefault((Product p) => mapItem.ProdId == p.definition.id);
			if (product != null && product.definition != null && product.availableToPurchase)
			{
				position.x = this.XRange.Lerp(0.5f);
				IBuyable[] bundleSkins = (from h in DestroyableSingleton<HatManager>.Instance.AllSkins
				where h.ProdId == mapItem.ProdId
				select h).Cast<IBuyable>().ToArray<IBuyable>();
				if (mapItem.IncludeHats)
				{
					bundleSkins = bundleSkins.Concat(from h in DestroyableSingleton<HatManager>.Instance.AllHats
					where h.ProdId == mapItem.ProdId && !bundleSkins.Contains(h.RelatedSkin)
					select h).ToArray<IBuyable>();
				}
				this.InsertProduct(position, product, mapItem);
				position.y += -1.05f;
				position = this.InsertHatsFromList(position, allProducts, bundleSkins);
				position.y += -0.1875f;
				if (i > 0)
				{
					position.y += -0.375f;
				}
			}
		}
		return position;
	}

	private Vector3 InsertHatsFromList(Vector3 position, Product[] allProducts, IBuyable[] hats)
	{
		int num = 0;
		for (int i = 0; i < hats.Length; i++)
		{
			IBuyable item = hats[i];
			Product product = allProducts.FirstOrDefault((Product p) => item.ProdId == p.definition.id);
			if (product == null || product.definition == null || !product.availableToPurchase)
			{
				if (product == null)
				{
					string str = "Couldn't add ";
					IBuyable item4 = item;
					Debug.LogWarning(str + ((item4 != null) ? item4.ToString() : null) + " due to product == null");
				}
				else if (product.definition == null)
				{
					string str2 = "Couldn't add ";
					IBuyable item2 = item;
					Debug.LogWarning(str2 + ((item2 != null) ? item2.ToString() : null) + " due to product.definition == null");
				}
				else if (!product.availableToPurchase)
				{
					string str3 = "Couldn't add ";
					IBuyable item3 = item;
					Debug.LogWarning(str3 + ((item3 != null) ? item3.ToString() : null) + " due to !product.availableToPurchase");
				}
			}
			else
			{
				int num2 = num % this.NumPerRow;
				position.x = this.XRange.Lerp((float)num2 / ((float)this.NumPerRow - 1f));
				if (num2 == 0 && num > 1)
				{
					position.y += -0.75f;
				}
				this.InsertProduct(position, product, item);
				num++;
			}
		}
		position.y += -0.75f;
		return position;
	}

	private PurchaseButton InsertProduct(Vector3 position, Product product, IBuyable item)
	{
		PurchaseButton purchaseButton = UnityEngine.Object.Instantiate<PurchaseButton>(this.PurchasablePrefab, this.Scroller.Inner);
		this.AllObjects.Add(purchaseButton.gameObject);
		purchaseButton.transform.localPosition = position;
		purchaseButton.Parent = this;
		PurchaseButton purchaseButton2 = purchaseButton;
		string id = product.definition.id;
		ProductMetadata metadata = product.metadata;
		string name;
		if (metadata == null)
		{
			name = null;
		}
		else
		{
			string localizedTitle = metadata.localizedTitle;
			name = ((localizedTitle != null) ? localizedTitle.Replace("(Among Us)", "") : null);
		}
		ProductMetadata metadata2 = product.metadata;
		purchaseButton2.SetItem(item, id, name, (metadata2 != null) ? metadata2.localizedPriceString : null, product.hasReceipt || SaveManager.GetPurchase(product.definition.id));
		UiElement component = purchaseButton.GetComponent<UiElement>();
		if (component)
		{
			if (!this.controllerNavMenu.DefaultButtonSelected)
			{
				this.controllerNavMenu.DefaultButtonSelected = component;
			}
			this.controllerNavMenu.ControllerSelectable.Add(component);
		}
		return purchaseButton;
	}

	private Vector3 InsertBanner(Vector3 position, ShopBanner prefab)
	{
		position.x = this.XRange.Lerp(0.5f);
		ShopBanner shopBanner = UnityEngine.Object.Instantiate<ShopBanner>(prefab, this.Scroller.Inner);
		shopBanner.transform.localPosition = position;
		position.y += -shopBanner.sRenderer.sprite.bounds.size.y;
		this.AllObjects.Add(shopBanner.gameObject);
		return position;
	}

	public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
	{
		this.controller = controller;
		this.extensions = extensions;
		if (this.controller == null || this.controller.products == null)
		{
			this.LoadingText.text = "Product controller\r\nfailed to load";
			return;
		}
		this.ShowAllButtons();
	}

	public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
	{
		Debug.Log("Completed Purchase: " + e.purchasedProduct.definition.id);
		this.PurchaseState = PurchaseStates.Success;
		SaveManager.SetPurchased(e.purchasedProduct.definition.id);
		return 0;
	}

	public void OnInitializeFailed(InitializationFailureReason error)
	{
		this.RestorePurchasesObj.SetActive(false);
		this.LoadingText.gameObject.SetActive(true);
		if (error == (InitializationFailureReason) 1)
		{
			this.LoadingText.text = "Coming Soon!";
			return;
		}
		if (error == null)
		{
			this.LoadingText.text = "Loading Failed:\r\nSteam must be running and logged in to view products.";
			return;
		}
		this.LoadingText.text = "Loading Failed:\r\n" + error.ToString();
	}

	public void OnPurchaseFailed(Product i, PurchaseFailureReason error)
	{
		if (error == (PurchaseFailureReason) 4)
		{
			this.PurchaseState = PurchaseStates.NotStarted;
			return;
		}
		if (error == (PurchaseFailureReason) 2)
		{
			this.DestroySliderObjects();
			this.LoadingText.gameObject.SetActive(true);
			this.LoadingText.text = "Coming Soon!";
		}
		else if (error == null)
		{
			this.DestroySliderObjects();
			this.LoadingText.gameObject.SetActive(true);
			this.LoadingText.text = "Steam overlay is required for in-game purchasing. You can still buy and install DLC in Steam.";
		}
		else
		{
			this.DestroySliderObjects();
			this.LoadingText.gameObject.SetActive(true);
			this.LoadingText.text = "Purchase Failed:\r\n" + error.ToString();
		}
		Debug.LogError("Failed: " + error.ToString());
		this.PurchaseState = PurchaseStates.Fail;
	}

	public void ResetPurchaseState()
	{
		this.PurchaseState = PurchaseStates.NotStarted;
	}
}
