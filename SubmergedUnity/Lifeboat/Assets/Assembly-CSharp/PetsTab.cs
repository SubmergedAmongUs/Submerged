using System;
using System.Collections.Generic;
using UnityEngine;

public class PetsTab : MonoBehaviour
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
		PetBehaviour[] unlockedPets = DestroyableSingleton<HatManager>.Instance.GetUnlockedPets();
		for (int i = 0; i < unlockedPets.Length; i++)
		{
			PetBehaviour pet = unlockedPets[i];
			float num = this.XRange.Lerp((float)(i % this.NumPerRow) / ((float)this.NumPerRow - 1f));
			float num2 = this.YStart - (float)(i / this.NumPerRow) * this.YOffset;
			ColorChip chip = UnityEngine.Object.Instantiate<ColorChip>(this.ColorTabPrefab, this.scroller.Inner);
			chip.transform.localPosition = new Vector3(num, num2, -1f);
			chip.InUseForeground.SetActive(DestroyableSingleton<HatManager>.Instance.GetIdFromPet(pet) == SaveManager.LastPet);
			chip.Button.OnClick.AddListener(delegate()
			{
				this.SelectPet(chip, pet);
			});
			PlayerControl.SetPetImage(pet, PlayerControl.LocalPlayer.Data.ColorId, chip.Inner.FrontLayer);
			this.ColorChips.Add(chip);
		}
		this.scroller.YBounds.max = -(this.YStart - (float)(unlockedPets.Length / this.NumPerRow) * this.YOffset) - 3f;
	}

	public void OnDisable()
	{
		for (int i = 0; i < this.ColorChips.Count; i++)
		{
			 UnityEngine.Object.Destroy(this.ColorChips[i].gameObject);
		}
		this.ColorChips.Clear();
	}

	private void SelectPet(ColorChip sender, PetBehaviour pet)
	{
		uint idFromPet = DestroyableSingleton<HatManager>.Instance.GetIdFromPet(pet);
		SaveManager.LastPet = idFromPet;
		PlayerControl.SetPetImage(pet, PlayerControl.LocalPlayer.Data.ColorId, this.PetImage);
		if (PlayerControl.LocalPlayer)
		{
			PlayerControl.LocalPlayer.RpcSetPet(idFromPet);
		}
		for (int i = 0; i < this.ColorChips.Count; i++)
		{
			ColorChip colorChip = this.ColorChips[i];
			colorChip.InUseForeground.SetActive(colorChip == sender);
		}
	}

	public ColorChip GetDefaultSelectable()
	{
		PetBehaviour[] unlockedPets = DestroyableSingleton<HatManager>.Instance.GetUnlockedPets();
		DestroyableSingleton<HatManager>.Instance.GetHatById(SaveManager.LastHat);
		for (int i = 0; i < this.ColorChips.Count; i++)
		{
			PetBehaviour pet = unlockedPets[i];
			ColorChip result = this.ColorChips[i];
			if (DestroyableSingleton<HatManager>.Instance.GetIdFromPet(pet) == SaveManager.LastPet)
			{
				return result;
			}
		}
		return this.ColorChips[0];
	}
}
