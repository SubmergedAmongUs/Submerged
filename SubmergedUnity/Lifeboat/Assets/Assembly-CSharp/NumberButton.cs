using System;
using TMPro;
using UnityEngine;

public class NumberButton : PoolableBehavior
{
	public TextMeshPro Text;

	public PassiveButton Button;

	public SpriteRenderer Background;

	public int monthNum = -1;

	public void SetSelected(bool selected)
	{
		base.GetComponent<ButtonRolloverHandler>().OutColor = (this.Background.color = (selected ? Color.white : Color.black));
	}

	public override void Reset()
	{
		base.Reset();
	}
}
