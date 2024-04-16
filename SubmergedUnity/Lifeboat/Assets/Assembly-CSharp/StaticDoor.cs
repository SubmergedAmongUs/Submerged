using System;
using UnityEngine;

public class StaticDoor : MonoBehaviour
{
	public Sprite OpenDoorImage;

	public Sprite CloseDoorImage;

	public bool IsOpen { get; private set; }

	public void SetOpen(bool isOpen)
	{
		this.IsOpen = isOpen;
		Collider2D component = base.GetComponent<Collider2D>();
		SpriteRenderer component2 = base.GetComponent<SpriteRenderer>();
		EdgeCollider2D componentInChildren = base.GetComponentInChildren<EdgeCollider2D>();
		if (isOpen)
		{
			component.enabled = false;
			component2.sprite = this.OpenDoorImage;
			if (componentInChildren)
			{
				componentInChildren.enabled = false;
				return;
			}
		}
		else
		{
			component.enabled = true;
			component2.sprite = this.CloseDoorImage;
			if (componentInChildren)
			{
				componentInChildren.enabled = true;
			}
		}
	}
}
