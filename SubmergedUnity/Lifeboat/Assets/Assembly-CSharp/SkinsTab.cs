using System;
using System.Collections.Generic;
using UnityEngine;

public class SkinsTab : MonoBehaviour
{
	public ColorChip ColorTabPrefab;

	public SpriteRenderer DemoImage;

	public HatParent HatImage;

	public SpriteRenderer SkinImage;

	public SpriteRenderer PetImage;

	public FloatRange XRange = new FloatRange(1.5f, 3f);

	public float YStart = 0.8f;

	public float YOffset = 0.8f;

	public int NumPerRow = 4;

	public Scroller scroller;

	[HideInInspector]
	public List<ColorChip> ColorChips = new List<ColorChip>();

	public void OnEnable()
	{
		PlayerControl.SetPlayerMaterialColors(PlayerControl.LocalPlayer.Data.ColorId, this.DemoImage);
		this.HatImage.SetHat(SaveManager.LastHat, PlayerControl.LocalPlayer.Data.ColorId);
		PlayerControl.SetSkinImage(SaveManager.LastSkin, this.SkinImage);
		PlayerControl.SetPetImage(SaveManager.LastPet, PlayerControl.LocalPlayer.Data.ColorId, this.PetImage);
		SkinData[] unlockedSkins = DestroyableSingleton<HatManager>.Instance.GetUnlockedSkins();
		for (int i = 0; i < unlockedSkins.Length; i++)
		{
			SkinData skin = unlockedSkins[i];
			float num = this.XRange.Lerp((float)(i % this.NumPerRow) / ((float)this.NumPerRow - 1f));
			float num2 = this.YStart - (float)(i / this.NumPerRow) * this.YOffset;
			ColorChip colorChip = UnityEngine.Object.Instantiate<ColorChip>(this.ColorTabPrefab, this.scroller.Inner);
			colorChip.transform.localPosition = new Vector3(num, num2, -1f);
			colorChip.Button.OnClick.AddListener(delegate()
			{
				this.SelectHat(skin);
			});
			colorChip.Inner.FrontLayer.sprite = skin.IdleFrame;
			this.ColorChips.Add(colorChip);
		}
		this.scroller.YBounds.max = -(this.YStart - (float)(unlockedSkins.Length / this.NumPerRow) * this.YOffset) - 3f;
	}

	public void OnDisable()
	{
		for (int i = 0; i < this.ColorChips.Count; i++)
		{
			 UnityEngine.Object.Destroy(this.ColorChips[i].gameObject);
		}
		this.ColorChips.Clear();
	}

	public void Update()
	{
		PlayerControl.SetPlayerMaterialColors(PlayerControl.LocalPlayer.Data.ColorId, this.DemoImage);
		SkinData skinById = DestroyableSingleton<HatManager>.Instance.GetSkinById(SaveManager.LastSkin);
		for (int i = 0; i < this.ColorChips.Count; i++)
		{
			ColorChip colorChip = this.ColorChips[i];
			colorChip.InUseForeground.SetActive(skinById.IdleFrame == colorChip.Inner.FrontLayer.sprite);
		}
	}

	private void SelectHat(SkinData skin)
	{
		uint idFromSkin = DestroyableSingleton<HatManager>.Instance.GetIdFromSkin(skin);
		SaveManager.LastSkin = idFromSkin;
		PlayerControl.SetSkinImage(SaveManager.LastSkin, this.SkinImage);
		if (PlayerControl.LocalPlayer)
		{
			PlayerControl.LocalPlayer.RpcSetSkin(idFromSkin);
		}
	}

	public ColorChip GetDefaultSelectable()
	{
		SkinData skinById = DestroyableSingleton<HatManager>.Instance.GetSkinById(SaveManager.LastSkin);
		for (int i = 0; i < this.ColorChips.Count; i++)
		{
			ColorChip colorChip = this.ColorChips[i];
			if (skinById.IdleFrame == colorChip.Inner.FrontLayer.sprite)
			{
				return colorChip;
			}
		}
		return this.ColorChips[0];
	}
}
