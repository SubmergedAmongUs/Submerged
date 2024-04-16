using System;
using System.Collections;
using UnityEngine;

public class MiraExileController : ExileController
{
	public Transform BackgroundClouds;

	public Transform ForegroundClouds;

	protected override IEnumerator Animate()
	{
		yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.black, Color.clear, 0.2f);
		yield return Effects.All(new IEnumerator[]
		{
			this.PlayerSpin(),
			this.HandleText(),
			Effects.Slide2D(this.BackgroundClouds, new Vector2(0f, -3f), new Vector2(0f, 0.5f), this.Duration),
			Effects.Sequence(new IEnumerator[]
			{
				Effects.Wait(2f),
				Effects.Slide2D(this.ForegroundClouds, new Vector2(0f, -7f), new Vector2(0f, 2.5f), 0.75f)
			})
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

	private IEnumerator PlayerSpin()
	{
		float num = Camera.main.orthographicSize + 1f;
		Vector2 top = Vector2.up * num;
		Vector2 bottom = Vector2.down * num;
		for (float t = 0f; t <= this.Duration; t += Time.deltaTime)
		{
			float num2 = t / this.Duration;
			this.Player.transform.localPosition = Vector2.Lerp(top, bottom, num2);
			float num3 = (t + 0.75f) * 25f / Mathf.Exp(t * 0.75f + 1f);
			this.Player.transform.Rotate(new Vector3(0f, 0f, num3 * Time.deltaTime * 5f));
			yield return null;
		}
		yield break;
	}
}
