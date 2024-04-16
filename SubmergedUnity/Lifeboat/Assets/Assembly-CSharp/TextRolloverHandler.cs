using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TextRolloverHandler : MonoBehaviour
{
	public TextMeshPro Target;

	public Color OverColor = Color.green;

	public Color OverOutlineColor = Color.white;

	public Color OutColor = Color.white;

	public Color OutOutlineColor = Color.white;

	public void Start()
	{
		PassiveButton component = base.GetComponent<PassiveButton>();
		component.OnMouseOver.AddListener(new UnityAction(this.DoMouseOver));
		component.OnMouseOut.AddListener(new UnityAction(this.DoMouseOut));
	}

	public void DoMouseOver()
	{
		this.Target.color = this.OverColor;
		this.Target.outlineColor = this.OverOutlineColor;
	}

	public void DoMouseOut()
	{
		this.Target.color = this.OutColor;
		this.Target.outlineColor = this.OutOutlineColor;
	}
}
