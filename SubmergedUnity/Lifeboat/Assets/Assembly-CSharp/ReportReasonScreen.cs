using System;
using InnerNet;
using TMPro;
using UnityEngine;

public class ReportReasonScreen : MonoBehaviour
{
	public BanMenu Parent;

	public ButtonRolloverHandler[] Buttons;

	public TextMeshPro NameText;

	public SpriteRenderer PlayerIcon;

	private ReportReasons? currentReason;

	public ReportConfirmScreen ConfirmScreen;

	private string playerName;

	private int colorId;

	public void Show(string playerName, int colorId)
	{
		Vector3 position = base.transform.position;
		Vector3 vector = DestroyableSingleton<HudManager>.Instance.UICamera.transform.TransformPoint(Vector3.zero);
		position.x = vector.x;
		position.y = vector.y;
		base.transform.position = position;
		this.playerName = playerName;
		this.colorId = colorId;
		this.NameText.text = playerName;
		PlayerControl.SetPlayerMaterialColors(colorId, this.PlayerIcon);
		this.SelectReason(-1);
		base.gameObject.SetActive(true);
	}

	public void SelectReason(int reason)
	{
		if (reason < 0 || reason > 3)
		{
			this.currentReason = null;
		}
		else
		{
			this.currentReason = new ReportReasons?((ReportReasons)reason);
		}
		for (int i = 0; i < this.Buttons.Length; i++)
		{
			this.Buttons[i].ChangeOutColor((i == reason) ? Color.green : Color.white);
		}
	}

	public void Submit()
	{
		if (this.currentReason == null)
		{
			return;
		}
		this.Parent.ReportPlayer(this.currentReason.Value);
		this.Hide();
		this.ConfirmScreen.Show(this.playerName, this.colorId);
	}

	public void Hide()
	{
		base.gameObject.SetActive(false);
	}
}
