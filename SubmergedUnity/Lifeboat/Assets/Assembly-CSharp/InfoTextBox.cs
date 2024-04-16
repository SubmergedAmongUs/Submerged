using System;
using TMPro;
using UnityEngine;

public class InfoTextBox : MonoBehaviour
{
	public SpriteRenderer background;

	public bool isConfirmWindow;

	public TextMeshPro titleTexxt;

	public TextMeshPro bodyText;

	public TextMeshPro button1Text;

	public TextMeshPro button2Text;

	public PassiveButton button1;

	public PassiveButton button2;

	public Transform button1Trans;

	public Transform button2Trans;

	public void Awake()
	{
		this.SetOneButton();
		if (this.isConfirmWindow)
		{
			this.SetConfirmWindow();
		}
	}

	public void Close()
	{
		base.GetComponent<TransitionOpen>().Close();
	}

	public void SetConfirmWindow()
	{
		this.background.color = Color.green;
	}

	public void SetTwoButtons()
	{
		this.button2Trans.gameObject.SetActive(true);
		this.button1Trans.localPosition = new Vector2(2f, this.button1Trans.localPosition.y);
		this.button2Trans.localPosition = new Vector2(-2f, this.button2Trans.localPosition.y);
	}

	public void SetOneButton()
	{
		this.button2Trans.gameObject.SetActive(false);
		this.button1Trans.localPosition = new Vector2(0f, this.button1Trans.localPosition.y);
	}

	public void Update()
	{
		if (!ControllerManager.Instance.IsMenuActiveAtAll(base.gameObject.name))
		{
			ControllerNavMenu component = base.GetComponent<ControllerNavMenu>();
			Debug.Log("EOS: InfoTextBox overlay menu wasn't active, opening it...");
			component.OpenMenu(true);
		}
	}
}
