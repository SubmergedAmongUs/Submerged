using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AirshipAuthGame : Minigame
{
	public TextMeshPro TargetText;

	public TextMeshPro NumberText;

	public TextMeshPro OtherStatusText;

	public int number;

	public string numString = string.Empty;

	private bool animating;

	private HeliSabotageSystem system;

	public SpriteRenderer OurLight;

	public SpriteRenderer TheirLight;

	public SpriteRenderer TimeBar;

	public AudioClip ButtonSound;

	public AudioClip AcceptSound;

	public AudioClip RejectSound;

	private int OtherConsoleId;

	private bool evenColor;

	public UiElement defaultSelection;

	public UiElement closeButton;

	public List<UiElement> selectableButtons;

	public override void Begin(PlayerTask task)
	{
		this.OtherConsoleId = (base.ConsoleId + 1) % 2;
		base.Begin(task);
		this.system = (ShipStatus.Instance.Systems[SystemTypes.Reactor] as HeliSabotageSystem);
		ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 64 | base.ConsoleId);
		ControllerManager.Instance.OpenOverlayMenu(base.gameObject.name, this.closeButton, this.defaultSelection, this.selectableButtons, false);
	}

	public override void Close()
	{
		base.Close();
		ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 32 | base.ConsoleId);
		ControllerManager.Instance.CloseOverlayMenu(base.gameObject.name);
	}

	public void Update()
	{
		this.evenColor = ((int)Time.time * 2 % 2 == 0);
		Vector3 localScale = this.TimeBar.transform.localScale;
		localScale.x = this.system.PercentActive;
		this.TimeBar.transform.localScale = localScale;
		if (this.system.PercentActive < 0.25f)
		{
			this.TimeBar.color = new Color(1f, 0.45f, 0.25f);
		}
		else if ((double)this.system.PercentActive < 0.5)
		{
			this.TimeBar.color = Color.yellow;
		}
		else
		{
			this.TimeBar.color = Color.white;
		}
		this.TargetText.text = this.system.TargetCode.ToString("D5");
		if (this.system.IsConsoleOkay(base.ConsoleId))
		{
			this.OurLight.color = Color.green;
		}
		else
		{
			this.OurLight.color = (this.evenColor ? Color.white : Color.yellow);
		}
		if (this.amClosing == Minigame.CloseState.None && !this.system.IsActive)
		{
			base.StartCoroutine(base.CoStartClose(0.75f));
		}
		if (this.system.IsConsoleOkay(this.OtherConsoleId))
		{
			this.TheirLight.color = Color.green;
			StringNames id = (this.OtherConsoleId == 1) ? StringNames.AuthRightOkay : StringNames.AuthLeftOkay;
			this.OtherStatusText.text = DestroyableSingleton<TranslationController>.Instance.GetString(id, Array.Empty<object>());
			return;
		}
		if (this.system.IsConsoleActive(this.OtherConsoleId))
		{
			this.TheirLight.color = (this.evenColor ? Color.white : Color.yellow);
			StringNames id2 = (this.OtherConsoleId == 1) ? StringNames.AuthRightActive : StringNames.AuthLeftActive;
			this.OtherStatusText.text = DestroyableSingleton<TranslationController>.Instance.GetString(id2, Array.Empty<object>());
			return;
		}
		this.TheirLight.color = Color.red;
		StringNames id3 = (this.OtherConsoleId == 1) ? StringNames.AuthRightNotActive : StringNames.AuthLeftNotActive;
		this.OtherStatusText.text = DestroyableSingleton<TranslationController>.Instance.GetString(id3, Array.Empty<object>());
	}

	public void ClickNumber(int i)
	{
		if (this.animating)
		{
			return;
		}
		if (this.NumberText.text.Length >= 5)
		{
			return;
		}
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.ButtonSound, false, 1f);
		}
		this.numString += i.ToString();
		this.number = this.number * 10 + i;
		this.NumberText.text = this.numString;
	}

	public void ClearEntry()
	{
		if (this.animating)
		{
			return;
		}
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.ButtonSound, false, 1f);
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
		if (this.system.TargetCode == this.number)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(this.AcceptSound, false, 1f);
			}
			ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 16 | base.ConsoleId);
			try
			{
				((SabotageTask)this.MyTask).MarkContributed();
			}
			catch
			{
			}
			this.NumberText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.OK, Array.Empty<object>());
			yield return wait;
			this.NumberText.text = string.Empty;
			yield return wait;
			this.NumberText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.OK, Array.Empty<object>());
			yield return wait;
			this.NumberText.text = string.Empty;
			yield return wait;
			this.NumberText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.OK, Array.Empty<object>());
		}
		else
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(this.RejectSound, false, 1f);
			}
			this.NumberText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Bad, Array.Empty<object>());
			yield return wait;
			this.NumberText.text = string.Empty;
			yield return wait;
			this.NumberText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Bad, Array.Empty<object>());
			yield return wait;
			this.numString = string.Empty;
			this.number = 0;
			this.NumberText.text = this.numString;
		}
		this.number = 0;
		this.numString = string.Empty;
		this.NumberText.text = string.Empty;
		this.animating = false;
		yield break;
	}
}
