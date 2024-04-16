using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class ShhhBehaviour : MonoBehaviour
{
	public SpriteRenderer Background;

	public SpriteRenderer Body;

	public SpriteRenderer Hand;

	public TextMeshPro TextImage;

	public float RotateSpeed = 15f;

	public Vector2Range HandTarget;

	public AnimationCurve PositionEasing;

	public FloatRange HandRotate;

	public AnimationCurve RotationEasing;

	public Vector2Range TextTarget;

	public AnimationCurve TextEasing;

	public float Duration = 0.5f;

	public float Delay = 0.1f;

	public float TextDuration = 0.5f;

	public float PulseDuration = 0.1f;

	public float PulseSize = 0.1f;

	public float HoldDuration = 2f;

	public bool Autoplay;

	public void OnEnable()
	{
		if (this.Autoplay)
		{
			Vector3 localScale = default(Vector3);
			this.UpdateHand(ref localScale, 1f);
			this.UpdateText(ref localScale, 1f);
			localScale.Set(1f, 1f, 1f);
			this.Body.transform.localScale = localScale;
			this.TextImage.color = Color.white;
		}
	}

	public IEnumerator PlayAnimation()
	{
		base.StartCoroutine(this.AnimateHand());
		yield return this.AnimateText();
		yield return ShhhBehaviour.WaitWithInterrupt(this.HoldDuration);
		yield break;
	}

	public void Update()
	{
		this.Background.transform.Rotate(0f, 0f, Time.deltaTime * this.RotateSpeed);
	}

	private IEnumerator AnimateText()
	{
		this.TextImage.color = Palette.ClearWhite;
		for (float t = 0f; t < this.Delay; t += Time.deltaTime)
		{
			yield return null;
		}
		Vector3 vec = default(Vector3);
		for (float t = 0f; t < this.PulseDuration; t += Time.deltaTime)
		{
			float num = t / this.PulseDuration;
			float num2 = 1f + Mathf.Sin(3.1415927f * num) * this.PulseSize;
			vec.Set(num2, num2, 1f);
			this.Body.transform.localScale = vec;
			this.TextImage.color = Color.Lerp(Palette.ClearWhite, Palette.White, num * 2f);
			yield return null;
		}
		vec.Set(1f, 1f, 1f);
		this.Body.transform.localScale = vec;
		this.TextImage.color = Color.white;
		yield break;
	}

	private IEnumerator AnimateHand()
	{
		this.Hand.transform.localPosition = this.HandTarget.min;
		Vector3 vec = default(Vector3);
		for (float t = 0f; t < this.Duration; t += Time.deltaTime)
		{
			float p = t / this.Duration;
			this.UpdateHand(ref vec, p);
			yield return null;
		}
		this.UpdateHand(ref vec, 1f);
		yield break;
	}

	private void UpdateHand(ref Vector3 vec, float p)
	{
		this.HandTarget.LerpUnclamped(ref vec, this.PositionEasing.Evaluate(p), -1f);
		this.Hand.transform.localPosition = vec;
		vec.Set(0f, 0f, this.HandRotate.LerpUnclamped(this.RotationEasing.Evaluate(p)));
		this.Hand.transform.eulerAngles = vec;
	}

	private void UpdateText(ref Vector3 vec, float p)
	{
		this.TextTarget.LerpUnclamped(ref vec, p, -2f);
		this.TextImage.transform.localPosition = vec;
	}

	public static IEnumerator WaitWithInterrupt(float duration)
	{
		float timer = 0f;
		while (timer < duration && !ShhhBehaviour.CheckForInterrupt())
		{
			yield return null;
			timer += Time.deltaTime;
		}
		yield break;
	}

	public static bool CheckForInterrupt()
	{
		return Input.anyKeyDown;
	}
}
