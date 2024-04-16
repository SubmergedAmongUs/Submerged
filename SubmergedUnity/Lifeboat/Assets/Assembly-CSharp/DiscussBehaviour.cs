using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class DiscussBehaviour : MonoBehaviour
{
	public SpriteRenderer LeftPlayer;

	public SpriteRenderer RightPlayer;

	public TextMeshPro Text;

	public FloatRange RotateRange = new FloatRange(-5f, 5f);

	public Vector2Range TextTarget;

	public AnimationCurve TextEasing;

	public float Delay = 0.1f;

	public float TextDuration = 0.5f;

	public float HoldDuration = 2f;

	private Vector3 vec;

	public IEnumerator PlayAnimation()
	{
		this.Text.transform.localPosition = this.TextTarget.min;
		yield return this.AnimateText();
		yield return ShhhBehaviour.WaitWithInterrupt(this.HoldDuration);
		yield break;
	}

	public void Update()
	{
		this.vec.Set(0f, 0f, this.RotateRange.Lerp(Mathf.PerlinNoise(1f, Time.time * 8f)));
		this.LeftPlayer.transform.eulerAngles = this.vec;
		this.vec.Set(0f, 0f, this.RotateRange.Lerp(Mathf.PerlinNoise(2f, Time.time * 8f)));
		this.RightPlayer.transform.eulerAngles = this.vec;
	}

	private IEnumerator AnimateText()
	{
		for (float t = 0f; t < this.Delay; t += Time.deltaTime)
		{
			yield return null;
		}
		Vector3 vec = default(Vector3);
		for (float t = 0f; t < this.TextDuration; t += Time.deltaTime)
		{
			float num = t / this.TextDuration;
			this.UpdateText(ref vec, this.TextEasing.Evaluate(num));
			yield return null;
		}
		this.UpdateText(ref vec, 1f);
		yield break;
	}

	private void UpdateText(ref Vector3 vec, float p)
	{
		this.TextTarget.LerpUnclamped(ref vec, p, -7f);
		this.Text.transform.localPosition = vec;
	}
}
