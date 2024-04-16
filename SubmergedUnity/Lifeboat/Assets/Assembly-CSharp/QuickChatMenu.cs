using System;
using System.Collections.Generic;
using Rewired;
using TMPro;
using UnityEngine;

public class QuickChatMenu : MonoBehaviour
{
	public ChatController chatController;

	public SpriteRenderer playerHeadSprite;

	public TextBoxTMP targetTextBox;

	public ButtonBehavior sendMessageButton;

	public RadialMenu childRadialMenu;

	public Transform currentBlankArrow;

	public SpriteRenderer sendButtonGlyph;

	public SpriteRenderer quickChatGlyph;

	public QuickChatSubmenu topLevelMenu;

	public QuickChatSubmenu currentMenu;

	private QuickChatSubmenu stashedMenu;

	private bool showingAlternate;

	public GameObject alternateTooltipObject;

	public TextMeshPro alternateText;

	public MeshRenderer radialMenuRenderer;

	public QuickChatSubmenu.QuickChatColorSet defaultColorSet;

	private QuickChatMenuItem fillInBlankTarget;

	private bool fillInBlankIsAlternate;

	private bool fillInBlankMode;

	private bool updateFITBArrow;

	public List<string> fillInBlankEntries = new List<string>();

	public List<StringNames> fillInBlankEntryIDs = new List<StringNames>();

	private List<string> fillInBlankPreviewList = new List<string>();

	private List<byte> fillInBlankPlayerIDs = new List<byte>();

	private static string fitbCurrentBlank = "(-----)";

	private static string fitbBlank = "-------";

	private static int colorIndex = Shader.PropertyToID("_Color");

	private static int edgeColorIndex = Shader.PropertyToID("_EdgeColor");

	private float backspaceHeldTimer;

	private float backspaceRepeatTimer;

	private const float backspaceHoldUntilRepeatDelay = 0.6f;

	private const float backspaceRepeatDelay = 0.1f;

	private void Start()
	{
		if (this.topLevelMenu)
		{
			this.DisplayMenu(this.topLevelMenu, false);
		}
		if (this.playerHeadSprite)
		{
			PlayerControl.LocalPlayer.SetPlayerMaterialColors(this.playerHeadSprite);
		}
	}

	private void OnEnable()
	{
		this.targetTextBox.LoseFocus();
		this.targetTextBox.enabled = false;
	}

	private void OnDisable()
	{
		this.targetTextBox.enabled = true;
	}

	private void SetColors(QuickChatSubmenu.QuickChatColorSet colorSet)
	{
		this.radialMenuRenderer.material.SetColor(QuickChatMenu.colorIndex, colorSet.fillColor);
		this.radialMenuRenderer.material.SetColor(QuickChatMenu.edgeColorIndex, colorSet.edgeColor);
	}

	public void DisplayMenu(QuickChatSubmenu menu, bool alternate = false)
	{
		this.showingAlternate = alternate;
		this.currentMenu = menu;
		if (this.currentMenu.OnWillDisplay != null)
		{
			this.currentMenu.OnWillDisplay();
		}
		menu.UpdateActiveItems(alternate);
		RadialMenu.CachedButtonObject[] array = this.childRadialMenu.CreateButtonsForStrings(menu.GetMenuButtonStrings(alternate));
		this.childRadialMenu.prevSelectedButton = -1;
		this.alternateTooltipObject.SetActive(menu.hasAlternateSet);
		QuickChatSubmenu.QuickChatColorSet colors = menu.hasCustomColorSet ? menu.customColorSet : this.defaultColorSet;
		if (menu.hasAlternateSet)
		{
			this.alternateText.text = DestroyableSingleton<TranslationController>.Instance.GetString(alternate ? menu.primarySetName : menu.alternateSetName, Array.Empty<object>());
			if (alternate)
			{
				this.SetColors(menu.alternateColorSet);
			}
			else
			{
				this.SetColors(colors);
			}
		}
		else
		{
			this.SetColors(colors);
		}
		for (int i = 0; i < array.Length; i++)
		{
			array[i].button.OnClick = menu.activeMenuItems[i].OnClick;
			if (menu.activeMenuItems[i].icon)
			{
				array[i].AddIcon(menu.activeMenuItems[i].icon);
			}
		}
	}

