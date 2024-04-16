using System;
using PowerTools;
using UnityEngine;
using UnityEngine.Events;

public class ButtonAnimRolloverHandler : MonoBehaviour
{
	public Sprite StaticOutImage;

	public AnimationClip RolloverAnim;

	public AudioClip HoverSound;

	private SpriteRenderer target;

	private SpriteAnim animTarget;

	public void Start()
	{
		this.target = base.GetComponent<SpriteRenderer>();
		this.animTarget = base.GetComponent<SpriteAnim>();
		PassiveButton component = base.GetComponent<PassiveButton>();
		component.OnMouseOver.AddListener(new UnityAction(this.DoMouseOver));
		component.OnMouseOut.AddListener(new UnityAction(this.DoMouseOut));
	}

	public void DoMouseOver()
	{
		this.animTarget.Play(this.RolloverAnim, 1f);
		if (this.HoverSound)
		{
			SoundManager.Instance.PlaySound(this.HoverSound, false, 1f);
		}
	}

	public void DoMouseOut()
	{
		this.animTarget.Stop();
		this.target.sprite = this.StaticOutImage;
	}
}
