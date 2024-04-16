using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KeyValueOption : OptionBehaviour
{
	public TextMeshPro TitleText;

	public TextMeshPro ValueText;

	public List<KeyValuePair<string, int>> Values;

	private int Selected;

	private int oldValue = -1;

	public void OnEnable()
	{
		GameOptionsData gameOptions = PlayerControl.GameOptions;
		if (this.Title == StringNames.GameMapName)
		{
			this.Selected = (int)((gameOptions.MapId > 3) ? (gameOptions.MapId - 1) : gameOptions.MapId);
		}
		else
		{
			Debug.Log("Ono, unrecognized setting: " + this.Title.ToString());
		}
		this.TitleText.text = DestroyableSingleton<TranslationController>.Instance.GetString(this.Title, Array.Empty<object>());
		this.ValueText.text = this.Values[Mathf.Clamp(this.Selected, 0, this.Values.Count - 1)].Key;
	}

	private void FixedUpdate()
	{
		if (this.oldValue != this.Selected)
		{
			this.oldValue = this.Selected;
			this.ValueText.text = this.Values[this.Selected].Key;
		}
	}

	public void Increase()
	{
		this.Selected = Mathf.Clamp(this.Selected + 1, 0, this.Values.Count - 1);
		this.OnValueChanged(this);
	}

	public void Decrease()
	{
		this.Selected = Mathf.Clamp(this.Selected - 1, 0, this.Values.Count - 1);
		this.OnValueChanged(this);
	}

	public override int GetInt()
	{
		return this.Values[this.Selected].Value;
	}
}
