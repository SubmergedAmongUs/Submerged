using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ConditionalSpriteSetter : MonoBehaviour
{
	public bool ignoreIfNoSpriteForPlatform = true;

	public ConditionalSprite sprite;

	private void Start()
	{
		SpriteRenderer component = base.GetComponent<SpriteRenderer>();
		if (component && (this.sprite.Select() || !this.ignoreIfNoSpriteForPlatform))
		{
			component.sprite = this.sprite;
		}
	}
}
