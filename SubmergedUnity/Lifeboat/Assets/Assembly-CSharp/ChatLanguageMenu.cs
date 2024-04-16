using System;
using System.Collections.Generic;
using UnityEngine;

public class ChatLanguageMenu : MonoBehaviour
{
	public CreateOptionsPicker Parent;

	public ObjectPoolBehavior ButtonPool;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	private UiElement defaultButtonSelected;

	private List<UiElement> controllerSelectable;

	private void Awake()
	{
		this.defaultButtonSelected = null;
		this.controllerSelectable = new List<UiElement>();
	}

	public void OnEnable()
	{
		uint keywords = (uint)this.Parent.GetTargetOptions().Keywords;
		int num = ChatLanguageSet.Instance.Languages.Count / 10;
		if (ChatLanguageSet.Instance.Languages.Count != 10)
		{
			num++;
		}
		float num2 = ((float)num / 2f - 0.5f) * -2.5f;
		this.controllerSelectable.Clear();
		int num3 = 0;
		foreach (KeyValuePair<string, uint> keyValuePair in ChatLanguageSet.Instance.Languages)
		{
			uint lang = keyValuePair.Value;
			ChatLanguageButton chatLanguageButton = this.ButtonPool.Get<ChatLanguageButton>();
			chatLanguageButton.transform.localPosition = new Vector3(num2 + (float)(num3 / 10) * 2.5f, 2f - (float)(num3 % 10) * 0.5f, 0f);
			if (keyValuePair.Key == "Other")
			{
				chatLanguageButton.Text.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.OtherLanguage, Array.Empty<object>());
			}
			else
			{
				chatLanguageButton.Text.text = keyValuePair.Key;
			}
			chatLanguageButton.Button.OnClick.RemoveAllListeners();
			chatLanguageButton.Button.OnClick.AddListener(delegate()
			{
				this.ChooseOption(lang);
			});
			chatLanguageButton.SetSelected(keyValuePair.Value == keywords);
			this.controllerSelectable.Add(chatLanguageButton.Button);
			if (keyValuePair.Value == keywords)
			{
				this.defaultButtonSelected = chatLanguageButton.Button;
			}
			num3++;
		}
		ControllerManager.Instance.OpenOverlayMenu(base.name, this.BackButton, this.defaultButtonSelected, this.controllerSelectable, false);
	}

	public void OnDisable()
	{
		this.ButtonPool.ReclaimAll();
		ControllerManager.Instance.CloseOverlayMenu(base.name);
	}

	public void Open()
	{
		base.gameObject.SetActive(true);
	}

	public void ChooseOption(uint language)
	{
		this.Parent.SetLanguageFilter(language);
		this.Close();
	}

	public void Close()
	{
		base.gameObject.SetActive(false);
	}
}
