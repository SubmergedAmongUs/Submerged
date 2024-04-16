using System;
using System.Collections;
using UnityEngine;

public class PulseWaitingText : MonoBehaviour
{
	public Transform textSource;

	private float duration = 1f;

	private void Awake()
	{
		base.StartCoroutine(this.GetBig());
	}

	private IEnumerator GetBig()
	{
		Vector3 vec = default(Vector3);
		for (float t = 0f; t < this.duration; t += Time.deltaTime)
		{
			float num = t / this.duration;
			float num2 = Mathf.SmoothStep(1f, 1.25f, num);
			vec.Set(num2, num2, num2);
			base.transform.localScale = vec;
			yield return null;
		}
		vec.Set(0f, 0f, 0f);
		base.transform.localScale = vec;
		base.StartCoroutine(this.GetSmall());
		yield break;
	}

	private IEnumerator GetSmall()
	{
		Vector3 vec = default(Vector3);
		for (float t = 0f; t < this.duration; t += Time.deltaTime)
		{
			float num = t / this.duration;
			float num2 = Mathf.SmoothStep(1.25f, 1f, num);
			vec.Set(num2, num2, num2);
			base.transform.localScale = vec;
			yield return null;
		}
		vec.Set(1f, 1f, 1f);
		base.transform.localScale = vec;
		base.StartCoroutine(this.GetBig());
		yield break;
	}
}
