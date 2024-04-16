using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class ExileController : MonoBehaviour
{
	public static ExileController Instance;

	public TextMeshPro ImpostorText;

	public TextMeshPro Text;

	public SpriteRenderer Player;

	public HatParent PlayerHat;

	public SpriteRenderer PlayerSkin;

	public AnimationCurve LerpCurve;

	public float Duration = 7f;

	public AudioClip TextSound;

	public AudioClip EjectSound;

	protected string completeString = "TestPlayer was not The Impostor";

	protected GameData.PlayerInfo exiled;

	private SpecialInputHandler specialInputHandler;

	private void Awake()
	{
		this.specialInputHandler = base.GetComponent<SpecialInputHandler>();
	}

	public void Begin(GameData.PlayerInfo exiled, bool tie)
	{
		if (this.specialInputHandler != null)
		{
			this.specialInputHandler.disableVirtualCursor = true;
		}
		ExileController.Instance = this;
		this.exiled = exiled;
		this.Text.gameObject.SetActive(false);
		this.Text.text = string.Empty;
		int num = GameData.Instance.AllPlayers.Count((GameData.PlayerInfo p) => p.IsImpostor && !p.IsDead && !p.Disconnected);
		if (exiled != null)
		{
			int num2 = GameData.Instance.AllPlayers.Count((GameData.PlayerInfo p) => p.IsImpostor);
			if (!PlayerControl.GameOptions.ConfirmImpostor)
			{
				this.completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextNonConfirm, new object[]
				{
					exiled.PlayerName
				});
			}
			else if (exiled.IsImpostor)
			{
				if (num2 > 1)
				{
					this.completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextPP, new object[]
					{
						exiled.PlayerName
					});
				}
				else
				{
					this.completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextSP, new object[]
					{
						exiled.PlayerName
					});
				}
			}
			else if (num2 > 1)
			{
				this.completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextPN, new object[]
				{
					exiled.PlayerName
				});
			}
			else
			{
				this.completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextSN, new object[]
				{
					exiled.PlayerName
				});
			}
			PlayerControl.SetPlayerMaterialColors(exiled.ColorId, this.Player);
			this.PlayerHat.SetHat(exiled.HatId, exiled.ColorId);
			this.PlayerSkin.sprite = DestroyableSingleton<HatManager>.Instance.GetSkinById(exiled.SkinId).EjectFrame;
			if (exiled.IsImpostor)
			{
				num--;
			}
		}
		else
		{
			if (tie)
			{
				this.completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NoExileTie, Array.Empty<object>());
			}
			else
			{
				this.completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NoExileSkip, Array.Empty<object>());
			}
			this.Player.gameObject.SetActive(false);
		}
		if (num == 1)
		{
			this.ImpostorText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ImpostorsRemainS, new object[]
			{
				num
			});
		}
		else
		{
			this.ImpostorText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ImpostorsRemainP, new object[]
			{
				num
			});
		}
		base.StartCoroutine(this.Animate());
	}

	protected virtual IEnumerator Animate()
	{
		yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.black, Color.clear, 0.2f);
		yield return new WaitForSeconds(0.2f);
		if (this.exiled != null && this.EjectSound)
		{
			SoundManager.Instance.PlayDynamicSound("PlayerEjected", this.EjectSound, true, new DynamicSound.GetDynamicsFunction(this.SoundDynamics), true);
		}
		yield return new WaitForSeconds(0.8f);
		float num = Camera.main.orthographicSize * Camera.main.aspect + 1f;
		Vector2 left = Vector2.left * num;
		Vector2 right = Vector2.right * num;
		for (float t = 0f; t <= this.Duration; t += Time.deltaTime)
		{
			float num2 = t / this.Duration;
			this.Player.transform.localPosition = Vector2.Lerp(left, right, this.LerpCurve.Evaluate(num2));
			float num3 = (t + 0.75f) * 25f / Mathf.Exp(t * 0.75f + 1f);
			this.Player.transform.Rotate(new Vector3(0f, 0f, num3 * Time.deltaTime * 30f));
			if (num2 >= 0.3f)
			{
				int num4 = (int)(Mathf.Min(1f, (num2 - 0.3f) / 0.3f) * (float)this.completeString.Length);
				if (num4 > this.Text.text.Length)
				{
					this.Text.text = this.completeString.Substring(0, num4);
					this.Text.gameObject.SetActive(true);
					if (this.completeString[num4 - 1] != ' ')
					{
						SoundManager.Instance.PlaySoundImmediate(this.TextSound, false, 0.8f, 1f);
					}
				}
			}
			yield return null;
		}
		this.Text.text = this.completeString;
		if (PlayerControl.GameOptions.ConfirmImpostor)
		{
			this.ImpostorText.gameObject.SetActive(true);
		}
		yield return Effects.Bloop(0f, this.ImpostorText.transform, 1f, 0.5f);
		yield return new WaitForSeconds(0.5f);
		yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.clear, Color.black, 0.2f);
		this.WrapUp();
		yield break;
	}

	protected void WrapUp()
	{
		if (this.exiled != null)
		{
			PlayerControl @object = this.exiled.Object;
			if (@object)
			{
				@object.Exiled();
			}
			this.exiled.IsDead = true;
		}
		if (DestroyableSingleton<TutorialManager>.InstanceExists || !ShipStatus.Instance.IsGameOverDueToDeath())
		{
			this.ReEnableGameplay();
		}
		 UnityEngine.Object.Destroy(base.gameObject);
	}

	protected void ReEnableGameplay()
	{
		DestroyableSingleton<HudManager>.Instance.StartCoroutine(DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.black, Color.clear, 0.2f));
		PlayerControl.LocalPlayer.SetKillTimer(PlayerControl.GameOptions.KillCooldown);
		ShipStatus.Instance.EmergencyCooldown = (float)PlayerControl.GameOptions.EmergencyCooldown;
		Camera.main.GetComponent<FollowerCamera>().Locked = false;
		DestroyableSingleton<HudManager>.Instance.SetHudActive(true);
		ControllerManager.Instance.ResetAll();
	}

	private void SoundDynamics(AudioSource source, float dt)
	{
		source.volume = 0.8f;
		source.loop = false;
		source.panStereo = -1f;
	}
}
