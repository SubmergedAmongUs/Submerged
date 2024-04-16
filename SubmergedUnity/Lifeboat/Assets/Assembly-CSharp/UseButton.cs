using System;
using TMPro;
using UnityEngine;

public class UseButton : MonoBehaviour
{
	public ImageNames imageName;

	public SpriteRenderer graphic;

	public TextMeshPro text;

	private void Awake()
	{
	}

	public void Hide()
	{
		this.graphic.enabled = false;
		this.text.enabled = false;
	}

	public void Show()
	{
		this.graphic.enabled = true;
		this.text.enabled = true;
		this.graphic.SetCooldownNormalizedUvs();
	}

	public void Show(float percentCool)
	{
		this.Show();
		this.graphic.material.SetFloat("_Percent", percentCool);
	}
}
