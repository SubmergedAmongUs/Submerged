using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KeypadGame : Minigame
{
	public TextMeshPro TargetText;

	public TextMeshPro NumberText;

	public int number;

	public string numString = string.Empty;

	private bool animating;

	public SpriteRenderer AcceptButton;

	private LifeSuppSystemType system;

	private NoOxyTask oxyTask;

	private bool done;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultButtonSelected;

	public List<UiElement> ControllerSelectable;

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(base.name);
	}

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		this.oxyTask = (NoOxyTask)task;
		this.TargetText.text = "today's code:\r\n" + this.oxyTask.targetNumber.ToString("D5");
		this.NumberText.text = string.Empty;
		this.system = (LifeSuppSystemType)ShipStatus.Instance.Systems[SystemTypes.LifeSupp];
		this.done = this.system.GetConsoleComplete(base.ConsoleId);
		ControllerManager.Instance.OpenOverlayMenu(base.name, this.BackButton, this.DefaultButtonSelected, this.ControllerSelectable, false);
	}

	public void ClickNumber(int i)
	{
		if (this.animating)
		{
			return;
		}
		if (this.done)
		{
			return;
		}
		if (this.NumberText.text.Length >= 5)
		{
			base.StartCoroutine(this.BlinkAccept());
			return;
		}
		this.numString += i.ToString();
		this.number = this.number * 10 + i;
		this.NumberText.text = this.numString;
	}

	private IEnumerator BlinkAccept()
	{
		int num;
		for (int i = 0; i < 5; i = num)
		{
			this.AcceptButton.color = Color.gray;
			yield return null;
			yield return null;
			this.AcceptButton.color = Color.white;
			yield return null;
			yield return null;
			num = i + 1;
		}
		yield break;
	}

	public void ClearEntry()
	{
		if (this.animating)
		{
			return;
		}
		this.number = 0;
		this.numString = string.Empty;
		this.NumberText.text = string.Empty;
	}

	public void Enter()
	{
		if (this.animating)
		{
			return;
		}
		base.StartCoroutine(this.Animate());
	}

	private IEnumerator Animate()
	{
		this.animating = true;
		WaitForSeconds wait = new WaitForSeconds(0.1f);
		yield return wait;
		this.NumberText.text = string.Empty;
		yield return wait;
		if (this.oxyTask.targetNumber == this.number)
		{
			this.done = true;
			byte amount = (byte)(base.ConsoleId | 64);
			ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, (int)amount);
			try
			{
				((SabotageTask)this.MyTask).MarkContributed();
			}
			catch
			{
			}
			string okStr = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.OK, Array.Empty<object>());
			this.NumberText.text = okStr;
			yield return wait;
			this.NumberText.text = string.Empty;
			yield return wait;
			this.NumberText.text = okStr;
			yield return wait;
			this.NumberText.text = string.Empty;
			yield return wait;
			this.NumberText.text = okStr;
			base.StartCoroutine(base.CoStartClose(0.75f));
			okStr = null;
		}
		else
		{
			string okStr = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Bad, Array.Empty<object>());
			this.NumberText.text = okStr;
			yield return wait;
			this.NumberText.text = string.Empty;
			yield return wait;
			this.NumberText.text = okStr;
			yield return wait;
			this.numString = string.Empty;
			this.number = 0;
			this.NumberText.text = this.numString;
			okStr = null;
		}
		this.animating = false;
		yield break;
	}
}
