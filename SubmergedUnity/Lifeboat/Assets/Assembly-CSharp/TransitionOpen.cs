using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TransitionOpen : MonoBehaviour
{
	public float duration = 0.2f;

	public Button.ButtonClickedEvent OnClose = new Button.ButtonClickedEvent();

	public void OnEnable()
	{
		base.StartCoroutine(this.AnimateOpen());
	}

	public void Toggle()
	{
		base.StopAllCoroutines();
		if (base.isActiveAndEnabled)
		{
			this.Close();
			return;
		}
		base.gameObject.SetActive(true);
		this.OnEnable();
	}

	public void Close()
	{
		if (base.isActiveAndEnabled)
		{
			base.StartCoroutine(this.AnimateClose());
		}
	}

	private IEnumerator AnimateClose()
	{
		Vector3 vec = default(Vector3);
		for (float t = 0f; t < this.duration; t += Time.deltaTime)
		{
			float num = t / this.duration;
			float num2 = Mathf.SmoothStep(1f, 0f, num);
			vec.Set(num2, num2, num2);
			base.transform.localScale = vec;
			yield return null;
		}
		vec.Set(0f, 0f, 0f);
		base.transform.localScale = vec;
		this.OnClose.Invoke();
		yield break;
	}

	private IEnumerator AnimateOpen()
	{
		Vector3 vec = default(Vector3);
		for (float t = 0f; t < this.duration; t += Time.deltaTime)
		{
			float num = t / this.duration;
			float num2 = Mathf.SmoothStep(0f, 1f, num);
			vec.Set(num2, num2, num2);
			base.transform.localScale = vec;
			yield return null;
		}
		vec.Set(1f, 1f, 1f);
		base.transform.localScale = vec;
		yield break;
	}
}
