using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EdgeTransition : MonoBehaviour
{
	public float Duration = 0.2f;

	public Vector3 OpenPosition;

	public Vector3 ClosedPosition;

	public AspectPosition.EdgeAlignments Alignment;

	public Button.ButtonClickedEvent OnClose = new Button.ButtonClickedEvent();

	public void Awake()
	{
		base.transform.localPosition = AspectPosition.ComputePosition(this.Alignment, this.ClosedPosition);
	}

	public void Open()
	{
		base.gameObject.SetActive(true);
		base.StopAllCoroutines();
		base.StartCoroutine(this.CoOpen());
	}

	private IEnumerator CoOpen()
	{
		Vector3 sourcePos = base.transform.localPosition;
		Vector3 targetPos = AspectPosition.ComputePosition(this.Alignment, this.OpenPosition);
		Vector3 localPosition = default(Vector3);
		for (float timer = 0f; timer < this.Duration; timer += Time.deltaTime)
		{
			localPosition = Vector3.Lerp(sourcePos, targetPos, timer / this.Duration);
			base.transform.localPosition = localPosition;
			yield return null;
		}
		base.transform.localPosition = targetPos;
		yield break;
	}

	public void Close()
	{
		if (!base.isActiveAndEnabled)
		{
			return;
		}
		base.StopAllCoroutines();
		base.StartCoroutine(this.CoClose());
	}

	private IEnumerator CoClose()
	{
		Vector3 sourcePos = base.transform.localPosition;
		Vector3 targetPos = AspectPosition.ComputePosition(this.Alignment, this.ClosedPosition);
		Vector3 localPosition = default(Vector3);
		for (float timer = 0f; timer < this.Duration; timer += Time.deltaTime)
		{
			localPosition = Vector3.Lerp(sourcePos, targetPos, timer / this.Duration);
			base.transform.localPosition = localPosition;
			yield return null;
		}
		base.transform.localPosition = targetPos;
		this.OnClose.Invoke();
		yield break;
	}
}
