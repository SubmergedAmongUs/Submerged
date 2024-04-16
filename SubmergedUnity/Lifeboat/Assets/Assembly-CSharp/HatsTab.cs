using System;
using System.Collections.Generic;
using UnityEngine;

public class HatsTab : MonoBehaviour
{
	public ColorChip ColorTabPrefab;

	public SpriteRenderer DemoImage;

	public HatParent HatImage;

	public SpriteRenderer SkinImage;

	public SpriteRenderer PetImage;

	public FloatRange XRange = new FloatRange(1.5f, 3f);

	public float YStart = 1.1f;

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
		HatBehaviour[] unlockedHats = DestroyableSingleton<HatManager>.Instance.GetUnlockedHats();
		for (int i = 0; i < unlockedHats.Length; i++)
		{
			HatBehaviour hat = unlockedHats[i];
			float num = this.XRange.Lerp((float)(i % this.NumPerRow) / ((float)this.NumPerRow - 1f));
			float num2 = this.YStart - (float)(i / this.NumPerRow) * this.YOffset;
			ColorChip colorChip = UnityEngine.Object.Instantiate<ColorChip>(this.ColorTabPrefab, this.scroller.Inner);
			colorChip.transform.localPosition = new Vector3(num, num2, -1f);
			colorChip.Button.OnClick.AddListener(delegate()
			{
				this.SelectHat(hat);
			});
			colorChip.Inner.SetHat(hat, PlayerControl.LocalPlayer.Data.ColorId);
			colorChip.Inner.transform.localPosition = hat.ChipOffset + new Vector2(0f, -0.3f);
			colorChip.Tag = hat;
			this.ColorChips.Add(colorChip);
		}
		this.scroller.YBounds.max = -(this.YStart - (float)(unlockedHats.Length / this.NumPerRow) * this.YOffset) - 3f;
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
		HatBehaviour hatById = DestroyableSingleton<HatManager>.Instance.GetHatById(SaveManager.LastHat);
		for (int i = 0; i < this.ColorChips.Count; i++)
		{
			ColorChip colorChip = this.ColorChips[i];
			colorChip.InUseForeground.SetActive(hatById == colorChip.Tag);
		}
	}

	private void SelectHat(HatBehaviour hat)
	{
		uint idFromHat = DestroyableSingleton<HatManager>.Instance.GetIdFromHat(hat);
		SaveManager.LastHat = idFromHat;
		this.HatImage.SetHat(idFromHat, PlayerControl.LocalPlayer.Data.ColorId);
		if (PlayerControl.LocalPlayer)
		{
			PlayerControl.LocalPlayer.RpcSetHat(idFromHat);
		}
	}

	public ColorChip GetDefaultSelectable()
	{
		HatBehaviour hatById = DestroyableSingleton<HatManager>.Instance.GetHatById(SaveManager.LastHat);
		for (int i = 0; i < this.ColorChips.Count; i++)
		{
			ColorChip colorChip = this.ColorChips[i];
			if (hatById == colorChip.Tag)
			{
				return colorChip;
			}
		}
		return this.ColorChips[0];
	}
}
