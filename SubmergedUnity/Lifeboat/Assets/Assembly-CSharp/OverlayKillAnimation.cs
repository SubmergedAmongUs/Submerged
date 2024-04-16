using System;
using System.Collections;
using PowerTools;
using UnityEngine;

public class OverlayKillAnimation : OverlayAnimation
{
	public KillAnimType KillType;

	public PoolablePlayer killerParts;

	public PoolablePlayer victimParts;

	private uint victimHat;

	public AudioClip Stinger;

	public AudioClip Sfx;

	public float StingerVolume = 0.6f;

	public void Initialize(GameData.PlayerInfo kInfo, GameData.PlayerInfo vInfo)
	{
		if (this.killerParts)
		{
			PlayerControl.SetPlayerMaterialColors(kInfo.ColorId, this.killerParts.Body);
			this.killerParts.Hands.ForEach(delegate(SpriteRenderer b)
			{
				PlayerControl.SetPlayerMaterialColors(kInfo.ColorId, b);
			});
			if (this.killerParts.HatSlot)
			{
				this.killerParts.HatSlot.SetHat(kInfo.HatId, kInfo.ColorId);
			}
			switch (this.KillType)
			{
			case KillAnimType.Stab:
			{
				SkinData skinById = DestroyableSingleton<HatManager>.Instance.GetSkinById(kInfo.SkinId);
				this.killerParts.Skin.GetComponent<SpriteAnim>().Play(skinById.KillStabImpostor, 1f);
				break;
			}
			case KillAnimType.Tongue:
			{
				SkinData skinById2 = DestroyableSingleton<HatManager>.Instance.GetSkinById(kInfo.SkinId);
				this.killerParts.Skin.GetComponent<SpriteAnim>().Play(skinById2.KillTongueImpostor, 1f);
				break;
			}
			case KillAnimType.Shoot:
			{
				SkinData skinById3 = DestroyableSingleton<HatManager>.Instance.GetSkinById(kInfo.SkinId);
				this.killerParts.Skin.GetComponent<SpriteAnim>().Play(skinById3.KillShootImpostor, 1f);
				break;
			}
			case KillAnimType.Neck:
			{
				SkinData skinById4 = DestroyableSingleton<HatManager>.Instance.GetSkinById(kInfo.SkinId);
				this.killerParts.Skin.GetComponent<SpriteAnim>().Play(skinById4.KillNeckImpostor, 1f);
				break;
			}
			}
			if (this.killerParts.PetSlot)
			{
				PetBehaviour petById = DestroyableSingleton<HatManager>.Instance.GetPetById(kInfo.PetId);
				if (petById && petById.scaredClip)
				{
					this.killerParts.PetSlot.GetComponent<SpriteAnim>().Play(petById.idleClip, 1f);
					this.killerParts.PetSlot.sharedMaterial = petById.rend.sharedMaterial;
					PlayerControl.SetPlayerMaterialColors(kInfo.ColorId, this.killerParts.PetSlot);
				}
				else
				{
					this.killerParts.PetSlot.enabled = false;
				}
			}
		}
		if (vInfo != null && this.victimParts)
		{
			this.victimHat = vInfo.HatId;
			PlayerControl.SetPlayerMaterialColors(vInfo.ColorId, this.victimParts.Body);
			if (this.victimParts.HatSlot)
			{
				this.victimParts.HatSlot.SetHat(vInfo.HatId, vInfo.ColorId);
			}
			SkinData skinById5 = DestroyableSingleton<HatManager>.Instance.GetSkinById(vInfo.SkinId);
			switch (this.KillType)
			{
			case KillAnimType.Stab:
				this.victimParts.Skin.GetComponent<SpriteAnim>().Play(skinById5.KillStabVictim, 1f);
				break;
			case KillAnimType.Tongue:
				this.victimParts.Skin.GetComponent<SpriteAnim>().Play(skinById5.KillTongueVictim, 1f);
				break;
			case KillAnimType.Shoot:
				this.victimParts.Skin.GetComponent<SpriteAnim>().Play(skinById5.KillShootVictim, 1f);
				break;
			case KillAnimType.Neck:
				this.victimParts.Skin.GetComponent<SpriteAnim>().Play(skinById5.KillNeckVictim, 1f);
				break;
			case KillAnimType.RHM:
				this.victimParts.Skin.GetComponent<SpriteAnim>().Play(skinById5.KillRHMVictim, 1f);
				break;
			}
			if (this.victimParts.PetSlot)
			{
				PetBehaviour petById2 = DestroyableSingleton<HatManager>.Instance.GetPetById(vInfo.PetId);
				if (petById2 && petById2.scaredClip)
				{
					this.victimParts.PetSlot.GetComponent<SpriteAnim>().Play(petById2.scaredClip, 1f);
					this.victimParts.PetSlot.sharedMaterial = petById2.rend.sharedMaterial;
					PlayerControl.SetPlayerMaterialColors(vInfo.ColorId, this.victimParts.PetSlot);
					return;
				}
				this.victimParts.PetSlot.enabled = false;
			}
		}
	}

	public void SetHatFloor()
	{
		HatBehaviour hatById = DestroyableSingleton<HatManager>.Instance.GetHatById(this.victimHat);
		if (!hatById)
		{
			return;
		}
		this.victimParts.HatSlot.Hat = hatById;
		this.victimParts.HatSlot.SetFloorAnim();
	}

	public void PlayKillSound()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.Sfx, false, 1f).volume = 0.8f;
			VibrationManager.Vibrate(3f, 3f, 0f, VibrationManager.VibrationFalloff.None, this.Sfx, false);
			DualshockLightManager.Flash(Color.red, 2f, this.Sfx);
		}
	}

	public override IEnumerator CoShow(KillOverlay parent)
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.Stinger, false, 1f).volume = this.StingerVolume;
		}
		parent.background.enabled = true;
		yield return Effects.Wait(0.083333336f);
		parent.background.enabled = false;
		parent.flameParent.SetActive(true);
		parent.flameParent.transform.localScale = new Vector3(1f, 0.3f, 1f);
		parent.flameParent.transform.localEulerAngles = new Vector3(0f, 0f, 25f);
		yield return Effects.Wait(0.083333336f);
		parent.flameParent.transform.localScale = new Vector3(1f, 0.5f, 1f);
		parent.flameParent.transform.localEulerAngles = new Vector3(0f, 0f, -15f);
		yield return Effects.Wait(0.083333336f);
		parent.flameParent.transform.localScale = new Vector3(1f, 1f, 1f);
		parent.flameParent.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		base.gameObject.SetActive(true);
		yield return this.WaitForFinish();
		base.gameObject.SetActive(false);
		yield return new WaitForLerp(0.16666667f, delegate(float t)
		{
			parent.flameParent.transform.localScale = new Vector3(1f, 1f - t, 1f);
		});
		parent.flameParent.SetActive(false);
		yield break;
	}

	private IEnumerator WaitForFinish()
	{
		SpriteAnim[] anims = base.GetComponentsInChildren<SpriteAnim>();
		if (anims.Length == 0)
		{
			yield return new WaitForSeconds(1f);
		}
		else
		{
			for (;;)
			{
				bool flag = false;
				for (int i = 0; i < anims.Length; i++)
				{
					if (anims[i].IsPlaying())
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					break;
				}
				yield return null;
			}
		}
		yield break;
	}
}
