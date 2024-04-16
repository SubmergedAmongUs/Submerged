using System;
using UnityEngine;

public class WireNode : MonoBehaviour
{
	public Collider2D hitbox;

	public SpriteRenderer[] WireColors;

	public SpriteRenderer BaseSymbol;

	public sbyte WireId;

	internal void SetColor(Color color, Sprite symbol)
	{
		this.BaseSymbol.sprite = symbol;
		for (int i = 0; i < this.WireColors.Length; i++)
		{
			this.WireColors[i].color = color;
		}
	}
}
