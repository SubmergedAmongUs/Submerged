using System;
using InnerNet;
using TMPro;
using UnityEngine;

public class GameStartManager : DestroyableSingleton<GameStartManager>, IDisconnectHandler
{
	private const float CountdownDuration = 5.0001f;

	public int MinPlayers = 4;

	public TextMeshPro PlayerCounter;

	private int LastPlayerCount = -1;

	public GameObject GameSizePopup;

	public TextMeshPro GameRoomName;

	public LobbyBehaviour LobbyPrefab;

	public TextMeshPro GameStartText;

	public TextMeshPro startLabelText;

	public SpriteRenderer StartButton;

	public SpriteRenderer MakePublicButton;

	public Sprite PublicGameImage;

	public Sprite PrivateGameImage;

	public TextMeshPro privatePublicText;

	public SpriteRenderer ShareOnDiscordButton;

	public GameObject InviteFriendsButton;

	private GameStartManager.StartingStates startState;

	private float countDownTimer;

	private ImageTranslator publicButtonTranslator;

	public void Start()
	{
		if (DestroyableSingleton<TutorialManager>.InstanceExists)
		{
			 UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		string text = GameCode.IntToGameName(AmongUsClient.Instance.GameId);
		if (text != null)
		{
			this.GameRoomName.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.RoomCode, Array.Empty<object>()) + "\r\n" + text;
		}
		else
		{
			this.StartButton.transform.localPosition = new Vector3(0f, -0.2f, 0f);
			this.PlayerCounter.transform.localPosition = new Vector3(0f, -0.8f, 0f);
		}
		AmongUsClient.Instance.DisconnectHandlers.AddUnique(this);
		if (!AmongUsClient.Instance.AmHost)
		{
			this.StartButton.gameObject.SetActive(false);
			this.MakePublicButton.GetComponent<ControllerHeldButtonBehaviour>().enabled = false;
			ActionMapGlyphDisplay componentInChildren = this.MakePublicButton.GetComponentInChildren<ActionMapGlyphDisplay>(true);
			if (componentInChildren)
			{
				componentInChildren.gameObject.SetActive(false);
			}
		}
		else
		{
			LobbyBehaviour.Instance = UnityEngine.Object.Instantiate<LobbyBehaviour>(this.LobbyPrefab);
			AmongUsClient.Instance.Spawn(LobbyBehaviour.Instance, -2, SpawnFlags.None);
		}
		this.MakePublicButton.gameObject.SetActive(AmongUsClient.Instance.GameMode == GameModes.OnlineGame);
		this.ShareOnDiscordButton.gameObject.SetActive(false);
	}

	public void MakePublic()
	{
		if (AmongUsClient.Instance.AmHost)
		{
			AmongUsClient.Instance.ChangeGamePublic(!AmongUsClient.Instance.IsGamePublic);
		}
	}

	public void ShareGameInvite()
	{
		if (!DestroyableSingleton<DiscordManager>.InstanceExists)
		{
			return;
		}
		DestroyableSingleton<DiscordManager>.Instance.ShareGameOnDiscord();
	}

