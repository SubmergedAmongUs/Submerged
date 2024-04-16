using System;
using TMPro;
using UnityEngine;

public class StringOption : OptionBehaviour
{
	public TextMeshPro TitleText;

	public TextMeshPro ValueText;

	public StringNames[] Values;

	public int Value;

	private int oldValue = -1;

	public void OnEnable()
	{
		GameOptionsData gameOptions = PlayerControl.GameOptions;
		StringNames title = this.Title;
		if (title != StringNames.GameKillDistance)
		{
			if (title != StringNames.GameTaskBarMode)
			{
				Debug.Log("Ono, unrecognized setting: " + this.Title.ToString());
			}
			else
			{
				this.Value = (int)gameOptions.TaskBarMode;
			}
		}
		else
		{
			this.Value = gameOptions.KillDistance;
		}
		this.TitleText.text = DestroyableSingleton<TranslationController>.Instance.GetString(this.Title, Array.Empty<object>());
		this.ValueText.text = DestroyableSingleton<TranslationController>.Instance.GetString(this.Values[this.Value], Array.Empty<object>());
	}

	private void FixedUpdate()
	{
		if (this.oldValue != this.Value)
		{
			this.oldValue = this.Value;
			this.ValueText.text = DestroyableSingleton<TranslationController>.Instance.GetString(this.Values[this.Value], Array.Empty<object>());
		}
	}

	public void Increase()
	{
		this.Value = Mathf.Clamp(this.Value + 1, 0, this.Values.Length - 1);
		this.OnValueChanged(this);
	}

	public void Decrease()
	{
		this.Value = Mathf.Clamp(this.Value - 1, 0, this.Values.Length - 1);
		this.OnValueChanged(this);
	}

	public override int GetInt()
	{
		return this.Value;
	}
}
