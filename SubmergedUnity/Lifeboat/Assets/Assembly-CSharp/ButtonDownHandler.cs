using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ButtonDownHandler : MonoBehaviour
{
	private Coroutine downState;

	public SpriteRenderer Target;

	public Sprite UpSprite;

	public Sprite DownSprite;

	public void Start()
	{
		base.GetComponent<PassiveButton>().OnClick.AddListener(new UnityAction(this.StartDown));
	}

	public void OnDisable()
	{
		if (this.downState != null)
		{
			base.StopCoroutine(this.downState);
			this.downState = null;
			this.Target.sprite = this.UpSprite;
		}
	}

	private void StartDown()
	{
		if (this.downState == null)
		{
			this.downState = base.StartCoroutine(this.CoRunDown());
		}
	}

	private IEnumerator CoRunDown()
	{
		this.Target.sprite = this.DownSprite;
		while (DestroyableSingleton<PassiveButtonManager>.Instance.controller.AnyTouch)
		{
			yield return null;
		}
		this.Target.sprite = this.UpSprite;
		this.downState = null;
		yield break;
	}
}
