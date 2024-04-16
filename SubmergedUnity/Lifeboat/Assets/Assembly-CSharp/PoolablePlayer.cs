using System;
using TMPro;
using UnityEngine;

public class PoolablePlayer : MonoBehaviour
{
	public SpriteRenderer Body;

	public SpriteRenderer[] Hands;

	public HatParent HatSlot;

	public SkinLayer Skin;

	public SpriteRenderer PetSlot;

	public TextMeshPro NameText;

	public void SetSkin(uint skinId)
	{
		this.Skin.SetSkin(skinId, this.Body.flipX);
		this.Skin.Flipped = this.Body.flipX;
	}

	public void SetFlipX(bool flipped)
	{
		this.Body.flipX = flipped;
		this.HatSlot.flipX = flipped;
		Vector3 localPosition = this.HatSlot.transform.localPosition;
		localPosition.x = (flipped ? Mathf.Abs(localPosition.x) : (-Mathf.Abs(localPosition.x)));
		this.HatSlot.transform.localPosition = localPosition;
		if (this.PetSlot)
		{
			this.PetSlot.flipX = flipped;
			Vector3 localPosition2 = this.PetSlot.transform.localPosition;
			localPosition2.x = (flipped ? (-Mathf.Abs(localPosition2.x)) : Mathf.Abs(localPosition2.x));
			this.PetSlot.transform.localPosition = localPosition2;
		}
	}

	public void SetDeadFlipX(bool flipped)
	{
		this.Body.flipX = flipped;
		this.PetSlot.flipX = flipped;
		this.HatSlot.flipX = flipped;
		Vector3 localPosition = this.HatSlot.transform.localPosition;
		localPosition.x = (flipped ? -0.2f : 0.2f);
		localPosition.y = 0.75f;
		this.HatSlot.transform.localPosition = localPosition;
		if (this.PetSlot)
		{
			this.PetSlot.flipX = flipped;
			Vector3 localPosition2 = this.PetSlot.transform.localPosition;
			localPosition2.x = (flipped ? (-Mathf.Abs(localPosition2.x)) : Mathf.Abs(localPosition2.x));
			this.PetSlot.transform.localPosition = localPosition2;
		}
	}

	internal void UpdateFromSaveManager()
	{
		PlayerControl.SetPlayerMaterialColors((int)SaveManager.BodyColor, this.Body);
		this.SetSkin(SaveManager.LastSkin);
		PlayerControl.SetPetImage(SaveManager.LastPet, (int)SaveManager.BodyColor, this.PetSlot);
		this.HatSlot.SetHat(SaveManager.LastHat, (int)SaveManager.BodyColor);
	}
}
