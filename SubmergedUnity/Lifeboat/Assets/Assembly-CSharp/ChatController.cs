using System;
using System.Collections;
using System.Collections.Generic;
using InnerNet;
using TMPro;
using UnityEngine;

public class ChatController : MonoBehaviour
{
	public ObjectPoolBehavior chatBubPool;

	public Transform TypingArea;

	public SpriteRenderer TextBubble;

	public TextBoxTMP TextArea;

	public TextMeshPro CharCount;

	public int MaxChat = 15;

	public Scroller scroller;

	public GameObject Content;

	public SpriteRenderer BackgroundImage;

	public SpriteRenderer ChatNotifyDot;

	public TextMeshPro SendRateMessage;

	public Vector3 SourcePos = new Vector3(0f, 0f, -10f);

	public Vector3 TargetPos = new Vector3(-0.35f, 0.02f, -10f);

	private const float MaxChatSendRate = 3f;

	private float TimeSinceLastMessage = 3f;

	public AudioClip MessageSound;

	private bool animating;

	private Coroutine notificationRoutine;

	public BanMenu BanButton;

	public QuickChatMenu quickChatMenu;

	public QuickChatNetData quickChatData;

	public GameObject OpenKeyboardButton;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultButtonSelected;

	public List<UiElement> ControllerSelectable;

	private SpecialInputHandler specialInputHandler;

	public bool IsOpen
	{
		get
		{
			return this.Content.activeInHierarchy;
		}
	}

	private void Awake()
	{
		this.specialInputHandler = base.GetComponentInChildren<SpecialInputHandler>(true);
	}

	private void OnEnable()
	{
		if (this.specialInputHandler != null)
		{
			this.specialInputHandler.disableVirtualCursor = true;
		}
	}

	public void Toggle()
	{
		CustomNetworkTransform customNetworkTransform = PlayerControl.LocalPlayer ? PlayerControl.LocalPlayer.NetTransform : null;
		if (this.animating || !customNetworkTransform)
		{
			return;
		}
		if (!string.IsNullOrEmpty(this.TextArea.text))
		{
			this.TextArea.SetText("", "");
			this.quickChatMenu.ResetGlyphs();
			return;
		}
		base.StopAllCoroutines();
		if (this.IsOpen)
		{
			base.StartCoroutine(this.CoClose());
			if (this.quickChatMenu.gameObject.activeSelf)
			{
				this.quickChatMenu.Toggle();
				return;
			}
		}
		else
		{
			this.Content.SetActive(true);
			customNetworkTransform.Halt();
			base.StartCoroutine(this.CoOpen());
		}
	}

	public void SetVisible(bool visible)
	{
		Debug.Log("Chat is hidden: " + visible.ToString());
		this.ForceClosed();
		base.gameObject.SetActive(visible);
	}

	public void ForceClosed()
	{
		base.StopAllCoroutines();
		this.Content.SetActive(false);
		this.BanButton.SetVisible(false);
		this.BanButton.Hide();
		this.animating = false;
		ConsoleJoystick.SetMode_Menu();
		ControllerManager.Instance.CloseOverlayMenu(base.name);
	}

	public IEnumerator CoOpen()
	{
		this.animating = true;
		Vector3 scale = Vector3.one;
		this.BanButton.Hide();
		this.BanButton.SetVisible(true);
		float targetScale = AspectSize.CalculateSize(base.transform.localPosition, this.BackgroundImage.sprite);
		float timer = 0f;
		while (timer < 0.15f)
		{
			timer += Time.deltaTime;
			float num = Mathf.SmoothStep(0f, 1f, timer / 0.15f);
			scale.y = (scale.x = Mathf.Lerp(0.1f, targetScale, num));
			this.Content.transform.localScale = scale;
			this.Content.transform.localPosition = Vector3.Lerp(this.SourcePos, this.TargetPos, num) * targetScale;
			this.BanButton.transform.localPosition = new Vector3(0f, -num * 0.75f, -20f);
			yield return null;
		}
		this.ChatNotifyDot.enabled = false;
		this.animating = false;
		ConsoleJoystick.SetMode_QuickChat();
		ControllerManager.Instance.OpenOverlayMenu(base.name, this.BackButton, this.DefaultButtonSelected, this.ControllerSelectable, false);
		if (Controller.currentTouchType != Controller.TouchType.Joystick)
		{
			this.GiveFocus();
		}
		this.quickChatMenu.ResetGlyphs();
		yield break;
	}