	public void QuickChat(QuickChatMenuItem item)
	{
		if (this.fillInBlankMode)
		{
			this.fillInBlankEntries.Add(this.showingAlternate ? item.alternateText : item.text);
			this.fillInBlankPlayerIDs.Add(this.showingAlternate ? item.alternatePlayerID : item.playerID);
			StringNames stringNames = this.showingAlternate ? item.locStringAltKey : item.locStringKey;
			if (stringNames == StringNames.ExitButton)
			{
				stringNames = StringNames.ANY;
			}
			this.fillInBlankEntryIDs.Add(stringNames);
			QuickChatSubmenu[] array = this.fillInBlankIsAlternate ? this.fillInBlankTarget.alternateFillBlankSelectionsInOder : this.fillInBlankTarget.fillBlankSelectionsInOder;
			if (this.fillInBlankEntries.Count == array.Length)
			{
				StringNames stringNames2 = this.fillInBlankIsAlternate ? this.fillInBlankTarget.locStringAltKey : this.fillInBlankTarget.locStringKey;
				if (stringNames2 != StringNames.ExitButton)
				{
					string fitbvariant = DestroyableSingleton<TranslationController>.Instance.GetFITBVariant(stringNames2, this.fillInBlankEntryIDs);
					if (fitbvariant != null)
					{
						TextBoxTMP textBoxTMP = this.targetTextBox;
						string format = fitbvariant;
						object[] args = this.fillInBlankEntries.ToArray();
						textBoxTMP.SetText(string.Format(format, args), "");
						this.chatController.quickChatData.SetSentence(stringNames2, this.fillInBlankEntryIDs, this.fillInBlankPlayerIDs);
					}
					else
					{
						TextBoxTMP textBoxTMP2 = this.targetTextBox;
						string format2 = this.fillInBlankIsAlternate ? this.fillInBlankTarget.alternateFillInText : this.fillInBlankTarget.fillInText;
						object[] args = this.fillInBlankEntries.ToArray();
						textBoxTMP2.SetText(string.Format(format2, args), "");
						this.chatController.quickChatData.SetSentence(this.fillInBlankIsAlternate ? this.fillInBlankTarget.locStringAltKey : this.fillInBlankTarget.locStringKey, this.fillInBlankEntryIDs, this.fillInBlankPlayerIDs);
					}
				}
				else
				{
					TextBoxTMP textBoxTMP3 = this.targetTextBox;
					string format3 = this.fillInBlankIsAlternate ? this.fillInBlankTarget.alternateFillInText : this.fillInBlankTarget.fillInText;
					object[] args = this.fillInBlankEntries.ToArray();
					textBoxTMP3.SetText(string.Format(format3, args), "");
					this.chatController.quickChatData.SetSentence(this.fillInBlankIsAlternate ? this.fillInBlankTarget.locStringAltKey : this.fillInBlankTarget.locStringKey, this.fillInBlankEntryIDs, this.fillInBlankPlayerIDs);
				}
				this.fillInBlankMode = false;
				this.DisplayMenu(this.stashedMenu, this.fillInBlankIsAlternate);
				this.stashedMenu = null;
				this.Toggle();
			}
			else
			{
				this.DisplayMenu(array[this.fillInBlankEntries.Count], false);
			}
			this.UpdateFillInBlankPreview();
			return;
		}
		this.targetTextBox.AddText(this.showingAlternate ? item.alternateText : item.text);
		if (item.playerID != 255 || item.alternatePlayerID != 255)
		{
			this.chatController.quickChatData.SetName(this.showingAlternate ? item.alternatePlayerID : item.playerID);
		}
		else
		{
			this.chatController.quickChatData.SetPhrase(this.showingAlternate ? item.locStringAltKey : item.locStringKey);
		}
		this.Toggle();
	}

	public void QuickChat(string text)
	{
		this.targetTextBox.AddText(text);
	}

	public void BeginFillInBlank(QuickChatMenuItem item)
	{
		this.fillInBlankMode = true;
		this.fillInBlankEntries.Clear();
		this.fillInBlankEntryIDs.Clear();
		this.fillInBlankPlayerIDs.Clear();
		this.fillInBlankIsAlternate = this.showingAlternate;
		this.fillInBlankTarget = item;
		this.stashedMenu = this.currentMenu;
		this.DisplayMenu(this.showingAlternate ? item.alternateFillBlankSelectionsInOder[0] : item.fillBlankSelectionsInOder[0], false);
		this.UpdateFillInBlankPreview();
	}

	public void UpdateFillInBlankPreview()
	{
		this.currentBlankArrow.gameObject.SetActive(this.fillInBlankMode);
		if (this.fillInBlankMode)
		{
			this.updateFITBArrow = true;
			this.fillInBlankPreviewList.Clear();
			QuickChatSubmenu[] array = this.fillInBlankIsAlternate ? this.fillInBlankTarget.alternateFillBlankSelectionsInOder : this.fillInBlankTarget.fillBlankSelectionsInOder;
			foreach (string item in this.fillInBlankEntries)
			{
				this.fillInBlankPreviewList.Add(item);
			}
			if (this.fillInBlankPreviewList.Count < array.Length)
			{
				this.fillInBlankPreviewList.Add(QuickChatMenu.fitbCurrentBlank);
			}
			while (this.fillInBlankPreviewList.Count < array.Length)
			{
				this.fillInBlankPreviewList.Add(QuickChatMenu.fitbBlank);
			}
			string text = this.fillInBlankIsAlternate ? this.fillInBlankTarget.alternateFillInText : this.fillInBlankTarget.fillInText;
			TextBoxTMP textBoxTMP = this.targetTextBox;
			string format = text;
			object[] args = this.fillInBlankPreviewList.ToArray();
			textBoxTMP.SetText(string.Format(format, args), "");
		}
	}

