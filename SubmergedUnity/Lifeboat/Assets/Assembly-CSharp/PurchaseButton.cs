using System;
using TMPro;
using UnityEngine;

public class PurchaseButton : MonoBehaviour
{
	private const float BorderSize = 0.7f;

	private const float BorderSelectHighlightOffset = 0.05f;

	public SpriteRenderer PurchasedIcon;

	public TextMeshPro NameText;

	public SpriteRenderer CrewHeadImage;

	public HatParent HatImage;

	public Sprite MannequinFrame;

	public SpriteRenderer Background;

	public SpriteRenderer SelectionHighlight;

	public IBuyable Product;

	public bool Purchased;

	public string Name;

	public string Price;

	public string ProductId;

	public StoreMenu Parent { get; set; }

	public void SetItem(IBuyable product, string productId, string name, string price, bool purchased)
	{
		this.Product = product;
		this.Purchased = purchased;
		this.Name = name;
		this.Price = price;
		this.ProductId = productId;
		base.name = productId;
		this.PurchasedIcon.enabled = this.Purchased;
		if (this.Product is HatBehaviour)
		{
			HatBehaviour hat = (HatBehaviour)this.Product;
			this.NameText.gameObject.SetActive(false);
			this.HatImage.SetHat(hat, 0);
			this.HatImage.FrontLayer.transform.localPosition = new Vector3(0f, 0f, -0.01f);
			this.HatImage.FrontLayer.transform.localScale = Vector3.one * 0.5f;
			this.HatImage.BackLayer.transform.localPosition = new Vector3(0f, 0f, 0.01f);
			this.HatImage.BackLayer.transform.localScale = Vector3.one * 0.5f;
			this.SetSquare();
			return;
		}
		if (this.Product is SkinData)
		{
			SkinData skin = (SkinData)this.Product;
			this.NameText.gameObject.SetActive(false);
			this.CrewHeadImage.sprite = this.MannequinFrame;
			this.CrewHeadImage.transform.localPosition = new Vector3(0f, 0f, -0.01f);
			this.CrewHeadImage.transform.localScale = Vector3.one * 0.3f;
			this.HatImage.FrontLayer.transform.localPosition = new Vector3(0f, 0f, -0.01f);
			this.HatImage.FrontLayer.transform.localScale = Vector3.one;
			PlayerControl.SetSkinImage(skin, this.HatImage.FrontLayer);
			this.SetSquare();
			return;
		}
		if (this.Product is PetBehaviour)
		{
			PetBehaviour petBehaviour = (PetBehaviour)this.Product;
			this.NameText.gameObject.SetActive(false);
			this.CrewHeadImage.enabled = false;
			this.HatImage.FrontLayer.material = new Material(petBehaviour.rend.sharedMaterial);
			this.HatImage.FrontLayer.transform.localPosition = new Vector3(0f, 0f, -0.01f);
			this.HatImage.FrontLayer.transform.localScale = Vector3.one * 0.5f;
			PlayerControl.SetPetImage(petBehaviour, (int)SaveManager.BodyColor, this.HatImage.FrontLayer);
			this.SetSquare();
			return;
		}
		if (this.Product is MapBuyable)
		{
			MapBuyable mapBuyable = (MapBuyable)this.Product;
			this.NameText.text = "";
			this.NameText.alignment = (TextAlignmentOptions.Left) ;
			this.NameText.transform.localPosition = new Vector3(-1.4f, -1.5f, -0.01f);
			this.NameText.color = Color.black;
			this.NameText.outlineColor = Color.clear;
			this.HatImage.FrontLayer.sprite = mapBuyable.StoreImage;
			this.CrewHeadImage.enabled = false;
			this.SetBig();
			this.Background.enabled = false;
			return;
		}
		this.NameText.text = this.Name;
		this.HatImage.gameObject.SetActive(false);
	}

	private void SetBig()
	{
		this.Background.size = new Vector2(2.8f, 1.4f);
		this.Background.GetComponent<BoxCollider2D>().size = new Vector2(2.8f, 1.4f);
		this.SelectionHighlight.size = new Vector2(2.85f, 1.4499999f);
		this.PurchasedIcon.transform.localPosition = new Vector3(1.1f, -0.45f, -2f);
	}

	private void SetSquare()
	{
		this.Background.size = new Vector2(0.7f, 0.7f);
		this.Background.GetComponent<BoxCollider2D>().size = new Vector2(0.7f, 0.7f);
		this.SelectionHighlight.size = new Vector2(0.75f, 0.75f);
		this.PurchasedIcon.transform.localPosition = new Vector3(0f, 0f, -1f);
	}

	internal void SetPurchased()
	{
		this.Purchased = true;
		this.PurchasedIcon.enabled = true;
	}

	internal void SetNonPurchased()
	{
		this.Purchased = false;
		this.PurchasedIcon.enabled = false;
	}

	public void DoPurchase()
	{
		this.Parent.SetProduct(this);
	}
}
