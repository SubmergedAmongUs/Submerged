using System;
using TMPro;
using UnityEngine;

public class ToggleButtonBehaviour : MonoBehaviour, ITranslatedText
{
	public StringNames BaseText;

	public TextMeshPro Text;

	public SpriteRenderer Background;

	public ButtonRolloverHandler Rollover;

	private bool onState;

	public void Start()
	{
		DestroyableSingleton<TranslationController>.Instance.ActiveTexts.Add(this);
	}

	public void OnDestroy()
	{
		if (DestroyableSingleton<TranslationController>.InstanceExists)
		{
			DestroyableSingleton<TranslationController>.Instance.ActiveTexts.Remove(this);
		}
	}

	public void ResetText()
	{
		this.Text.text = DestroyableSingleton<TranslationController>.Instance.GetString(this.BaseText, Array.Empty<object>()) + ": " + DestroyableSingleton<TranslationController>.Instance.GetString(this.onState ? StringNames.SettingsOn : StringNames.SettingsOff, Array.Empty<object>());
	}

	public void UpdateText(bool on)
	{
		this.onState = on;
		Color color = on ? new Color(0f, 1f, 0.16470589f, 1f) : Color.white;
		this.Background.color = color;
		this.ResetText();
		if (this.Rollover)
		{
			this.Rollover.ChangeOutColor(color);
		}
	}
}
