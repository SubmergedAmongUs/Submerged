using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.CoreScripts;
using InnerNet;
using TMPro;
using UnityEngine;

public class EndGameManager : MonoBehaviour
{
	public TextMeshPro WinText;

	public MeshRenderer BackgroundBar;

	public MeshRenderer Foreground;

	public FloatRange ForegroundRadius;

	public SpriteRenderer FrontMost;

	public PoolablePlayer PlayerPrefab;

	public Sprite GhostSprite;

	public SpriteRenderer PlayAgainButton;

	public SpriteRenderer ExitButton;

	public AudioClip DisconnectStinger;

	public AudioClip CrewStinger;

	public AudioClip ImpostorStinger;

	private const float BaseY = -1f;

	private const float ScaleAll = 0.9f;

	private const float OffsetWidth = 1f;

	private const float OffsetHeight = 0.1f;

	private float stingerTime;

	public void Start()
	{
		this.SetEverythingUp();
		base.StartCoroutine(this.CoBegin());
		base.Invoke("ShowButtons", 1.1f);
		ConsoleJoystick.SetMode_Menu();
	}

	private void ShowButtons()
	{
		this.FrontMost.gameObject.SetActive(false);
		this.PlayAgainButton.gameObject.SetActive(true);
		this.ExitButton.gameObject.SetActive(true);
	}

