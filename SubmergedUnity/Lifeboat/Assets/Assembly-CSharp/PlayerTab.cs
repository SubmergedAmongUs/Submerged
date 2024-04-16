using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTab : MonoBehaviour
{
	public ColorChip ColorTabPrefab;

	public SpriteRenderer DemoImage;

	public HatParent HatImage;

	public SpriteRenderer SkinImage;

	public SpriteRenderer PetImage;

	public Transform ColorTabArea;

	public FloatRange XRange = new FloatRange(1.5f, 3f);

	public float YStart = 1.65f;

	private HashSet<int> AvailableColors = new HashSet<int>();

	[HideInInspector]
	public List<ColorChip> ColorChips = new List<ColorChip>();

	private const int Columns = 4;

	public void OnEnable()
	{
		PlayerControl.SetPlayerMaterialColors(PlayerControl.LocalPlayer.Data.ColorId, this.DemoImage);
		this.HatImage.SetHat(SaveManager.LastHat, PlayerControl.LocalPlayer.Data.ColorId);
		PlayerControl.SetSkinImage(SaveManager.LastSkin, this.SkinImage);
		PlayerControl.SetPetImage(SaveManager.LastPet, PlayerControl.LocalPlayer.Data.ColorId, this.PetImage);
		float num = (float)Palette.PlayerColors.Length / 4f;
		float num2 = 0.55f;
		for (int i = 0; i < Palette.PlayerColors.Length; i++)
		{
			float num3 = this.XRange.Lerp((float)(i % 4) / 3f);
			float num4 = this.YStart - (float)(i / 4) * num2;
			ColorChip colorChip = UnityEngine.Object.Instantiate<ColorChip>(this.ColorTabPrefab);
			colorChip.transform.SetParent(this.ColorTabArea);
			colorChip.transform.localPosition = new Vector3(num3, num4, -1f);
			int j = i;
			colorChip.Button.OnClick.AddListener(delegate()
			{
				this.SelectColor(j);
			});
			colorChip.Inner.color = Palette.PlayerColors[i];
			this.ColorChips.Add(colorChip);
		}
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
		this.UpdateAvailableColors();
		for (int i = 0; i < this.ColorChips.Count; i++)
		{
			this.ColorChips[i].InUseForeground.SetActive(!this.AvailableColors.Contains(i));
		}
	}

	private void SelectColor(int colorId)
	{
		this.UpdateAvailableColors();
		if (this.AvailableColors.Remove(colorId))
		{
			SaveManager.BodyColor = (byte)colorId;
			if (PlayerControl.LocalPlayer)
			{
				PlayerControl.LocalPlayer.CmdCheckColor((byte)colorId);
			}
		}
	}

	public void UpdateAvailableColors()
	{
		PlayerControl.SetPlayerMaterialColors(PlayerControl.LocalPlayer.Data.ColorId, this.DemoImage);
		PlayerControl.SetPetImage(SaveManager.LastPet, PlayerControl.LocalPlayer.Data.ColorId, this.PetImage);
		for (int i = 0; i < Palette.PlayerColors.Length; i++)
		{
			this.AvailableColors.Add(i);
		}
		if (GameData.Instance)
		{
			List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
			for (int j = 0; j < allPlayers.Count; j++)
			{
				GameData.PlayerInfo playerInfo = allPlayers[j];
				this.AvailableColors.Remove(playerInfo.ColorId);
			}
		}
	}

	public ColorChip GetDefaultSelectable()
	{
		return this.ColorChips[PlayerControl.LocalPlayer.Data.ColorId];
	}
}
