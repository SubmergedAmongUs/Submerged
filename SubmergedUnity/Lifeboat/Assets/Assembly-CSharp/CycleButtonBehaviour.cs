using System;
using TMPro;
using UnityEngine;

public class CycleButtonBehaviour : MonoBehaviour, ITranslatedText
{
	public StringNames[] options;

	public StringNames BaseText;

	public TextMeshPro Text;

	public SpriteRenderer Background;

	public ButtonRolloverHandler Rollover;

	private int curSelection;

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
		this.Text.text = DestroyableSingleton<TranslationController>.Instance.GetString(this.options[this.curSelection], Array.Empty<object>());
	}

	public void UpdateText(int selection)
	{
		this.curSelection = selection;
		this.ResetText();
		DestroyableSingleton<AccountManager>.Instance.UpdateVisuals();
	}
}
