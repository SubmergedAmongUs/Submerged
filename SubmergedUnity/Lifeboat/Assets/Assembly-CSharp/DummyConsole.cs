using System;
using UnityEngine;

public class DummyConsole : MonoBehaviour
{
	public int ConsoleId;

	public PlayerAnimator[] Players;

	public float UseDistance;

	[HideInInspector]
	private SpriteRenderer rend;

	public void Start()
	{
		this.rend = base.GetComponent<SpriteRenderer>();
	}

	public void FixedUpdate()
	{
		this.rend.material.SetColor("_OutlineColor", Color.yellow);
		float num = float.MaxValue;
		for (int i = 0; i < this.Players.Length; i++)
		{
			PlayerAnimator playerAnimator = this.Players[i];
			Vector2 vector = base.transform.position - playerAnimator.transform.position;
			vector.y += 0.3636f;
			float magnitude = vector.magnitude;
			if (magnitude < num)
			{
				num = magnitude;
			}
			if (magnitude < this.UseDistance)
			{
				playerAnimator.NearbyConsoles |= 1 << this.ConsoleId;
			}
			else
			{
				playerAnimator.NearbyConsoles &= ~(1 << this.ConsoleId);
			}
		}
		if (num >= this.UseDistance * 2f)
		{
			this.rend.material.SetFloat("_Outline", 0f);
			this.rend.material.SetColor("_AddColor", Color.clear);
			return;
		}
		this.rend.material.SetFloat("_Outline", 1f);
		if (num < this.UseDistance)
		{
			this.rend.material.SetColor("_AddColor", Color.yellow);
			return;
		}
		this.rend.material.SetColor("_AddColor", Color.clear);
	}
}
