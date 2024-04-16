using System;
using UnityEngine;

public class CourseStarBehaviour : MonoBehaviour
{
	public SpriteRenderer Upper;

	public SpriteRenderer Lower;

	public float Speed = 30f;

	public void Update()
	{
		this.Upper.transform.Rotate(0f, 0f, Time.deltaTime * this.Speed);
		this.Lower.transform.Rotate(0f, 0f, Time.deltaTime * this.Speed);
	}
}