	private void SetEverythingUp()
	{
		StatsManager instance = StatsManager.Instance;
		uint gamesFinished = instance.GamesFinished;
		instance.GamesFinished = gamesFinished + 1U;
		bool flag = TempData.DidHumansWin(TempData.EndReason);
		if (TempData.EndReason == GameOverReason.ImpostorDisconnect)
		{
			StatsManager.Instance.AddDrawReason(TempData.EndReason);
			this.WinText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ImpostorDisconnected, Array.Empty<object>());
			SoundManager.Instance.PlaySound(this.DisconnectStinger, false, 1f);
		}
		else if (TempData.EndReason == GameOverReason.HumansDisconnect)
		{
			StatsManager.Instance.AddDrawReason(TempData.EndReason);
			this.WinText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.CrewmatesDisconnected, Array.Empty<object>());
			SoundManager.Instance.PlaySound(this.DisconnectStinger, false, 1f);
		}
		else
		{
			if (TempData.winners.Any((WinningPlayerData h) => h.IsYou))
			{
				StatsManager.Instance.AddWinReason(TempData.EndReason);
				this.WinText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Victory, Array.Empty<object>());
				this.BackgroundBar.material.SetColor("_Color", Palette.CrewmateBlue);
				WinningPlayerData winningPlayerData = TempData.winners.FirstOrDefault((WinningPlayerData h) => h.IsYou);
				if (winningPlayerData != null)
				{
					DestroyableSingleton<Telemetry>.Instance.WonGame(winningPlayerData.ColorId, winningPlayerData.HatId);
				}
			}
			else
			{
				StatsManager.Instance.AddLoseReason(TempData.EndReason);
				this.WinText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Defeat, Array.Empty<object>());
				this.WinText.color = Color.red;
			}
			if (flag)
			{
				SoundManager.Instance.PlayDynamicSound("Stinger", this.CrewStinger, false, new DynamicSound.GetDynamicsFunction(this.GetStingerVol), false);
			}
			else
			{
				SoundManager.Instance.PlayDynamicSound("Stinger", this.ImpostorStinger, false, new DynamicSound.GetDynamicsFunction(this.GetStingerVol), false);
			}
		}
		int num = Mathf.CeilToInt(7.5f);
		List<WinningPlayerData> list = TempData.winners.OrderBy(delegate(WinningPlayerData b)
		{
			if (!b.IsYou)
			{
				return 0;
			}
			return -1;
		}).ToList<WinningPlayerData>();
		for (int i = 0; i < list.Count; i++)
		{
			WinningPlayerData winningPlayerData2 = list[i];
			int num2 = (i % 2 == 0) ? -1 : 1;
			int num3 = (i + 1) / 2;
			float num4 = (float)num3 / (float)num;
			float num5 = Mathf.Lerp(1f, 0.75f, num4);
			float num6 = (float)((i == 0) ? -8 : -1);
			PoolablePlayer poolablePlayer = UnityEngine.Object.Instantiate<PoolablePlayer>(this.PlayerPrefab, base.transform);
			poolablePlayer.transform.localPosition = new Vector3(1f * (float)num2 * (float)num3 * num5, FloatRange.SpreadToEdges(-1.125f, 0f, num3, num), num6 + (float)num3 * 0.01f) * 0.9f;
			float num7 = Mathf.Lerp(1f, 0.65f, num4) * 0.9f;
			Vector3 vector = new Vector3(num7, num7, 1f);
			poolablePlayer.transform.localScale = vector;
			if (winningPlayerData2.IsDead)
			{
				poolablePlayer.Body.sprite = this.GhostSprite;
				poolablePlayer.SetDeadFlipX(i % 2 == 0);
			}
			else
			{
				poolablePlayer.SetFlipX(i % 2 == 0);
			}
			if (!winningPlayerData2.IsDead)
			{
				poolablePlayer.SetSkin(winningPlayerData2.SkinId);
			}
			else
			{
				poolablePlayer.HatSlot.color = new Color(1f, 1f, 1f, 0.5f);
			}
			PlayerControl.SetPlayerMaterialColors(winningPlayerData2.ColorId, poolablePlayer.Body);
			poolablePlayer.HatSlot.SetHat(winningPlayerData2.HatId, winningPlayerData2.ColorId);
			PlayerControl.SetPetImage(winningPlayerData2.PetId, winningPlayerData2.ColorId, poolablePlayer.PetSlot);
			if (flag)
			{
				poolablePlayer.NameText.gameObject.SetActive(false);
			}
			else
			{
				poolablePlayer.NameText.text = winningPlayerData2.Name;
				if (winningPlayerData2.IsImpostor)
				{
					poolablePlayer.NameText.color = Palette.ImpostorRed;
				}
				poolablePlayer.NameText.transform.localScale = vector.Inv();
				poolablePlayer.NameText.transform.SetLocalZ(-15f);
			}
		}
	}

	private void GetStingerVol(AudioSource source, float dt)
	{
		this.stingerTime += dt * 0.75f;
		source.volume = Mathf.Clamp(1f / this.stingerTime, 0f, 1f);
	}

	public IEnumerator CoBegin()
	{
		Color c = this.WinText.color;
		Color fade = Color.black;
		Color white = Color.white;
		Vector3 titlePos = this.WinText.transform.localPosition;
		float timer = 0f;
		while (timer < 3f)
		{
			timer += Time.deltaTime;
			float num = Mathf.Min(1f, timer / 3f);
			this.Foreground.material.SetFloat("_Rad", this.ForegroundRadius.ExpOutLerp(num * 2f));
			fade.a = Mathf.Lerp(1f, 0f, num * 3f);
			this.FrontMost.color = fade;
			c.a = Mathf.Clamp(FloatRange.ExpOutLerp(num, 0f, 1f), 0f, 1f);
			this.WinText.color = c;
			titlePos.y = 2.7f - num * 0.3f;
			this.WinText.transform.localPosition = titlePos;
			yield return null;
		}
		this.FrontMost.gameObject.SetActive(false);
		yield break;
	}

	public void NextGame()
	{
		this.PlayAgainButton.gameObject.SetActive(false);
		this.ExitButton.gameObject.SetActive(false);
		if (TempData.showAd && !SaveManager.BoughtNoAds)
		{
			TempData.showAd = false;
		}
		base.StartCoroutine(this.CoJoinGame());
	}

	public IEnumerator CoJoinGame()
	{
		AmongUsClient.Instance.JoinGame();
		yield return EndGameManager.WaitWithTimeout(() => AmongUsClient.Instance.ClientId >= 0);
		if (AmongUsClient.Instance.ClientId < 0)
		{
			AmongUsClient.Instance.ExitGame(AmongUsClient.Instance.LastDisconnectReason);
		}
		yield break;
	}

	public void Exit()
	{
		this.PlayAgainButton.gameObject.SetActive(false);
		this.ExitButton.gameObject.SetActive(false);
		AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);
	}

	public static IEnumerator WaitWithTimeout(Func<bool> success)
	{
		float timer = 0f;
		while (timer < 5f && !success())
		{
			yield return null;
			timer += Time.deltaTime;
		}
		yield break;
	}
}