	public void Update()
	{
		if (!GameData.Instance)
		{
			return;
		}
		this.MakePublicButton.sprite = (AmongUsClient.Instance.IsGamePublic ? this.PublicGameImage : this.PrivateGameImage);
		this.privatePublicText.text = (AmongUsClient.Instance.IsGamePublic ? DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.PublicHeader, Array.Empty<object>()) : DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.PrivateHeader, Array.Empty<object>()));
		if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.C))
		{
			ClipboardHelper.PutClipboardString(GameCode.IntToGameName(AmongUsClient.Instance.GameId));
		}
		if (DestroyableSingleton<DiscordManager>.InstanceExists)
		{
			bool active = AmongUsClient.Instance.AmHost && AmongUsClient.Instance.GameMode == GameModes.OnlineGame && DestroyableSingleton<DiscordManager>.Instance.CanShareGameOnDiscord() && DestroyableSingleton<DiscordManager>.Instance.HasValidPartyID();
			this.ShareOnDiscordButton.gameObject.SetActive(active);
		}
		if (GameData.Instance.PlayerCount != this.LastPlayerCount)
		{
			this.LastPlayerCount = GameData.Instance.PlayerCount;
			string arg = "<color=#FF0000FF>";
			if (this.LastPlayerCount > this.MinPlayers)
			{
				arg = "<color=#00FF00FF>";
			}
			if (this.LastPlayerCount == this.MinPlayers)
			{
				arg = "<color=#FFFF00FF>";
			}
			if (AmongUsClient.Instance.GameMode == GameModes.LocalGame)
			{
				this.PlayerCounter.text = string.Format("{0}{1}/{2}", arg, this.LastPlayerCount, 15);
			}
			else
			{
				this.PlayerCounter.text = string.Format("{0}{1}/{2}", arg, this.LastPlayerCount, PlayerControl.GameOptions.MaxPlayers);
			}
			this.StartButton.color = ((this.LastPlayerCount >= this.MinPlayers) ? Palette.EnabledColor : Palette.DisabledClear);
			this.startLabelText.color = ((this.LastPlayerCount >= this.MinPlayers) ? Palette.EnabledColor : Palette.DisabledClear);
			if (DestroyableSingleton<DiscordManager>.InstanceExists)
			{
				if (AmongUsClient.Instance.AmHost && AmongUsClient.Instance.GameMode == GameModes.OnlineGame)
				{
					DestroyableSingleton<DiscordManager>.Instance.SetInLobbyHost(this.LastPlayerCount, PlayerControl.GameOptions.MaxPlayers, AmongUsClient.Instance.GameId);
				}
				else
				{
					DestroyableSingleton<DiscordManager>.Instance.SetInLobbyClient(this.LastPlayerCount, PlayerControl.GameOptions.MaxPlayers, AmongUsClient.Instance.GameId);
				}
			}
		}
		if (AmongUsClient.Instance.AmHost)
		{
			if (this.startState == GameStartManager.StartingStates.Countdown)
			{
				int num = Mathf.CeilToInt(this.countDownTimer);
				this.countDownTimer -= Time.deltaTime;
				int num2 = Mathf.CeilToInt(this.countDownTimer);
				this.GameStartText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameStarting, new object[]
				{
					num2
				});
				if (num != num2)
				{
					PlayerControl.LocalPlayer.RpcSetStartCounter(num2);
				}
				if (num2 <= 0)
				{
					this.FinallyBegin();
					return;
				}
			}
			else
			{
				this.GameStartText.text = string.Empty;
			}
		}
	}

	public void ResetStartState()
	{
		this.startState = GameStartManager.StartingStates.NotStarting;
		if (this.StartButton && this.StartButton.gameObject)
		{
			this.StartButton.gameObject.SetActive(AmongUsClient.Instance.AmHost);
		}
		if (PlayerControl.LocalPlayer)
		{
			PlayerControl.LocalPlayer.RpcSetStartCounter(-1);
		}
	}

	public void SetStartCounter(sbyte sec)
	{
		if (sec == -1)
		{
			this.GameStartText.text = string.Empty;
			return;
		}
		this.GameStartText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameStarting, new object[]
		{
			sec
		});
	}

	public void BeginGame()
	{
		if (this.startState != GameStartManager.StartingStates.NotStarting)
		{
			return;
		}
		if (SaveManager.ShowMinPlayerWarning && GameData.Instance.PlayerCount == this.MinPlayers)
		{
			this.GameSizePopup.SetActive(true);
			return;
		}
		if (GameData.Instance.PlayerCount < this.MinPlayers)
		{
			base.StartCoroutine(Effects.SwayX(this.PlayerCounter.transform, 0.75f, 0.25f));
			return;
		}
		this.ReallyBegin(false);
	}

	public void ReallyBegin(bool neverShow)
	{
		this.startState = GameStartManager.StartingStates.Countdown;
		this.GameSizePopup.SetActive(false);
		if (neverShow)
		{
			SaveManager.ShowMinPlayerWarning = false;
		}
		this.StartButton.gameObject.SetActive(false);
		this.countDownTimer = 5.0001f;
		this.startState = GameStartManager.StartingStates.Countdown;
	}

	public void FinallyBegin()
	{
		if (this.startState != GameStartManager.StartingStates.Countdown)
		{
			return;
		}
		this.startState = GameStartManager.StartingStates.Starting;
		AmongUsClient.Instance.StartGame();
		AmongUsClient.Instance.DisconnectHandlers.Remove(this);
		 UnityEngine.Object.Destroy(base.gameObject);
	}

	public void HandleDisconnect(PlayerControl pc, DisconnectReasons reason)
	{
		if (AmongUsClient.Instance.AmHost)
		{
			this.LastPlayerCount = -1;
			if (this.StartButton)
			{
				this.StartButton.gameObject.SetActive(true);
			}
		}
	}

	public void HandleDisconnect()
	{
		this.HandleDisconnect(null, DisconnectReasons.ExitGame);
	}

	public void ShowInviteMenu()
	{
	}

	private enum StartingStates
	{
		NotStarting,
		Countdown,
		Starting
	}
}
