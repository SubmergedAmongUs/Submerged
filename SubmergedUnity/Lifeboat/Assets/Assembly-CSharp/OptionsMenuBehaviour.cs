using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

public class OptionsMenuBehaviour : MonoBehaviour, ITranslatedText
{
	public SpriteRenderer Background;

	public SpriteRenderer JoystickButton;

	public SpriteRenderer TouchButton;

	public SlideBar JoystickSizeSlider;

	public FloatRange JoystickSizes = new FloatRange(0.5f, 1.5f);

	public SlideBar SoundSlider;

	public SlideBar MusicSlider;

	public ToggleButtonBehaviour CensorChatButton;

	public bool Toggle = true;

	public TabGroup[] Tabs;

	private bool grabbedControllerButtons;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultButtonSelected;

	public List<UiElement> ControllerSelectable;

	public List<UiElement> IgnoreControllerSelection;

	private bool saveMusic;

	private bool saveSFX;

	private float musicVolume;

	private float sfxVolume;

	public bool IsOpen
	{
		get
		{
			return base.isActiveAndEnabled;
		}
	}

	public void OpenTabGroup(int index)
	{
		for (int i = 0; i < this.Tabs.Length; i++)
		{
			TabGroup tabGroup = this.Tabs[i];
			if (i == index)
			{
				tabGroup.Open();
			}
			else
			{
				tabGroup.Close();
			}
		}
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			this.Close();
		}
	}

	public void Start()
	{
		DestroyableSingleton<TranslationController>.Instance.ActiveTexts.Add(this);
		if (!this.grabbedControllerButtons)
		{
			this.grabbedControllerButtons = true;
			this.GrabControllerButtons();
		}
	}

	private void GrabControllerButtons()
	{
		this.ControllerSelectable.Clear();
		foreach (UiElement item in base.GetComponentsInChildren<UiElement>(true))
		{
			if (!this.IgnoreControllerSelection.Contains(item))
			{
				this.ControllerSelectable.Add(item);
			}
		}
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
		this.JoystickButton.transform.parent.GetComponentInChildren<TextMeshPro>().text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SettingsMouseMode, Array.Empty<object>());
		this.TouchButton.transform.parent.GetComponentInChildren<TextMeshPro>().text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SettingsKeyboardMode, Array.Empty<object>());
		this.JoystickSizeSlider.gameObject.SetActive(false);
	}

	public void Open()
	{
		this.ResetText();
		if (base.gameObject.activeSelf)
		{
			if (this.Toggle)
			{
				base.GetComponent<TransitionOpen>().Close();
			}
			return;
		}
		this.OpenTabGroup(0);
		this.UpdateButtons();
		base.gameObject.SetActive(true);
		if (DestroyableSingleton<HudManager>.InstanceExists)
		{
			ConsoleJoystick.SetMode_MenuAdditive();
		}
		if (!this.grabbedControllerButtons)
		{
			this.grabbedControllerButtons = true;
			this.GrabControllerButtons();
		}
		ControllerManager.Instance.OpenOverlayMenu("OptionsMenu", this.BackButton, this.DefaultButtonSelected, this.ControllerSelectable, true);
	}

	public void SetControlType(int i)
	{
		ControlTypes controlTypes = i + ControlTypes.ScreenJoystick;
		SaveManager.ControlMode = controlTypes;
		this.UpdateButtons();
		if (DestroyableSingleton<HudManager>.InstanceExists)
		{
			DestroyableSingleton<HudManager>.Instance.SetTouchType(controlTypes);
		}
	}

	public void UpdateJoystickSize()
	{
		SaveManager.JoystickSize = this.JoystickSizes.Lerp(this.JoystickSizeSlider.Value);
		if (DestroyableSingleton<HudManager>.InstanceExists)
		{
			DestroyableSingleton<HudManager>.Instance.SetJoystickSize(SaveManager.JoystickSize);
		}
	}

	public void UpdateSfxVolume()
	{
		SaveManager.SfxVolume = this.SoundSlider.Value;
		SoundManager.Instance.ChangeSfxVolume(this.SoundSlider.Value);
	}

	public void UpdateMusicVolume()
	{
		SaveManager.MusicVolume = this.MusicSlider.Value;
		SoundManager.Instance.ChangeMusicVolume(this.MusicSlider.Value);
	}

	public void OpenPrivacyPolicy()
	{
		Application.OpenURL("https://innersloth.com/privacy.php");
	}

	public void OpenTermsOfUse()
	{
		Application.OpenURL("https://innersloth.com/terms.php");
	}

	public void TogglePersonalizedAd()
	{
		this.Close();
		Object.FindObjectOfType<MainMenuManager>().AdsPolicy.ForceShow();
	}

	public void ToggleCensorChat()
	{
		SaveManager.CensorChat = !SaveManager.CensorChat;
		this.UpdateButtons();
	}

	public void UpdateButtons()
	{
		if (SaveManager.ControlMode - ControlTypes.ScreenJoystick == 0)
		{
			this.JoystickButton.color = Palette.AcceptedGreen;
			this.TouchButton.color = Color.white;
			this.JoystickSizeSlider.enabled = true;
			this.JoystickSizeSlider.OnEnable();
		}
		else
		{
			this.JoystickButton.color = Color.white;
			this.TouchButton.color = Palette.AcceptedGreen;
			this.JoystickSizeSlider.enabled = false;
			this.JoystickSizeSlider.OnDisable();
		}
		this.JoystickSizeSlider.Value = this.JoystickSizes.ReverseLerp(SaveManager.JoystickSize);
		if (!this.saveSFX)
		{
			this.SoundSlider.Value = SaveManager.SfxVolume;
		}
		else
		{
			this.SoundSlider.Value = this.sfxVolume;
		}
		if (!this.saveMusic)
		{
			this.MusicSlider.Value = SaveManager.MusicVolume;
		}
		else
		{
			this.MusicSlider.Value = this.musicVolume;
		}
		this.CensorChatButton.UpdateText(SaveManager.CensorChat);
	}

	public void Close()
	{
		base.gameObject.SetActive(false);
		ControllerManager.Instance.CloseOverlayMenu("OptionsMenu");
	}
}
