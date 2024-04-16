using System;
using TMPro;
using UnityEngine;

internal class ChatBubble : PoolableBehavior
{
	private static readonly Vector3 PlayerMessageScale = new Vector3(0.35f, 0.35f, 1f);

	private static readonly Vector3 PlayerNotificationScale = new Vector3(0.25f, 0.25f, 1f);

	public PoolablePlayer Player;

	public SpriteRenderer Xmark;

	public SpriteRenderer votedMark;

	public TextMeshPro NameText;

	public TextMeshPro TextArea;

	public SpriteRenderer Background;

	public SpriteRenderer MaskArea;

	public PlatformIdentifierIcon PlatformIcon;

	public void SetLeft()
	{
		base.transform.localPosition = new Vector3(-3f, 0f, 0f);
		this.Player.SetFlipX(false);
		this.Player.transform.localScale = ChatBubble.PlayerMessageScale;
		this.Player.transform.localPosition = new Vector3(0.04f, -0.08f, 0f);
		this.Xmark.transform.localPosition = new Vector3(-0.72f, 0.29f, -0.0001f);
		this.votedMark.transform.localPosition = new Vector3(-0.72f, 0.29f, -0.0001f);
		this.NameText.rectTransform.pivot = new Vector2(0f, 1f);
		this.NameText.transform.localPosition = new Vector3(0.5f, 0.358f, 0f);
		this.NameText.horizontalAlignment = HorizontalAlignmentOptions.Left;
		this.TextArea.rectTransform.pivot = new Vector2(0f, 1f);
		this.TextArea.transform.localPosition = new Vector3(0.5f, 0.125f, 0f);
		this.TextArea.horizontalAlignment = HorizontalAlignmentOptions.Left;
	}

	public void SetNotification()
	{
		base.transform.localPosition = new Vector3(-2.75f, 0f, 0f);
		this.Player.SetFlipX(false);
		this.Player.transform.localScale = ChatBubble.PlayerNotificationScale;
		this.Player.transform.localPosition = new Vector3(0f, 0.06f, 0f);
		this.Xmark.transform.localPosition = new Vector3(-0.72f, 0.29f, -0.0001f);
		this.votedMark.transform.localPosition = new Vector3(-0.72f, 0.29f, -0.0001f);
		this.NameText.rectTransform.pivot = new Vector2(0f, 1f);
		this.NameText.transform.localPosition = new Vector3(0.5f, 0.358f, 0f);
		this.NameText.horizontalAlignment = HorizontalAlignmentOptions.Left;
		this.TextArea.rectTransform.pivot = new Vector2(0f, 1f);
		this.TextArea.transform.localPosition = new Vector3(0.5f, 0.125f, 0f);
		this.TextArea.horizontalAlignment = HorizontalAlignmentOptions.Left;
		this.TextArea.text = string.Empty;
	}

	public void SetCosmetics(GameData.PlayerInfo playerInfo)
	{
		int num = 51 + this.PoolIndex;
		this.MaskArea.material.SetInt("_MaskLayer", num);
		this.Player.Body.material.SetInt("_MaskLayer", num);
		this.Player.Skin.SetMaskLayer(num);
		this.Player.HatSlot.SetMaskLayer(num);
		this.Background.material.SetInt("_MaskLayer", num);
		PlayerControl.SetPlayerMaterialColors(playerInfo.ColorId, this.Player.Body);
		this.Player.HatSlot.SetHat(playerInfo.HatId, playerInfo.ColorId);
		this.Player.SetSkin(playerInfo.SkinId);
	}

	public void SetRight()
	{
		base.transform.localPosition = new Vector3(-2.35f, 0f, 0f);
		this.Player.SetFlipX(true);
		this.Player.transform.localScale = ChatBubble.PlayerMessageScale;
		this.Player.transform.localPosition = new Vector3(4.75f, -0.08f, 0f);
		this.Xmark.transform.localPosition = new Vector3(0.72f, 0.29f, -0.0001f);
		this.votedMark.transform.localPosition = new Vector3(0.72f, 0.29f, -0.0001f);
		this.NameText.rectTransform.pivot = new Vector2(1f, 1f);
		this.NameText.transform.localPosition = new Vector3(4.35f, 0.358f, 0f);
		this.NameText.horizontalAlignment = HorizontalAlignmentOptions.Right;
		this.TextArea.rectTransform.pivot = new Vector2(1f, 1f);
		this.TextArea.transform.localPosition = new Vector3(4.35f, 0.125f, 0f);
		this.TextArea.horizontalAlignment = HorizontalAlignmentOptions.Right;
	}

	public void SetName(string playerName, bool isDead, bool voted, Color color)
	{
		this.NameText.text = (playerName ?? "...");
		this.NameText.color = color;
		this.NameText.ForceMeshUpdate(true, true);
		if (isDead)
		{
			this.Xmark.enabled = true;
			this.Background.color = Palette.HalfWhite;
		}
		if (voted)
		{
			this.votedMark.enabled = true;
		}
	}

	public override void Reset()
	{
		this.Xmark.enabled = false;
		this.votedMark.enabled = false;
		this.Background.color = Color.white;
	}

	public void AlignChildren()
	{
		Vector3 localPosition = this.Background.transform.localPosition;
		localPosition.y = this.NameText.transform.localPosition.y - this.Background.size.y / 2f + 0.05f;
		this.Background.transform.localPosition = localPosition;
		this.MaskArea.transform.localPosition = localPosition + new Vector3(0f, 0.02f, 0f);
	}

	internal void SetText(string chatText)
	{
		this.TextArea.text = chatText;
		this.TextArea.ForceMeshUpdate(true, true);
		this.Background.size = new Vector2(5.52f, 0.2f + this.NameText.GetNotDumbRenderedHeight() + this.TextArea.GetNotDumbRenderedHeight());
		this.MaskArea.size = this.Background.size - new Vector2(0f, 0.03f);
	}
}
