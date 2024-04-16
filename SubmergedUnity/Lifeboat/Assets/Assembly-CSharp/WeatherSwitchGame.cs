using System;
using Rewired;
using UnityEngine;

public class WeatherSwitchGame : Minigame
{
	public static StringNames[] ControlNames = new StringNames[]
	{
		StringNames.NodeCA,
		StringNames.NodeTB,
		StringNames.NodeIRO,
		StringNames.NodePD,
		StringNames.NodeGI,
		StringNames.NodeMLG
	};

	public WeatherControl[] Controls;

	private WeatherNodeTask WeatherTask;

	public Transform buttonGlyph;

	public Vector3 buttonGlyphOffset_Off;

	public Vector3 buttonGlyphOffset_On;

	public AudioClip SwitchSound;

	public void Start()
	{
		for (int i = 0; i < this.Controls.Length; i++)
		{
			WeatherControl weatherControl = this.Controls[i];
			weatherControl.name = DestroyableSingleton<TranslationController>.Instance.GetString(WeatherSwitchGame.ControlNames[i], Array.Empty<object>());
			weatherControl.Label.text = DestroyableSingleton<TranslationController>.Instance.GetString(WeatherSwitchGame.ControlNames[i], Array.Empty<object>());
		}
	}

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		this.WeatherTask = (this.MyNormTask as WeatherNodeTask);
		this.Controls[this.WeatherTask.NodeId].SetInactive();
		base.SetupInput(true);
		Vector3 localPosition = this.Controls[this.WeatherTask.NodeId].transform.localPosition + this.buttonGlyphOffset_On;
		localPosition.z = this.buttonGlyph.transform.localPosition.z;
		this.buttonGlyph.transform.localPosition = localPosition;
	}

	private void Update()
	{
		if (ReInput.players.GetPlayer(0).GetButtonDown(20))
		{
			this.FlipSwitch(this.WeatherTask.NodeId);
		}
	}

	public void FlipSwitch(int i)
	{
		if (i == this.WeatherTask.NodeId)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(this.SwitchSound, false, 1f);
			}
			this.WeatherTask.NextStep();
			this.Controls[this.WeatherTask.NodeId].SetActive();
			base.StartCoroutine(base.CoStartClose(0.75f));
			Vector3 localPosition = this.Controls[this.WeatherTask.NodeId].transform.localPosition + this.buttonGlyphOffset_Off;
			localPosition.z = this.buttonGlyph.transform.localPosition.z;
			this.buttonGlyph.transform.localPosition = localPosition;
		}
	}
}
