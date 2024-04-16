using System;
using UnityEngine;

public class KeySlotBehaviour : MonoBehaviour
{
	public Sprite Highlit;

	public Sprite Inserted;

	public Sprite Finished;

	public SpriteRenderer Image;

	public BoxCollider2D Hitbox;

	internal void SetFinished()
	{
		this.Image.sprite = this.Finished;
		base.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
	}

	internal void SetInserted()
	{
		this.Image.sprite = this.Inserted;
	}

	internal void SetHighlight()
	{
		this.Image.sprite = this.Highlit;
	}
}
