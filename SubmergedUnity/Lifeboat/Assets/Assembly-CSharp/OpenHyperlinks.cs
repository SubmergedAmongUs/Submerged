using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class OpenHyperlinks : MonoBehaviour
{
	public TextMeshPro pTextMeshPro;

	public Color linkColor;

	public void SetLinkColor()
	{
		this.pTextMeshPro.text = this.pTextMeshPro.text.Replace("<link=", "<color=#7272e3><link=");
		this.pTextMeshPro.text = this.pTextMeshPro.text.Replace("</link>", "</color></link>");
	}

	public void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			int num = TMP_TextUtilities.FindIntersectingLink(this.pTextMeshPro, Input.mousePosition, Camera.main);
			if (num != -1)
			{
				TMP_LinkInfo tmp_LinkInfo = this.pTextMeshPro.textInfo.linkInfo[num];
				Application.OpenURL(tmp_LinkInfo.GetLinkID());
			}
		}
	}
}
