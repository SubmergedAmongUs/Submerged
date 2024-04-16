using System;
using UnityEngine;

public class LoadingMarquee : MonoBehaviour
{
	public float duration = 5f;

	public float distanceFromBottom = 0.25f;

	private FloatRange XRange;

	private float timer;

	public void Start()
	{
		Camera main = Camera.main;
		float num = main.orthographicSize * main.aspect * 1.1f;
		this.XRange = new FloatRange(num, -num);
		base.transform.localPosition = new Vector3(num, -main.orthographicSize + this.distanceFromBottom, -1f);
	}

	public void Update()
	{
		this.timer += Time.deltaTime;
		Vector3 localPosition = base.transform.localPosition;
		localPosition.x = this.XRange.Lerp(this.timer / this.duration % 1f);
		base.transform.localPosition = localPosition;
	}
}