	public IEnumerator CoClose()
	{
		this.animating = true;
		this.BanButton.Hide();
		Vector3 scale = Vector3.one;
		float targetScale = AspectSize.CalculateSize(base.transform.localPosition, this.BackgroundImage.sprite);
		for (float timer = 0f; timer < 0.15f; timer += Time.deltaTime)
		{
			float num = 1f - Mathf.SmoothStep(0f, 1f, timer / 0.15f);
			scale.y = (scale.x = Mathf.Lerp(0.1f, targetScale, num));
			this.Content.transform.localScale = scale;
			this.Content.transform.localPosition = Vector3.Lerp(this.SourcePos, this.TargetPos, num) * targetScale;
			this.BanButton.transform.localPosition = new Vector3(0f, -num * 0.75f, -20f);
			yield return null;
		}
		this.BanButton.SetVisible(false);
		this.Content.SetActive(false);
		this.animating = false;
		ConsoleJoystick.SetMode_Menu();
		ControllerManager.Instance.CloseOverlayMenu(base.name);
		yield break;
	}

	public void SetPosition(MeetingHud meeting)
	{
		if (meeting)
		{
			base.transform.SetParent(meeting.transform);
			base.transform.localPosition = new Vector3(3.1f, 2.2f, -10f);
			return;
		}
		base.transform.SetParent(DestroyableSingleton<HudManager>.Instance.transform);
		base.GetComponent<AspectPosition>().AdjustPosition();
	}

	public void UpdateCharCount()
	{
		Vector2 size = this.TextBubble.size;
		size.y = Math.Max(0.62f, this.TextArea.TextHeight + 0.2f);
		this.TextBubble.size = size;
		Vector3 localPosition = this.TextBubble.transform.localPosition;
		localPosition.y = (0.62f - size.y) / 2f;
		this.TextBubble.transform.localPosition = localPosition;
		Vector3 localPosition2 = this.TypingArea.localPosition;
		localPosition2.y = -2.08f - localPosition.y * 2f;
		this.TypingArea.localPosition = localPosition2;
		int length = this.TextArea.text.Length;
		this.CharCount.text = string.Format("{0}/100", length);
		if (length < 75)
		{
			this.CharCount.color = Color.black;
		}
		else if (length < 100)
		{
			this.CharCount.color = new Color(1f, 1f, 0f, 1f);
		}
		else
		{
			this.CharCount.color = Color.red;
		}
		this.quickChatData.qcType = QuickChatNetType.None;
	}

