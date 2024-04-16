using System;
using TMPro;
using UnityEngine;

public class ServerListButton : PoolableBehavior
{
	public TextMeshPro Text;

	public PassiveButton Button;

	public SpriteRenderer Background;

	public TextTranslatorTMP textTranslator;

	public void SetSelected(bool selected)
	{
		base.GetComponent<ButtonRolloverHandler>().OutColor = (this.Background.color = (selected ? Color.white : Color.black));
	}

	public void SetTextTranslationId(StringNames id, string defaultStr)
	{
		this.textTranslator.TargetText = id;
		this.textTranslator.defaultStr = defaultStr;
	}
}
