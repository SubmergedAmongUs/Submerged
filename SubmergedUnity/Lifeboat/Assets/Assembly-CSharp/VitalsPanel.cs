using System;
using UnityEngine;

public class VitalsPanel : MonoBehaviour
{
	public PoolablePlayer PlayerIcon;

	public SpriteRenderer MaskingArea;

	public SpriteRenderer Background;

	public VertLineBehaviour Cardio;

	public Sprite VitalBgDead;

	public Sprite VitalBgDiscon;

	public IntRange BeatRange;

	public bool IsDead;

	public bool IsDiscon;

	public GameData.PlayerInfo PlayerInfo { get; set; }

	public void SetPlayer(int index, GameData.PlayerInfo playerInfo)
	{
		this.PlayerInfo = playerInfo;
		this.MaskingArea.material.SetInt("_MaskLayer", index + 2);
		this.PlayerIcon.Body.material.SetInt("_MaskLayer", index + 2);
		this.PlayerIcon.Skin.layer.material.SetInt("_MaskLayer", index + 2);
		this.PlayerIcon.HatSlot.SetMaskLayer(index + 2);
		this.PlayerIcon.SetFlipX(false);
		PlayerControl.SetPlayerMaterialColors(playerInfo.ColorId, this.PlayerIcon.Body);
		this.PlayerIcon.HatSlot.SetHat(playerInfo.HatId, playerInfo.ColorId);
		this.PlayerIcon.SetSkin(playerInfo.SkinId);
	}

	public void SetDisconnected()
	{
		this.IsDead = false;
		this.IsDiscon = true;
		this.Background.sprite = this.VitalBgDiscon;
		this.Cardio.gameObject.SetActive(false);
	}

	public void SetDead()
	{
		this.IsDiscon = false;
		this.IsDead = true;
		this.Background.sprite = this.VitalBgDead;
		this.Cardio.SetDead();
	}

	public void SetAlive()
	{
		this.Cardio.Offset = IntRange.Next(0, 64);
		this.Cardio.beats = this.BeatRange.Next();
		this.Cardio.SetAlive();
	}
}