	private void Update()
	{
		if (SaveManager.ChatModeType == QuickChatModes.QuickChatOnly)
		{
			this.OpenKeyboardButton.SetActive(false);
			this.TextArea.enabled = false;
		}
		this.TimeSinceLastMessage += Time.deltaTime;
		if (this.SendRateMessage.isActiveAndEnabled)
		{
			float num = 3f - this.TimeSinceLastMessage;
			if (num < 0f)
			{
				this.SendRateMessage.gameObject.SetActive(false);
				return;
			}
			this.SendRateMessage.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ChatRateLimit, new object[]
			{
				Mathf.CeilToInt(num)
			});
		}
	}

	public void SendChat()
	{
		float num = 3f - this.TimeSinceLastMessage;
		if (num > 0f)
		{
			this.SendRateMessage.gameObject.SetActive(true);
			this.SendRateMessage.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ChatRateLimit, new object[]
			{
				Mathf.CeilToInt(num)
			});
			return;
		}
		if (this.quickChatData.qcType != QuickChatNetType.None)
		{
			this.SendQuickChat();
		}
		else if (SaveManager.ChatModeType == QuickChatModes.FreeChatOrQuickChat)
		{
			this.SendFreeChat();
		}
		this.TimeSinceLastMessage = 0f;
		this.TextArea.Clear();
		this.quickChatMenu.ResetGlyphs();
		this.quickChatData.qcType = QuickChatNetType.None;
	}

	public void SendFreeChat()
	{
		PlayerControl.LocalPlayer.RpcSendChat(this.TextArea.text);
		Debug.Log("SendFreeChat has been called!");
	}

	public void SendQuickChat()
	{
		PlayerControl.LocalPlayer.RpcSendQuickChat(this.TextArea.text, this.quickChatData);
		Debug.Log("SendQuickChat has been called!");
	}

	public void AddChatNote(GameData.PlayerInfo srcPlayer, ChatNoteTypes noteType)
	{
		if (srcPlayer == null)
		{
			return;
		}
		if (this.chatBubPool.NotInUse == 0)
		{
			this.chatBubPool.ReclaimOldest();
		}
		ChatBubble chatBubble = this.chatBubPool.Get<ChatBubble>();
		chatBubble.SetCosmetics(srcPlayer);
		chatBubble.transform.SetParent(this.scroller.Inner);
		chatBubble.transform.localScale = Vector3.one;
		chatBubble.SetNotification();
		if (noteType == ChatNoteTypes.DidVote)
		{
			int votesRemaining = MeetingHud.Instance.GetVotesRemaining();
			chatBubble.SetName(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MeetingHasVoted, new object[]
			{
				srcPlayer.PlayerName,
				votesRemaining
			}), false, true, Color.green);
		}
		chatBubble.SetText(string.Empty);
		chatBubble.AlignChildren();
		this.AlignAllBubbles();
		if (!this.IsOpen && this.notificationRoutine == null)
		{
			this.notificationRoutine = base.StartCoroutine(this.BounceDot());
		}
		if (srcPlayer.Object != PlayerControl.LocalPlayer)
		{
			SoundManager.Instance.PlaySound(this.MessageSound, false, 1f).pitch = 0.5f + (float)srcPlayer.PlayerId / 15f;
		}
	}

	public void AddChat(PlayerControl sourcePlayer, string chatText)
	{
		if (!sourcePlayer || !PlayerControl.LocalPlayer)
		{
			return;
		}
		GameData.PlayerInfo data = PlayerControl.LocalPlayer.Data;
		GameData.PlayerInfo data2 = sourcePlayer.Data;
		if (data2 == null || data == null || (data2.IsDead && !data.IsDead))
		{
			return;
		}
		if (this.chatBubPool.NotInUse == 0)
		{
			this.chatBubPool.ReclaimOldest();
		}
		ChatBubble chatBubble = this.chatBubPool.Get<ChatBubble>();
		try
		{
			chatBubble.transform.SetParent(this.scroller.Inner);
			chatBubble.transform.localScale = Vector3.one;
			bool flag = sourcePlayer == PlayerControl.LocalPlayer;
			if (flag)
			{
				chatBubble.SetRight();
			}
			else
			{
				chatBubble.SetLeft();
			}
			bool flag2 = data.IsImpostor && data2.IsImpostor;
			bool voted = MeetingHud.Instance && MeetingHud.Instance.DidVote(sourcePlayer.PlayerId);
			chatBubble.SetCosmetics(data2);
			chatBubble.SetName(data2.PlayerName, data2.IsDead, voted, flag2 ? Palette.ImpostorRed : Color.white);
			ClientData client = AmongUsClient.Instance.GetClient(sourcePlayer.OwnerId);
			if (client != null && client.platformID != (RuntimePlatform) (-1))
			{
				chatBubble.PlatformIcon.SetIcon(client.platformID);
			}
			if (SaveManager.CensorChat)
			{
				chatText = BlockedWords.CensorWords(chatText);
			}
			chatBubble.SetText(chatText);
			chatBubble.AlignChildren();
			this.AlignAllBubbles();
			if (!this.IsOpen && this.notificationRoutine == null)
			{
				this.notificationRoutine = base.StartCoroutine(this.BounceDot());
			}
			if (!flag)
			{
				SoundManager.Instance.PlaySound(this.MessageSound, false, 1f).pitch = 0.5f + (float)sourcePlayer.PlayerId / 15f;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(ex);
			this.chatBubPool.Reclaim(chatBubble);
		}
	}

	private void AlignAllBubbles()
	{
		float num = 0f;
		List<PoolableBehavior> activeChildren = this.chatBubPool.activeChildren;
		for (int i = activeChildren.Count - 1; i >= 0; i--)
		{
			ChatBubble chatBubble = activeChildren[i] as ChatBubble;
			num += chatBubble.Background.size.y;
			Vector3 localPosition = chatBubble.transform.localPosition;
			localPosition.y = -1.85f + num;
			chatBubble.transform.localPosition = localPosition;
			num += 0.15f;
		}
		this.scroller.YBounds.min = Mathf.Min(0f, -num + this.scroller.Hitbox.bounds.size.y);
	}

	private IEnumerator BounceDot()
	{
		this.ChatNotifyDot.enabled = true;
		yield return Effects.Bounce(this.ChatNotifyDot.transform, 0.3f, 0.15f);
		this.notificationRoutine = null;
		yield break;
	}

	public void GiveFocus()
	{
		this.TextArea.GiveFocus();
	}
}
