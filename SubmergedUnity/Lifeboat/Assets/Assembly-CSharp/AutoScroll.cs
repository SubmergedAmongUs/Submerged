using System;
using UnityEngine;

public class AutoScroll : MonoBehaviour
{
	public Transform t;

	public float speed;

	public float stoppingPoint;

	public float initialDelay = 2f;

	private void Start()
	{
		this.t.localPosition = Vector3.zero;
		this.initialDelay = 2f;
	}

	private void OnEnable()
	{
		this.t.localPosition = Vector3.zero;
		this.initialDelay = 2f;
	}

	private void Update()
	{
		if (this.initialDelay > 0f)
		{
			this.initialDelay -= Time.deltaTime;
			return;
		}
		if (this.t.localPosition.y < this.stoppingPoint)
		{
			this.t.localPosition = new Vector3(this.t.localPosition.x, this.t.localPosition.y + Time.deltaTime * this.speed, this.t.localPosition.z);
		}
		if (this.t.localPosition.y > this.stoppingPoint)
		{
			this.t.localPosition = new Vector3(this.t.localPosition.x, this.stoppingPoint, this.t.localPosition.z);
		}
	}
}
