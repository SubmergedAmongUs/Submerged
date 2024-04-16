using System;
using InnerNet;
using TMPro;
using UnityEngine;

public class ChatModeCycle : MonoBehaviour
{
	public CycleButtonBehaviour chatMode;

	public TextMeshPro chatModeText;

	public SpriteRenderer backgroundSprite;

	public void Awake()
	{
		DestroyableSingleton<AccountManager>.Instance.SetChatModeButtonForUpdates(this);
	}

	public void OnEnable()
	{
		this.UpdateDisplay();
	}

	public void UpdateDisplay()
	{
		this.chatMode.options = new StringNames[]
		{
			StringNames.FreeOrQuickChat,
			StringNames.QuickChatOnly
		};
		if (!DestroyableSingleton<EOSManager>.Instance.IsFreechatAllowed())
		{
			SaveManager.ChatModeType = QuickChatModes.QuickChatOnly;
			this.chatMode.Rollover.OverColor = Color.grey;
			this.chatMode.Rollover.OutColor = Color.grey;
			this.chatMode.Text.color = Color.grey;
			this.chatModeText.color = Color.grey;
			this.backgroundSprite.color = Color.grey;
		}
		else
		{
			this.chatMode.Rollover.OverColor = Color.green;
			this.chatMode.Rollover.OutColor = Color.white;
			this.chatMode.Text.color = Color.white;
			this.chatModeText.color = Color.white;
			this.backgroundSprite.color = Color.white;
		}
		this.chatMode.UpdateText(SaveManager.ChatModeType - QuickChatModes.FreeChatOrQuickChat);
	}

	public void CycleChatMode()
	{
		if (!DestroyableSingleton<EOSManager>.Instance.IsFreechatAllowed())
		{
			return;
		}
		int num = (int)(SaveManager.ChatModeType + 1);
		num %= 3;
		if (num == 0)
		{
			num++;
		}
		SaveManager.ChatModeType = (QuickChatModes)num;
		this.chatMode.UpdateText(SaveManager.ChatModeType - QuickChatModes.FreeChatOrQuickChat);
	}
}
