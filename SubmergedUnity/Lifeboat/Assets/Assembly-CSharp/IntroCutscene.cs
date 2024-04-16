using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IntroCutscene : MonoBehaviour
{
	public static IntroCutscene Instance;

	public TextMeshPro Title;

	public TextMeshPro ImpostorText;

	public PoolablePlayer PlayerPrefab;

	public MeshRenderer BackgroundBar;

	public MeshRenderer Foreground;

	public FloatRange ForegroundRadius;

	public SpriteRenderer FrontMost;

	public AudioClip IntroStinger;

	private const float BaseY = -1f;

	private const float ScaleAll = 1f;

	private const float OffsetWidth = 0.9f;

	private const float OffsetHeight = 0.15f;

	private DualshockLightManager.LightOverlayHandle overlayHandle;

	public IEnumerator CoBegin(List<PlayerControl> yourTeam, bool isImpostor)
	{
		SoundManager.Instance.PlaySound(this.IntroStinger, false, 1f);
		if (this.overlayHandle == null)
		{
			this.overlayHandle = DestroyableSingleton<DualshockLightManager>.Instance.AllocateLight();
		}
		if (!isImpostor)
		{
			this.BeginCrewmate(yourTeam);
			this.overlayHandle.color = Palette.CrewmateBlue;
		}
		else
		{
			this.BeginImpostor(yourTeam);
			this.overlayHandle.color = Palette.ImpostorRed;
		}
		Color c = this.Title.color;
		Color fade = Color.black;
		Color impColor = Color.white;
		Vector3 titlePos = this.Title.transform.localPosition;
		float timer = 0f;
		while (timer < 3f)
		{
			timer += Time.deltaTime;
			float num = Mathf.Min(1f, timer / 3f);
			this.Foreground.material.SetFloat("_Rad", this.ForegroundRadius.ExpOutLerp(num * 2f));
			fade.a = Mathf.Lerp(1f, 0f, num * 3f);
			this.FrontMost.color = fade;
			c.a = Mathf.Clamp(FloatRange.ExpOutLerp(num, 0f, 1f), 0f, 1f);
			this.Title.color = c;
			impColor.a = Mathf.Lerp(0f, 1f, (num - 0.3f) * 3f);
			this.ImpostorText.color = impColor;
			titlePos.y = 2.7f - num * 0.3f;
			this.Title.transform.localPosition = titlePos;
			this.overlayHandle.color.a = Mathf.Min(1f, timer * 2f);
			yield return null;
		}
		timer = 0f;
		while (timer < 1f)
		{
			timer += Time.deltaTime;
			float num2 = timer / 1f;
			fade.a = Mathf.Lerp(0f, 1f, num2 * 3f);
			this.FrontMost.color = fade;
			this.overlayHandle.color.a = 1f - fade.a;
			yield return null;
		}
		 UnityEngine.Object.Destroy(base.gameObject);
		yield break;
	}

	private void BeginCrewmate(List<PlayerControl> yourTeam)
	{
		Vector3 position = this.BackgroundBar.transform.position;
		position.y -= 0.25f;
		this.BackgroundBar.transform.position = position;
		int adjustedNumImpostors = PlayerControl.GameOptions.GetAdjustedNumImpostors(GameData.Instance.PlayerCount);
		if (adjustedNumImpostors == 1)
		{
			this.ImpostorText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NumImpostorsS, Array.Empty<object>());
		}
		else
		{
			this.ImpostorText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NumImpostorsP, new object[]
			{
				adjustedNumImpostors
			});
		}
		this.ImpostorText.text = this.ImpostorText.text.Replace("[FF1919FF]", "<color=#FF1919FF>");
		this.ImpostorText.text = this.ImpostorText.text.Replace("[]", "</color>");
		this.BackgroundBar.material.SetColor("_Color", Palette.CrewmateBlue);
		this.Title.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Crewmate, Array.Empty<object>());
		this.Title.color = Palette.CrewmateBlue;
		int num = Mathf.CeilToInt(7.5f);
		for (int i = 0; i < yourTeam.Count; i++)
		{
			PlayerControl playerControl = yourTeam[i];
			if (playerControl)
			{
				GameData.PlayerInfo data = playerControl.Data;
				if (data != null)
				{
					int num2 = (i % 2 == 0) ? -1 : 1;
					int num3 = (i + 1) / 2;
					float num4 = (float)num3 / (float)num;
					float num5 = Mathf.Lerp(1f, 0.75f, num4);
					float num6 = (float)((i == 0) ? -8 : -1);
					PoolablePlayer poolablePlayer = UnityEngine.Object.Instantiate<PoolablePlayer>(this.PlayerPrefab, base.transform);
					poolablePlayer.name = data.PlayerName + "Dummy";
					poolablePlayer.SetFlipX(i % 2 == 0);
					poolablePlayer.transform.localPosition = new Vector3(0.9f * (float)num2 * (float)num3 * num5, FloatRange.SpreadToEdges(-1.125f, 0f, num3, num), num6 + (float)num3 * 0.01f) * 1f;
					float num7 = Mathf.Lerp((i == 0) ? 1.2f : 1f, 0.65f, num4) * 1f;
					poolablePlayer.transform.localScale = new Vector3(num7, num7, 1f);
					PlayerControl.SetPlayerMaterialColors(data.ColorId, poolablePlayer.Body);
					poolablePlayer.SetSkin(data.SkinId);
					poolablePlayer.HatSlot.SetHat(data.HatId, data.ColorId);
					PlayerControl.SetPetImage(data.PetId, data.ColorId, poolablePlayer.PetSlot);
					poolablePlayer.NameText.gameObject.SetActive(false);
				}
			}
		}
	}

	private void BeginImpostor(List<PlayerControl> yourTeam)
	{
		this.ImpostorText.gameObject.SetActive(false);
		this.Title.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Impostor, Array.Empty<object>());
		this.Title.color = Palette.ImpostorRed;
		for (int i = 0; i < yourTeam.Count; i++)
		{
			PlayerControl playerControl = yourTeam[i];
			if (playerControl)
			{
				GameData.PlayerInfo data = playerControl.Data;
				if (data != null)
				{
					int num = (i % 2 == 0) ? -1 : 1;
					int num2 = (i + 1) / 2;
					float num3 = (float)((i == 0) ? -8 : -1);
					PoolablePlayer poolablePlayer = UnityEngine.Object.Instantiate<PoolablePlayer>(this.PlayerPrefab, base.transform);
					poolablePlayer.transform.localPosition = new Vector3((float)(num * num2) * 1.5f, -1f + (float)num2 * 0.15f, num3 + (float)num2 * 0.01f) * 1f;
					float num4 = (1f - (float)num2 * 0.075f) * 1f;
					Vector3 vector = new Vector3(num4, num4, 1f);
					poolablePlayer.transform.localScale = vector;
					poolablePlayer.SetFlipX(i % 2 == 0);
					PlayerControl.SetPlayerMaterialColors(data.ColorId, poolablePlayer.Body);
					poolablePlayer.SetSkin(data.SkinId);
					poolablePlayer.HatSlot.SetHat(data.HatId, data.ColorId);
					PlayerControl.SetPetImage(data.PetId, data.ColorId, poolablePlayer.PetSlot);
					TextMeshPro nameText = poolablePlayer.NameText;
					nameText.text = data.PlayerName;
					nameText.transform.localScale = vector.Inv();
				}
			}
		}
	}

	private void OnDestroy()
	{
		if (this.overlayHandle != null)
		{
			this.overlayHandle.Dispose();
			this.overlayHandle = null;
		}
	}
}
