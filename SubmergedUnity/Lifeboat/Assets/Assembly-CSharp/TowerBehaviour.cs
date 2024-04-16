using System;
using UnityEngine;

public class TowerBehaviour : MonoBehaviour
{
	public float timer;

	public float frameTime = 0.2f;

	public SpriteRenderer circle;

	public SpriteRenderer middle1;

	public SpriteRenderer middle2;

	public SpriteRenderer outer1;

	public SpriteRenderer outer2;

	public void Update()
	{
		this.timer += Time.deltaTime;
		if (this.timer < this.frameTime)
		{
			this.circle.color = Color.white;
			this.middle1.color = (this.middle2.color = (this.outer1.color = (this.outer2.color = Color.black)));
			return;
		}
		if (this.timer < 2f * this.frameTime)
		{
			this.middle1.color = (this.middle2.color = Color.white);
			this.circle.color = (this.outer1.color = (this.outer2.color = Color.black));
			return;
		}
		if (this.timer < 3f * this.frameTime)
		{
			this.outer1.color = (this.outer2.color = Color.white);
			this.middle1.color = (this.middle2.color = (this.circle.color = Color.black));
			return;
		}
		this.timer = 0f;
	}
}
