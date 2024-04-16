using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LanguageSetter : MonoBehaviour
{
	public LanguageButton ButtonPrefab;

	public Scroller ButtonParent;

	public float ButtonStart = 0.5f;

	public float ButtonHeight = 0.5f;

	private LanguageButton[] AllButtons;

	public TextMeshPro parentLangButton;

	private List<UiElement> selectableButtons = new List<UiElement>();

	public UiElement backButton;

	private void OnEnable()
	{
		if (this.AllButtons != null)
		{
			LanguageButton languageButton = null;
			foreach (LanguageButton languageButton2 in this.AllButtons)
			{
				if (languageButton2.Title.color == Color.green)
				{
					languageButton = languageButton2;
				}
			}
			ControllerManager.Instance.OpenOverlayMenu(base.gameObject.name, this.backButton, languageButton.Button, this.selectableButtons, false);
		}
	}

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(base.gameObject.name);
	}

	public void Start()
	{
		TranslatedImageSet[] languages = DestroyableSingleton<TranslationController>.Instance.Languages;
		Collider2D component = this.ButtonParent.GetComponent<Collider2D>();
		bool flag = false;
		LanguageButton languageButton = null;
		if (this.AllButtons == null)
		{
			flag = true;
		}
		Vector3 localPosition = new Vector3(0f, this.ButtonStart, -0.5f);
		this.AllButtons = new LanguageButton[languages.Length];
		for (int i = 0; i < languages.Length; i++)
		{
			LanguageButton button = UnityEngine.Object.Instantiate<LanguageButton>(this.ButtonPrefab, this.ButtonParent.Inner);
			this.AllButtons[i] = button;
			button.Language = languages[i];
			button.Title.text = languages[i].Name;
			if ((long)i == (long)((ulong)SaveManager.LastLanguage))
			{
				languageButton = button;
				button.Title.color = Color.green;
				this.parentLangButton.text = languages[i].Name;
			}
			button.Button.OnClick.AddListener(delegate()
			{
				this.SetLanguage(button);
			});
			button.Button.ClickMask = component;
			button.transform.localPosition = localPosition;
			localPosition.y -= this.ButtonHeight;
		}
		if (flag)
		{
			foreach (LanguageButton languageButton2 in this.AllButtons)
			{
				this.selectableButtons.Add(languageButton2.Button);
			}
			ControllerManager.Instance.OpenOverlayMenu(base.gameObject.name, this.backButton, (languageButton != null) ? languageButton.Button : this.AllButtons[0].Button, this.selectableButtons, false);
		}
		this.ButtonParent.YBounds.max = (float)languages.Length * this.ButtonHeight - 2f * this.ButtonStart - 0.1f;
	}

	public void SetLanguage(LanguageButton selected)
	{
		for (int i = 0; i < this.AllButtons.Length; i++)
		{
			this.AllButtons[i].Title.color = Color.white;
		}
		selected.Title.color = Color.green;
		this.parentLangButton.text = selected.Language.Name;
		DestroyableSingleton<TranslationController>.Instance.SetLanguage(selected.Language);
		this.Close();
	}

	public void Open()
	{
		base.gameObject.SetActive(true);
	}

	public void Close()
	{
		base.gameObject.SetActive(false);
	}
}
