using System;
using System.Collections;
using PowerTools;
using UnityEngine;

public class PbExileController : ExileController
{
	public SpriteRenderer HandSlot;

	public Sprite BadHand;

	public Sprite GoodHand;

	public AudioClip SplashSound;

	public SpriteAnim Sploosher;

	public AnimationClip Sploosh;

	protected override IEnumerator Animate()
	{
		yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.black, Color.clear, 0.2f);
		yield return Effects.Wait(0.75f);
		yield return Effects.All(new IEnumerator[]
		{
			this.PlayerFall(),
			this.PlayerSpin(),
			this.HandleText()
		});
		if (PlayerControl.GameOptions.ConfirmImpostor)
		{
			this.ImpostorText.gameObject.SetActive(true);
		}
		yield return Effects.Bloop(0f, this.ImpostorText.transform, 1f, 0.5f);
		yield return new WaitForSeconds(0.5f);
		yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.clear, Color.black, 0.2f);
		base.WrapUp();
		yield break;
	}

	private IEnumerator HandleText()
	{
		yield return Effects.Wait(this.Duration * 0.5f);
		float newDur = this.Duration * 0.5f;
		for (float t = 0f; t <= newDur; t += Time.deltaTime)
		{
			int num = (int)(t / newDur * (float)this.completeString.Length);
			if (num > this.Text.text.Length)
			{
				this.Text.text = this.completeString.Substring(0, num);
				this.Text.gameObject.SetActive(true);
				if (this.completeString[num - 1] != ' ')
				{
					SoundManager.Instance.PlaySoundImmediate(this.TextSound, false, 0.8f, 1f);
				}
			}
			yield return null;
		}
		this.Text.text = this.completeString;
		yield break;
	}

	private IEnumerator PlayerFall()
	{
		float num = Camera.main.orthographicSize + 1f;
		Vector2 top = Vector2.up * num;
		Vector2 bottom = Vector2.down * 2.81f;
		bool started = this.exiled == null;
		float d = this.Duration / 1.8f;
		for (float t = 0f; t <= d; t += Time.deltaTime)
		{
			float num2 = t / d;
			float num3 = this.LerpCurve.Evaluate(num2);
			Vector2 vector = Vector2.Lerp(top, bottom, num3);
			this.Player.transform.localPosition = vector;
			if (!started && vector.y < -1.7f)
			{
				started = true;
				this.Sploosher.Play(this.Sploosh, 1f);
				SoundManager.Instance.PlaySound(this.SplashSound, false, 1f);
				VibrationManager.Vibrate(0.2f, 0.2f, 0.35f, VibrationManager.VibrationFalloff.Linear, null, false);
			}
			yield return null;
		}
		yield break;
	}

	private IEnumerator PlayerSpin()
	{
		float num = Camera.main.orthographicSize + 1f;
		Vector2 top = Vector2.up * num;
		Vector2 bottom = Vector2.down * 2.81f;
		float d = this.Duration / 1.8f;
		for (float t = 0f; t <= d; t += Time.deltaTime)
		{
			float num2 = t / d;
			float num3 = (t + 0.75f) * 25f / Mathf.Exp(t * 0.75f + 1f);
			this.Player.transform.Rotate(new Vector3(0f, 0f, num3 * Time.deltaTime * 5f));
			yield return null;
		}
		if (!PlayerControl.GameOptions.ConfirmImpostor)
		{
			yield break;
		}
		if (this.exiled != null)
		{
			this.HandSlot.sprite = (this.exiled.IsImpostor ? this.GoodHand : this.BadHand);
			PlayerControl.SetPlayerMaterialColors(this.exiled.ColorId, this.HandSlot);
		}
		this.Player.transform.eulerAngles = new Vector3(0f, 0f, -10f);
		float duration = this.Duration / 4f;
		top.y = -1.78f;
		yield return Effects.Overlerp(duration, delegate(float p)
		{
			this.Player.transform.localPosition = Vector2.LerpUnclamped(bottom, top, p);
		}, 0.05f);
		float d2 = this.Duration / 2f;
		for (float t = 0f; t <= d2; t += Time.deltaTime)
		{
			float num4 = t / d2;
			Vector2 vector = Vector2.Lerp(top, bottom, num4);
			vector += UnityEngine.Random.insideUnitCircle * 0.025f;
			this.Player.transform.localPosition = vector;
			this.Player.transform.eulerAngles = new Vector3(0f, 0f, Mathf.Lerp(-10f, 17f, num4));
			yield return null;
		}
		yield break;
	}
}
