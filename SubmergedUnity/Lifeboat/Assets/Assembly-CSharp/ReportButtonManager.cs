using System;
using TMPro;
using UnityEngine;

public class ReportButtonManager : MonoBehaviour
{
	public SpriteRenderer renderer;

	public TextMeshPro text;

	public void SetActive(bool isActive)
	{
		if (isActive)
		{
			this.renderer.color = Palette.EnabledColor;
			this.text.color = Palette.EnabledColor;
			this.renderer.material.SetFloat("_Desat", 0f);
			return;
		}
		this.renderer.color = Palette.DisabledClear;
		this.text.color = Palette.DisabledClear;
		this.renderer.material.SetFloat("_Desat", 1f);
	}

	public void DoClick()
	{
		if (!base.isActiveAndEnabled)
		{
			return;
		}
		PlayerControl.LocalPlayer.ReportClosest();
	}
}
