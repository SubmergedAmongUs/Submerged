using System;
using System.Collections;
using UnityEngine;

public class VendingSlot : MonoBehaviour
{
	public SpriteRenderer DrinkImage;

	public SpriteRenderer GlassImage;

	private const float SlideDuration = 0.75f;

	private const float SlideVibrateIntensity = 0.05f;

	private const float DrunkThunkVibrateIntensity = 0.4f;

	public IEnumerator CoBuy(AudioClip sliderOpen, AudioClip drinkShake, AudioClip drinkLand)
	{
		VibrationManager.Vibrate(0.05f, 0.05f, 0.75f, VibrationManager.VibrationFalloff.None, null, false);
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(sliderOpen, false, 1f);
		}
		yield return new WaitForLerp(0.75f, delegate(float v)
		{
			this.GlassImage.size = new Vector2(1f, Mathf.Lerp(1.7f, 0f, v));
			this.GlassImage.transform.localPosition = new Vector3(0f, Mathf.Lerp(0f, 0.85f, v), -1f);
		});
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(drinkShake, false, 1f);
		}
		yield return Effects.SwayX(this.DrinkImage.transform, 0.75f, 0.075f);
		Vector3 localPosition = this.DrinkImage.transform.localPosition;
		localPosition.z = -5f;
		this.DrinkImage.transform.localPosition = localPosition;
		Vector3 vector = localPosition;
		vector.y = -8f - localPosition.y;
		yield return Effects.All(new IEnumerator[]
		{
			Effects.Slide2D(this.DrinkImage.transform, localPosition, vector, 0.75f),
			Effects.Rotate2D(this.DrinkImage.transform, 0f, -FloatRange.Next(-45f, 45f), 0.75f),
			Effects.Sequence(new IEnumerator[]
			{
				Effects.Wait(0.25f),
				this.PlayLand(drinkLand)
			})
		});
		this.DrinkImage.enabled = false;
		yield break;
	}

	public IEnumerator CloseSlider(AudioClip sliderOpen)
	{
		VibrationManager.Vibrate(0.05f, 0.05f, 0.75f, VibrationManager.VibrationFalloff.None, null, false);
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(sliderOpen, false, 1f);
		}
		yield return new WaitForLerp(0.75f, delegate(float v)
		{
			this.GlassImage.size = new Vector2(1f, Mathf.Lerp(0f, 1.7f, v));
			this.GlassImage.transform.localPosition = new Vector3(0f, Mathf.Lerp(0.85f, 0f, v), -1f);
		});
		yield break;
	}

	private IEnumerator PlayLand(AudioClip drinkLand)
	{
		VibrationManager.Vibrate(0.4f, 0.4f, 0.5f, VibrationManager.VibrationFalloff.Linear, null, false);
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(drinkLand, false, 1f);
		}
		yield break;
	}
}
