using System;
using System.Collections.Generic;
using UnityEngine;

public class UseButtonManager : MonoBehaviour
{
	private static readonly Color DisabledColor = new Color(1f, 1f, 1f, 0.3f);

	private static readonly Color EnabledColor = new Color(1f, 1f, 1f, 1f);

	public List<UseButton> useButtons;

	private Dictionary<ImageNames, UseButton> otherButtons = new Dictionary<ImageNames, UseButton>();

	private UseButton currentButtonShown;

	private IUsable currentTarget;

	private void Start()
	{
		this.otherButtons.Clear();
		foreach (UseButton useButton in this.useButtons)
		{
			useButton.Hide();
			this.otherButtons.Add(useButton.imageName, useButton);
		}
	}

	public void SetTarget(IUsable target)
	{
		this.currentTarget = target;
		this.RefreshButtons();
		if (target != null)
		{
			if (target is Vent)
			{
				this.currentButtonShown = this.otherButtons[ImageNames.VentButton];
			}
			else if (target is OptionsConsole)
			{
				this.currentButtonShown = this.otherButtons[ImageNames.OptionsButton];
			}
			else
			{
				this.currentButtonShown = this.otherButtons[target.UseIcon];
				this.currentButtonShown.graphic.color = UseButtonManager.EnabledColor;
				this.currentButtonShown.text.color = UseButtonManager.EnabledColor;
			}
			this.currentButtonShown.Show(target.PercentCool);
			return;
		}
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		if (((localPlayer != null) ? localPlayer.Data : null) != null && PlayerControl.LocalPlayer.Data.IsImpostor && PlayerControl.LocalPlayer.CanMove)
		{
			this.currentButtonShown = this.otherButtons[ImageNames.SabotageButton];
			this.currentButtonShown.Show();
			return;
		}
		this.currentButtonShown = this.otherButtons[ImageNames.UseButton];
		this.currentButtonShown.Show();
		this.currentButtonShown.graphic.color = UseButtonManager.DisabledColor;
		this.currentButtonShown.text.color = UseButtonManager.DisabledColor;
	}

	public void DoClick()
	{
		if (!base.isActiveAndEnabled)
		{
			return;
		}
		if (!PlayerControl.LocalPlayer)
		{
			return;
		}
		GameData.PlayerInfo data = PlayerControl.LocalPlayer.Data;
		if (this.currentTarget != null)
		{
			PlayerControl.LocalPlayer.UseClosest();
			return;
		}
		if (data != null && data.IsImpostor)
		{
			DestroyableSingleton<HudManager>.Instance.ShowMap(delegate(MapBehaviour m)
			{
				m.ShowInfectedMap();
			});
		}
	}

	internal void Refresh()
	{
		this.SetTarget(this.currentTarget);
	}

	private void RefreshButtons()
	{
		if (this.currentButtonShown)
		{
			this.currentButtonShown.Hide();
		}
	}
}