	public void UpdateFITBArrow()
	{
		this.updateFITBArrow = false;
		Vector3 vector;
		Vector3 vector2;
		if (this.targetTextBox.outputText.GetWordPosition(QuickChatMenu.fitbCurrentBlank, out vector, out vector2))
		{
			vector = this.targetTextBox.outputText.transform.TransformPoint(vector);
			vector2 = this.targetTextBox.outputText.transform.TransformPoint(vector2);
			Vector3 position = vector2;
			position.x = (vector.x + vector2.x) * 0.5f;
			this.currentBlankArrow.gameObject.SetActive(true);
			this.currentBlankArrow.transform.position = position;
			return;
		}
		this.currentBlankArrow.gameObject.SetActive(false);
	}

	public void QuickChatButtonPressed()
	{
		if (ActiveInputManager.currentControlType != ActiveInputManager.InputType.Joystick || string.IsNullOrEmpty(this.targetTextBox.text))
		{
			this.Toggle();
			return;
		}
		this.chatController.SendChat();
	}

	public void ResetGlyphs()
	{
		this.sendButtonGlyph.enabled = false;
		this.quickChatGlyph.enabled = true;
	}

	public void Toggle()
	{
		bool flag = !base.gameObject.activeSelf;
		this.sendMessageButton.gameObject.SetActive(!flag);
		base.gameObject.SetActive(flag);
		this.currentBlankArrow.gameObject.SetActive(false);
		if (flag)
		{
			this.targetTextBox.SetText("", "");
			if (this.currentMenu != this.topLevelMenu)
			{
				this.DisplayMenu(this.topLevelMenu, false);
				return;
			}
		}
		else if (!string.IsNullOrEmpty(this.targetTextBox.text) && !this.sendButtonGlyph.enabled)
		{
			this.sendButtonGlyph.enabled = true;
			this.quickChatGlyph.enabled = false;
		}
	}

	private void HandleBackspace()
	{
		if (this.backspaceHeldTimer == 0f)
		{
			this.targetTextBox.ClearLastWord();
		}
		else if (this.backspaceHeldTimer > 0.6f)
		{
			if (this.backspaceRepeatTimer >= 0.1f)
			{
				this.backspaceRepeatTimer -= 0.1f;
				this.targetTextBox.ClearLastWord();
			}
			this.backspaceRepeatTimer += Time.deltaTime;
		}
		this.backspaceHeldTimer += Time.deltaTime;
	}

	public void BackPressed()
	{
		if (!this.fillInBlankMode)
		{
			if (this.currentMenu.parentMenu)
			{
				this.DisplayMenu(this.currentMenu.parentMenu, false);
				return;
			}
			if (this.currentMenu == this.topLevelMenu)
			{
				this.Toggle();
				return;
			}
		}
		else
		{
			if (this.fillInBlankEntries.Count == 0)
			{
				this.targetTextBox.SetText("", "");
				this.fillInBlankMode = false;
				this.DisplayMenu(this.stashedMenu, this.fillInBlankIsAlternate);
			}
			else
			{
				QuickChatSubmenu menu = this.fillInBlankIsAlternate ? this.fillInBlankTarget.alternateFillBlankSelectionsInOder[this.fillInBlankEntries.Count - 1] : this.fillInBlankTarget.fillBlankSelectionsInOder[this.fillInBlankEntries.Count - 1];
				this.fillInBlankEntries.RemoveAt(this.fillInBlankEntries.Count - 1);
				this.fillInBlankEntryIDs.RemoveAt(this.fillInBlankEntryIDs.Count - 1);
				this.fillInBlankPlayerIDs.RemoveAt(this.fillInBlankPlayerIDs.Count - 1);
				this.DisplayMenu(menu, false);
			}
			this.UpdateFillInBlankPreview();
		}
	}

	public void ToggleAlternateMenu()
	{
		if (this.currentMenu != null && this.currentMenu.hasAlternateSet)
		{
			this.DisplayMenu(this.currentMenu, !this.showingAlternate);
		}
	}

	private void Update()
	{
		Player player = ReInput.players.GetPlayer(0);
		if (this.updateFITBArrow)
		{
			this.UpdateFITBArrow();
		}
		if (player.GetButtonDown(32) || Input.GetMouseButtonDown(2))
		{
			this.ToggleAlternateMenu();
		}
		if (this.currentMenu.allowBackspace && player.GetButton(29))
		{
			this.HandleBackspace();
		}
		else
		{
			this.backspaceHeldTimer = 0f;
			this.backspaceRepeatTimer = 0.1f;
		}
		if (player.GetButtonDown(12) || Input.GetMouseButtonDown(1))
		{
			this.BackPressed();
		}
	}
}
