using System;
using UnityEngine;

public class WaitingRotate : MonoBehaviour
{
	public float speed = 20f;

	public float distanceFromBottom = 0.25f;

	public void Start()
	{
		Camera main = Camera.main;
		float num = main.orthographicSize * main.aspect * 1.1f;
		base.transform.localPosition = new Vector3(num - 1f, -main.orthographicSize + this.distanceFromBottom, -1f);
	}

	public void Update()
	{
		base.transform.RotateAround(base.transform.position, Vector3.forward, this.speed * Time.deltaTime);
	}
}
