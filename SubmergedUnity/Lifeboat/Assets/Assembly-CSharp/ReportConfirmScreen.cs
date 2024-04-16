using System;
using TMPro;
using UnityEngine;

public class ReportConfirmScreen : MonoBehaviour
{
	public TextMeshPro NameText;

	public SpriteRenderer PlayerIcon;

	public void Show(string playerName, int colorId)
	{
		this.NameText.text = playerName;
		PlayerControl.SetPlayerMaterialColors(colorId, this.PlayerIcon);
		base.gameObject.SetActive(true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(false);
	}
}
