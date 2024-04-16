using System;
using UnityEngine;
using UnityEngine.Events;

public abstract class UiElement : MonoBehaviour
{
	public UnityEvent OnMouseOver;

	public UnityEvent OnMouseOut;

	public virtual void ReceiveMouseOut()
	{
		this.OnMouseOut.Invoke();
	}

	public virtual void ReceiveMouseOver()
	{
		this.OnMouseOver.Invoke();
	}
}
