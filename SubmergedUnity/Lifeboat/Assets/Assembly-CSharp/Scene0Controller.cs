using System;
using System.Collections;
using UnityEngine;

public class Scene0Controller : SceneController
{
	public float Duration = 3f;

	public SpriteRenderer[] ExtraBoys;

	public AnimationCurve PopInCurve;

	public AnimationCurve PopOutCurve;

	public float OutDuration = 0.2f;

	public void OnEnable()
	{
		base.StartCoroutine(this.Run());
	}

	public void OnDisable()
	{
		for (int i = 0; i < this.ExtraBoys.Length; i++)
		{
			this.ExtraBoys[i].enabled = false;
		}
	}

	private IEnumerator Run()
	{
		int lastBoy = 0;
		float start = Time.time;
		for (;;)
		{
			float num = (Time.time - start) / this.Duration;
			int num2 = Mathf.RoundToInt((Mathf.Cos(3.1415927f * num + 3.1415927f) + 1f) / 2f * (float)this.ExtraBoys.Length);
			if (lastBoy < num2)
			{
				base.StartCoroutine(this.PopIn(this.ExtraBoys[lastBoy]));
				lastBoy = num2;
			}
			else if (lastBoy > num2)
			{
				lastBoy = num2;
				base.StartCoroutine(this.PopOut(this.ExtraBoys[lastBoy]));
			}
			yield return null;
		}
		yield break;
	}

	private IEnumerator PopIn(SpriteRenderer boy)
	{
		boy.enabled = true;
		for (float timer = 0f; timer < 0.2f; timer += Time.deltaTime)
		{
			float num = this.PopInCurve.Evaluate(timer / 0.2f);
			boy.transform.localScale = new Vector3(num, num, num);
			yield return null;
		}
		boy.transform.localScale = Vector3.one;
		yield break;
	}

	private IEnumerator PopOut(SpriteRenderer boy)
	{
		boy.enabled = true;
		for (float timer = 0f; timer < this.OutDuration; timer += Time.deltaTime)
		{
			float num = this.PopOutCurve.Evaluate(timer / this.OutDuration);
			boy.transform.localScale = new Vector3(num, num, num);
			yield return null;
		}
		boy.transform.localScale = Vector3.one;
		boy.enabled = false;
		yield break;
	}
}
