using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SlideOpen : MonoBehaviour
{
	public float duration = 0.2f;

	public Vector3 closedPosition = Vector3.zero;

	public Vector3 openPosition = Vector3.zero;

	public Button.ButtonClickedEvent OnClose = new Button.ButtonClickedEvent();

	public Camera parentCam;

	public bool isOpen;

	public void Awake()
	{
		Camera camera = this.parentCam ? this.parentCam : Camera.main;
		float orthographicSize = camera.orthographicSize;
		Rect safeArea = Screen.safeArea;
		float aspect = Mathf.Min(camera.aspect, safeArea.width / safeArea.height);
		this.closedPosition = AspectPosition.ComputePosition(AspectPosition.EdgeAlignments.Left, new Vector3(-3.2f, 0f, this.closedPosition.z), orthographicSize, aspect);
		base.transform.localPosition = this.closedPosition;
	}

	public void Toggle()
	{
		base.StopAllCoroutines();
		if (this.isOpen)
		{
			this.Close();
			return;
		}
		this.Open();
	}

	public void Close()
	{
		if (this.isOpen)
		{
			base.StartCoroutine(this.AnimateClose());
		}
	}

	public void Open()
	{
		if (!this.isOpen)
		{
			base.StartCoroutine(this.AnimateOpen());
		}
	}

	private IEnumerator AnimateClose()
	{
		for (float t = 0f; t < this.duration; t += Time.deltaTime)
		{
			float num = t / this.duration;
			Vector3 positionVector = Vector3.Lerp(this.openPosition, this.closedPosition, num);
			this.SetPositionVector(positionVector);
			yield return null;
		}
		this.SetPositionVector(this.closedPosition);
		this.isOpen = false;
		yield break;
	}

	private IEnumerator AnimateOpen()
	{
		for (float t = 0f; t < this.duration; t += Time.deltaTime)
		{
			float num = t / this.duration;
			Vector3 positionVector = Vector3.Lerp(this.closedPosition, this.openPosition, num);
			this.SetPositionVector(positionVector);
			yield return null;
		}
		this.isOpen = true;
		this.SetPositionVector(this.openPosition);
		yield break;
	}

	private void SetPositionVector(Vector3 pos)
	{
		base.transform.localPosition = pos;
	}
}
