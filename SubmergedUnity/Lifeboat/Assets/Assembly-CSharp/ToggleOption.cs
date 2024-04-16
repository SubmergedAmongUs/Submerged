using System;
using TMPro;
using UnityEngine;

public class ToggleOption : OptionBehaviour
{
	public TextMeshPro TitleText;

	public SpriteRenderer CheckMark;

	private bool oldValue;

	public void OnEnable()
	{
		this.TitleText.text = DestroyableSingleton<TranslationController>.Instance.GetString(this.Title, Array.Empty<object>());
		GameOptionsData gameOptions = PlayerControl.GameOptions;
		StringNames title = this.Title;
		if (title != StringNames.GameRecommendedSettings)
		{
			switch (title)
			{
			case StringNames.GameConfirmImpostor:
				this.CheckMark.enabled = gameOptions.ConfirmImpostor;
				return;
			case StringNames.GameVisualTasks:
				this.CheckMark.enabled = gameOptions.VisualTasks;
				return;
			case StringNames.GameAnonymousVotes:
				this.CheckMark.enabled = gameOptions.AnonymousVotes;
				return;
			}
			Debug.Log("Ono, unrecognized setting: " + this.Title.ToString());
			return;
		}
		this.CheckMark.enabled = gameOptions.isDefaults;
	}

	private void FixedUpdate()
	{
		bool @bool = this.GetBool();
		if (this.oldValue != @bool)
		{
			this.oldValue = @bool;
			this.CheckMark.enabled = @bool;
		}
	}

	public void Toggle()
	{
		this.CheckMark.enabled = !this.CheckMark.enabled;
		this.OnValueChanged(this);
	}

	public override bool GetBool()
	{
		return this.CheckMark.enabled;
	}
}
