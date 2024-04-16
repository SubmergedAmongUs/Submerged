using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using InnerNet;
using TMPro;
using UnityEngine;

public class MeetingHud : InnerNetObject, IDisconnectHandler
{
	private const int NumColumns = 3;

	private const float ResultsTime = 5f;

	private const float Depth = 0f;

	public static MeetingHud Instance;

	public SpriteRenderer[] PlayerColoredParts;

	public MeetingIntroAnimation MeetingIntro;

	public Transform ButtonParent;

	public TextMeshPro TitleText;

	public Vector3 VoteOrigin = new Vector3(-3.6f, 1.75f);

	public Vector3 VoteButtonOffsets = new Vector2(3.6f, -0.91f);

	public PlayerVoteArea SkipVoteButton;

	private PlayerVoteArea[] playerStates = new PlayerVoteArea[0];

	public PlayerVoteArea PlayerButtonPrefab;

	public SpriteRenderer PlayerVotePrefab;

	public Sprite CrackedGlass;

	public SpriteRenderer Glass;

	public PassiveButton ProceedButton;

	public AudioClip VoteSound;

	public AudioClip VoteLockinSound;

	public AudioClip VoteEndingSound;

	private MeetingHud.VoteStates state;

	public GameObject SkippedVoting;

	public SpriteRenderer HostIcon;

	private GameData.PlayerInfo exiledPlayer;

	private bool wasTie;

	public TextMeshPro TimerText;

	public float discussionTimer;

	private byte reporterId;

	private bool amDead;

	private float resultsStartedAt;

	private int lastSecond = 10;

	[Header("Console Controller Navigation")]
	public UiElement DefaultButtonSelected;

	public UiElement ProceedButtonUi;

	public List<UiElement> ControllerSelectable;

	public void RpcClose()
	{
		if (AmongUsClient.Instance.AmClient)
		{
			this.Close();
		}
		AmongUsClient.Instance.SendRpc(this.NetId, 22, SendOption.Reliable);
	}

	public void CmdCastVote(byte playerId, byte suspectIdx)
	{
		if (AmongUsClient.Instance.AmHost)
		{
			this.CastVote(playerId, suspectIdx);
			return;
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(this.NetId, 24, SendOption.Reliable, AmongUsClient.Instance.HostId);
		messageWriter.Write(playerId);
		messageWriter.Write(suspectIdx);
		AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
	}

	private void RpcVotingComplete(MeetingHud.VoterState[] states, GameData.PlayerInfo exiled, bool tie)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			this.VotingComplete(states, exiled, tie);
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(this.NetId, 23, SendOption.Reliable);
		messageWriter.WritePacked(states.Length);
		foreach (MeetingHud.VoterState voterState in states)
		{
			voterState.Serialize(messageWriter);
		}
		messageWriter.Write((exiled != null) ? exiled.PlayerId : byte.MaxValue);
		messageWriter.Write(tie);
		messageWriter.EndMessage();
	}

	private void RpcClearVote(int clientId)
	{
		if (AmongUsClient.Instance.ClientId == clientId)
		{
			this.ClearVote();
			return;
		}
		MessageWriter msg = AmongUsClient.Instance.StartRpcImmediately(this.NetId, 25, SendOption.Reliable, clientId);
		AmongUsClient.Instance.FinishRpcImmediately(msg);
	}

	public override bool Serialize(MessageWriter writer, bool initialState)
	{
		writer.WritePacked(this.playerStates.Length);
		for (int i = 0; i < this.playerStates.Length; i++)
		{
			PlayerVoteArea playerVoteArea = this.playerStates[i];
			writer.StartMessage(playerVoteArea.TargetPlayerId);
			this.playerStates[i].Serialize(writer);
			writer.EndMessage();
		}
		base.ClearDirtyBits();
		return true;
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
		if (initialState)
		{
			this.PopulateButtons(0);
		}
		int num = reader.ReadPackedInt32();
		for (int i = 0; i < num; i++)
		{
			MessageReader msg = reader.ReadMessage();
			PlayerVoteArea playerVoteArea = this.playerStates.FirstOrDefault((PlayerVoteArea ps) => ps.TargetPlayerId == msg.Tag);
			if (playerVoteArea)
			{
				playerVoteArea.Deserialize(msg);
				if (playerVoteArea.DidReport)
				{
					this.reporterId = playerVoteArea.TargetPlayerId;
				}
			}
		}
	}

	public override void HandleRpc(byte callId, MessageReader reader)
	{
		switch (callId)
		{
		case 22:
			this.Close();
			return;
		case 23:
		{
			MeetingHud.VoterState[] array = new MeetingHud.VoterState[reader.ReadPackedInt32()];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = MeetingHud.VoterState.Deserialize(reader);
			}
			GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(reader.ReadByte());
			bool tie = reader.ReadBoolean();
			this.VotingComplete(array, playerById, tie);
			return;
		}
		case 24:
		{
			byte srcPlayerId = reader.ReadByte();
			byte suspectPlayerId = reader.ReadByte();
			this.CastVote(srcPlayerId, suspectPlayerId);
			return;
		}
		case 25:
			this.ClearVote();
			return;
		default:
			return;
		}
	}

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(base.name);
	}

	private void Awake()
	{
		if (!MeetingHud.Instance)
		{
			MeetingHud.Instance = this;
			return;
		}
		if (MeetingHud.Instance != this)
		{
			 UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		foreach (SpriteRenderer playerMaterialColors in this.PlayerColoredParts)
		{
			PlayerControl.LocalPlayer.SetPlayerMaterialColors(playerMaterialColors);
		}
		DestroyableSingleton<HudManager>.Instance.Chat.gameObject.SetActive(true);
		DestroyableSingleton<HudManager>.Instance.Chat.SetPosition(this);
		DestroyableSingleton<HudManager>.Instance.StopOxyFlash();
		DestroyableSingleton<HudManager>.Instance.StopReactorFlash();
		this.SkipVoteButton.SetTargetPlayerId(253);
		this.SkipVoteButton.Parent = this;
		Camera.main.GetComponent<FollowerCamera>().Locked = true;
		if (PlayerControl.LocalPlayer.Data.IsDead)
		{
			this.SetForegroundForDead();
		}
		AmongUsClient.Instance.DisconnectHandlers.AddUnique(this);
		foreach (PlayerVoteArea playerVoteArea in this.playerStates)
		{
			this.ControllerSelectable.Add(playerVoteArea.PlayerButton);
		}
	}

	public override void OnDestroy()
	{
		ControllerManager.Instance.CloseOverlayMenu(base.name);
		base.OnDestroy();
	}

	private void SetForegroundForDead()
	{
		this.amDead = true;
		this.SkipVoteButton.gameObject.SetActive(false);
		this.Glass.sprite = this.CrackedGlass;
		this.Glass.color = Color.white;
	}

	public void Update()
	{
		if (this.state == MeetingHud.VoteStates.Animating)
		{
			return;
		}
		this.discussionTimer += Time.deltaTime;
		this.UpdateButtons();
		switch (this.state)
		{
		case MeetingHud.VoteStates.Discussion:
		{
			if (this.discussionTimer < (float)PlayerControl.GameOptions.DiscussionTime)
			{
				float num = (float)PlayerControl.GameOptions.DiscussionTime - this.discussionTimer;
				this.TimerText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MeetingVotingBegins, new object[]
				{
					Mathf.CeilToInt(num)
				});
				for (int i = 0; i < this.playerStates.Length; i++)
				{
					this.playerStates[i].SetDisabled();
				}
				this.SkipVoteButton.SetDisabled();
				return;
			}
			this.state = MeetingHud.VoteStates.NotVoted;
			bool active = PlayerControl.GameOptions.VotingTime > 0;
			this.TimerText.gameObject.SetActive(active);
			for (int j = 0; j < this.playerStates.Length; j++)
			{
				this.playerStates[j].SetEnabled();
			}
			this.SkipVoteButton.SetEnabled();
			return;
		}
		case MeetingHud.VoteStates.NotVoted:
		case MeetingHud.VoteStates.Voted:
			if (PlayerControl.GameOptions.VotingTime > 0)
			{
				float num2 = this.discussionTimer - (float)PlayerControl.GameOptions.DiscussionTime;
				float num3 = Mathf.Max(0f, (float)PlayerControl.GameOptions.VotingTime - num2);
				this.TimerText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MeetingVotingEnds, new object[]
				{
					Mathf.CeilToInt(num3)
				});
				if (this.state == MeetingHud.VoteStates.NotVoted && Mathf.CeilToInt(num3) <= this.lastSecond)
				{
					this.lastSecond--;
					base.StartCoroutine(Effects.PulseColor(this.TimerText, Color.red, Color.white, 0.25f));
					SoundManager.Instance.PlaySound(this.VoteEndingSound, false, 1f).pitch = Mathf.Lerp(1.5f, 0.8f, (float)this.lastSecond / 10f);
				}
				if (AmongUsClient.Instance.AmHost && num2 >= (float)PlayerControl.GameOptions.VotingTime)
				{
					this.ForceSkipAll();
					return;
				}
			}
			break;
		case MeetingHud.VoteStates.Results:
			if (AmongUsClient.Instance.GameMode == GameModes.OnlineGame)
			{
				float num4 = this.discussionTimer - this.resultsStartedAt;
				float num5 = Mathf.Max(0f, 5f - num4);
				this.TimerText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MeetingProceeds, new object[]
				{
					Mathf.CeilToInt(num5)
				});
				if (AmongUsClient.Instance.AmHost && num5 <= 0f)
				{
					this.HandleProceed();
				}
			}
			break;
		default:
			return;
		}
	}

	public IEnumerator CoIntro(GameData.PlayerInfo reporter, GameData.PlayerInfo reportedBody, GameData.PlayerInfo[] deadBodies)
	{
		this.SkipVoteButton.SetDisabled();
		base.transform.SetParent(DestroyableSingleton<HudManager>.Instance.transform);
		base.transform.localPosition = new Vector3(0f, -10f, 0f);
		DestroyableSingleton<HudManager>.Instance.Chat.ForceClosed();
		DestroyableSingleton<HudManager>.Instance.SetHudActive(false);
		bool flag = reportedBody == null;
		MeetingCalledAnimation prefab = flag ? ShipStatus.Instance.EmergencyOverlay : ShipStatus.Instance.ReportOverlay;
		GameData.PlayerInfo playerInfo = flag ? reporter : reportedBody;
		DestroyableSingleton<HudManager>.Instance.KillOverlay.ShowMeeting(prefab, playerInfo);
		yield return DestroyableSingleton<HudManager>.Instance.KillOverlay.WaitForFinish();
		this.MeetingIntro.Init(reporter, deadBodies);
		yield return Effects.Slide2D(base.transform, new Vector2(0f, -10f), new Vector2(0f, 0f), 0.25f);
		yield return Effects.Wait(0.5f);
		yield return this.MeetingIntro.CoRun();
		this.TitleText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MeetingWhoIsTitle, Array.Empty<object>());
		this.state = MeetingHud.VoteStates.Discussion;
		ControllerManager.Instance.OpenOverlayMenu(base.name, null, this.DefaultButtonSelected, this.ControllerSelectable, false);
		yield break;
	}

	private IEnumerator CoStartCutscene()
	{
		ConsoleJoystick.SetMode_Task();
		yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.clear, Color.black, 1f);
		ExileController exileController = UnityEngine.Object.Instantiate<ExileController>(ShipStatus.Instance.ExileCutscenePrefab);
		exileController.transform.SetParent(DestroyableSingleton<HudManager>.Instance.transform, false);
		exileController.transform.localPosition = new Vector3(0f, 0f, -60f);
		exileController.Begin(this.exiledPlayer, this.wasTie);
		this.DespawnOnDestroy = false;
		 UnityEngine.Object.Destroy(base.gameObject);
		yield break;
	}

	public void ServerStart(byte reporter)
	{
		this.reporterId = reporter;
		this.PopulateButtons(reporter);
	}

	public void Close()
	{
		GameData.PlayerInfo data = PlayerControl.LocalPlayer.Data;
		DestroyableSingleton<HudManager>.Instance.Chat.SetPosition(null);
		DestroyableSingleton<HudManager>.Instance.Chat.SetVisible(data.IsDead);
		DestroyableSingleton<HudManager>.Instance.Chat.BanButton.Hide();
		base.StartCoroutine(this.CoStartCutscene());
	}

	private void VotingComplete(MeetingHud.VoterState[] states, GameData.PlayerInfo exiled, bool tie)
	{
		if (this.state == MeetingHud.VoteStates.Results)
		{
			return;
		}
		this.state = MeetingHud.VoteStates.Results;
		this.resultsStartedAt = this.discussionTimer;
		this.exiledPlayer = exiled;
		this.wasTie = tie;
		this.SkipVoteButton.gameObject.SetActive(false);
		this.SkippedVoting.gameObject.SetActive(true);
		AmongUsClient.Instance.DisconnectHandlers.Remove(this);
		this.PopulateResults(states);
		this.SetupProceedButton();
		try
		{
			MeetingHud.VoterState voterState = states.FirstOrDefault((MeetingHud.VoterState s) => s.VoterId == PlayerControl.LocalPlayer.PlayerId);
			GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(voterState.VotedForId);
			DestroyableSingleton<AchievementManager>.Instance.OnMeetingVote(PlayerControl.LocalPlayer.Data, playerById);
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
		}
		if (DestroyableSingleton<HudManager>.Instance.Chat.IsOpen)
		{
			DestroyableSingleton<HudManager>.Instance.Chat.ForceClosed();
			ControllerManager.Instance.CloseOverlayMenu(DestroyableSingleton<HudManager>.Instance.Chat.name);
		}
		ControllerManager.Instance.CloseOverlayMenu(base.name);
		ControllerManager.Instance.OpenOverlayMenu(base.name, null, this.ProceedButtonUi);
	}

	public bool Select(int suspectStateIdx)
	{
		if (this.discussionTimer < (float)PlayerControl.GameOptions.DiscussionTime)
		{
			return false;
		}
		if (PlayerControl.LocalPlayer.Data.IsDead)
		{
			return false;
		}
		SoundManager.Instance.PlaySound(this.VoteSound, false, 1f).volume = 0.8f;
		for (int i = 0; i < this.playerStates.Length; i++)
		{
			PlayerVoteArea playerVoteArea = this.playerStates[i];
			if (suspectStateIdx != (int)playerVoteArea.TargetPlayerId)
			{
				playerVoteArea.ClearButtons();
			}
		}
		if (suspectStateIdx != -1)
		{
			this.SkipVoteButton.ClearButtons();
		}
		return true;
	}

	public void Confirm(byte suspectStateIdx)
	{
		if (PlayerControl.LocalPlayer.Data.IsDead)
		{
			return;
		}
		for (int i = 0; i < this.playerStates.Length; i++)
		{
			PlayerVoteArea playerVoteArea = this.playerStates[i];
			playerVoteArea.ClearButtons();
			playerVoteArea.voteComplete = true;
		}
		this.SkipVoteButton.ClearButtons();
		this.SkipVoteButton.voteComplete = true;
		this.SkipVoteButton.gameObject.SetActive(false);
		MeetingHud.VoteStates voteStates = this.state;
		if (voteStates != MeetingHud.VoteStates.NotVoted)
		{
			return;
		}
		this.state = MeetingHud.VoteStates.Voted;
		this.CmdCastVote(PlayerControl.LocalPlayer.PlayerId, suspectStateIdx);
	}

	public void HandleDisconnect(PlayerControl pc, DisconnectReasons reason)
	{
		if (!AmongUsClient.Instance.AmHost)
		{
			return;
		}
		if (this.playerStates == null)
		{
			return;
		}
		if (!pc)
		{
			return;
		}
		if (!GameData.Instance)
		{
			return;
		}
		int num = this.playerStates.IndexOf((PlayerVoteArea pv) => pv.TargetPlayerId == pc.PlayerId);
		PlayerVoteArea playerVoteArea = this.playerStates[num];
		playerVoteArea.AmDead = true;
		playerVoteArea.Overlay.gameObject.SetActive(true);
		for (int i = 0; i < this.playerStates.Length; i++)
		{
			PlayerVoteArea playerVoteArea2 = this.playerStates[i];
			if (!playerVoteArea2.AmDead && playerVoteArea2.DidVote && playerVoteArea2.VotedFor == pc.PlayerId)
			{
				playerVoteArea2.UnsetVote();
				base.SetDirtyBit(1U);
				GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(playerVoteArea2.TargetPlayerId);
				if (playerById != null)
				{
					int clientIdFromCharacter = AmongUsClient.Instance.GetClientIdFromCharacter(playerById.Object);
					if (clientIdFromCharacter != -1)
					{
						this.RpcClearVote(clientIdFromCharacter);
					}
				}
			}
		}
		base.SetDirtyBit(1U);
		this.CheckForEndVoting();
		if (this.state == MeetingHud.VoteStates.Results)
		{
			this.SetupProceedButton();
		}
	}

	public void HandleDisconnect()
	{
	}

	private void ForceSkipAll()
	{
		for (int i = 0; i < this.playerStates.Length; i++)
		{
			PlayerVoteArea playerVoteArea = this.playerStates[i];
			if (!playerVoteArea.DidVote)
			{
				playerVoteArea.VotedFor = 254;
				base.SetDirtyBit(1U);
			}
		}
		this.CheckForEndVoting();
	}

	public void CastVote(byte srcPlayerId, byte suspectPlayerId)
	{
		int num = this.playerStates.IndexOf((PlayerVoteArea pv) => pv.TargetPlayerId == srcPlayerId);
		PlayerVoteArea playerVoteArea = this.playerStates[num];
		if (!playerVoteArea.AmDead && !playerVoteArea.DidVote)
		{
			if (PlayerControl.LocalPlayer.PlayerId == srcPlayerId || AmongUsClient.Instance.GameMode != GameModes.LocalGame)
			{
				SoundManager.Instance.PlaySound(this.VoteLockinSound, false, 1f);
			}
			playerVoteArea.SetVote(suspectPlayerId);
			base.SetDirtyBit(1U);
			this.CheckForEndVoting();
			PlayerControl.LocalPlayer.RpcSendChatNote(srcPlayerId, ChatNoteTypes.DidVote);
		}
	}

	public void ClearVote()
	{
		for (int i = 0; i < this.playerStates.Length; i++)
		{
			this.playerStates[i].voteComplete = false;
		}
		this.SkipVoteButton.voteComplete = false;
		this.SkipVoteButton.gameObject.SetActive(true);
		this.state = MeetingHud.VoteStates.NotVoted;
	}

	private void CheckForEndVoting()
	{
		if (this.playerStates.All((PlayerVoteArea ps) => ps.AmDead || ps.DidVote))
		{
			Dictionary<byte, int> self = this.CalculateVotes();
			bool tie;
			KeyValuePair<byte, int> max = self.MaxPair(out tie);
			GameData.PlayerInfo exiled = GameData.Instance.AllPlayers.FirstOrDefault((GameData.PlayerInfo v) => !tie && v.PlayerId == max.Key);
			MeetingHud.VoterState[] array = new MeetingHud.VoterState[this.playerStates.Length];
			for (int i = 0; i < this.playerStates.Length; i++)
			{
				PlayerVoteArea playerVoteArea = this.playerStates[i];
				array[i] = new MeetingHud.VoterState
				{
					VoterId = playerVoteArea.TargetPlayerId,
					VotedForId = playerVoteArea.VotedFor
				};
			}
			this.RpcVotingComplete(array, exiled, tie);
		}
	}

	private Dictionary<byte, int> CalculateVotes()
	{
		Dictionary<byte, int> dictionary = new Dictionary<byte, int>();
		for (int i = 0; i < this.playerStates.Length; i++)
		{
			PlayerVoteArea playerVoteArea = this.playerStates[i];
			if (playerVoteArea.VotedFor != 252 && playerVoteArea.VotedFor != 255 && playerVoteArea.VotedFor != 254)
			{
				int num;
				if (dictionary.TryGetValue(playerVoteArea.VotedFor, out num))
				{
					dictionary[playerVoteArea.VotedFor] = num + 1;
				}
				else
				{
					dictionary[playerVoteArea.VotedFor] = 1;
				}
			}
		}
		return dictionary;
	}

	public void HandleProceed()
	{
		if (!AmongUsClient.Instance.AmHost)
		{
			base.StartCoroutine(Effects.SwayX(this.HostIcon.transform, 0.75f, 0.25f));
			return;
		}
		if (this.state != MeetingHud.VoteStates.Results)
		{
			return;
		}
		this.state = MeetingHud.VoteStates.Proceeding;
		this.RpcClose();
	}

	private void SetupProceedButton()
	{
		if (AmongUsClient.Instance.GameMode != GameModes.OnlineGame)
		{
			this.TimerText.gameObject.SetActive(false);
			this.ProceedButton.gameObject.SetActive(true);
			this.HostIcon.gameObject.SetActive(true);
			GameData.PlayerInfo host = GameData.Instance.GetHost();
			if (host != null)
			{
				PlayerControl.SetPlayerMaterialColors(host.ColorId, this.HostIcon);
				return;
			}
			this.HostIcon.enabled = false;
		}
	}

	private void PopulateResults(MeetingHud.VoterState[] states)
	{
		this.TitleText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MeetingVotingResults, Array.Empty<object>());
		int num = 0;
		for (int i = 0; i < this.playerStates.Length; i++)
		{
			PlayerVoteArea playerVoteArea = this.playerStates[i];
			playerVoteArea.ClearForResults();
			int num2 = 0;
			foreach (MeetingHud.VoterState voterState in states)
			{
				GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(voterState.VoterId);
				if (playerById == null)
				{
					Debug.LogError(string.Format("Couldn't find player info for voter: {0}", voterState.VoterId));
				}
				else if (i == 0 && voterState.SkippedVote)
				{
					this.BloopAVoteIcon(playerById, num, this.SkippedVoting.transform);
					num++;
				}
				else if (voterState.VotedForId == playerVoteArea.TargetPlayerId)
				{
					this.BloopAVoteIcon(playerById, num2, playerVoteArea.transform);
					num2++;
				}
			}
		}
	}

	private void BloopAVoteIcon(GameData.PlayerInfo voterPlayer, int index, Transform parent)
	{
		SpriteRenderer spriteRenderer = UnityEngine.Object.Instantiate<SpriteRenderer>(this.PlayerVotePrefab);
		if (PlayerControl.GameOptions.AnonymousVotes)
		{
			PlayerControl.SetPlayerMaterialColors(Palette.DisabledGrey, spriteRenderer);
		}
		else
		{
			PlayerControl.SetPlayerMaterialColors(voterPlayer.ColorId, spriteRenderer);
		}
		spriteRenderer.transform.SetParent(parent);
		spriteRenderer.transform.localScale = Vector3.zero;
		base.StartCoroutine(Effects.Bloop((float)index * 0.3f, spriteRenderer.transform, 1f, 0.5f));
		parent.GetComponent<VoteSpreader>().AddVote(spriteRenderer);
	}

	private void UpdateButtons()
	{
		if (PlayerControl.LocalPlayer.Data.IsDead && !this.amDead)
		{
			this.SetForegroundForDead();
		}
		if (AmongUsClient.Instance.AmHost)
		{
			for (int i = 0; i < this.playerStates.Length; i++)
			{
				PlayerVoteArea playerVoteArea = this.playerStates[i];
				GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(playerVoteArea.TargetPlayerId);
				if (playerById == null)
				{
					playerVoteArea.SetDisabled();
				}
				else
				{
					bool flag = playerById.Disconnected || playerById.IsDead;
					if (flag != playerVoteArea.AmDead)
					{
						playerVoteArea.SetDead(this.reporterId == playerById.PlayerId, flag);
						base.SetDirtyBit(1U);
					}
				}
			}
		}
	}

	private void PopulateButtons(byte reporter)
	{
		this.playerStates = new PlayerVoteArea[GameData.Instance.PlayerCount];
		for (int i = 0; i < this.playerStates.Length; i++)
		{
			GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
			PlayerVoteArea playerVoteArea = this.playerStates[i] = this.CreateButton(playerInfo);
			playerVoteArea.Parent = this;
			playerVoteArea.SetTargetPlayerId(playerInfo.PlayerId);
			playerVoteArea.SetDead(reporter == playerInfo.PlayerId, playerInfo.Disconnected || playerInfo.IsDead);
			playerVoteArea.UpdateOverlay();
		}
		foreach (PlayerVoteArea playerVoteArea2 in this.playerStates)
		{
			ControllerManager.Instance.AddSelectableUiElement(playerVoteArea2.PlayerButton, false);
		}
		this.SortButtons();
	}

	private void SortButtons()
	{
		PlayerVoteArea[] array = this.playerStates.OrderBy(delegate(PlayerVoteArea p)
		{
			if (!p.AmDead)
			{
				return 0;
			}
			return 50;
		}).ThenBy((PlayerVoteArea p) => p.TargetPlayerId).ToArray<PlayerVoteArea>();
		for (int i = 0; i < array.Length; i++)
		{
			int num = i % 3;
			int num2 = i / 3;
			array[i].transform.localPosition = this.VoteOrigin + new Vector3(this.VoteButtonOffsets.x * (float)num, this.VoteButtonOffsets.y * (float)num2, -0.9f - (float)num2 * 0.01f);
		}
	}

	private PlayerVoteArea CreateButton(GameData.PlayerInfo playerInfo)
	{
		PlayerVoteArea playerVoteArea = UnityEngine.Object.Instantiate<PlayerVoteArea>(this.PlayerButtonPrefab, this.ButtonParent.transform);
		playerVoteArea.SetCosmetics(playerInfo);
		playerVoteArea.NameText.text = playerInfo.PlayerName;
		bool flag = PlayerControl.LocalPlayer.Data.IsImpostor && playerInfo.IsImpostor;
		playerVoteArea.NameText.color = (flag ? Palette.ImpostorRed : Color.white);
		playerVoteArea.transform.localScale = Vector3.one;
		return playerVoteArea;
	}

	public bool DidVote(byte playerId)
	{
		return this.playerStates.First((PlayerVoteArea p) => p.TargetPlayerId == playerId).DidVote;
	}

	public int GetVotesRemaining()
	{
		int result;
		try
		{
			result = this.playerStates.Count((PlayerVoteArea ps) => !ps.AmDead && !ps.DidVote);
		}
		catch
		{
			result = 0;
		}
		return result;
	}

	private struct VoterState
	{
		public byte VoterId;

		public byte VotedForId;

		public bool AmDead
		{
			get
			{
				return this.VotedForId == 252;
			}
		}

		public bool SkippedVote
		{
			get
			{
				return this.VotedForId == 253;
			}
		}

		public static MeetingHud.VoterState Deserialize(MessageReader reader)
		{
			MessageReader messageReader = reader.ReadMessage();
			return new MeetingHud.VoterState
			{
				VoterId = messageReader.Tag,
				VotedForId = messageReader.ReadByte()
			};
		}

		public void Serialize(MessageWriter writer)
		{
			writer.StartMessage(this.VoterId);
			writer.Write(this.VotedForId);
			writer.EndMessage();
		}
	}

	public enum VoteStates
	{
		Animating,
		Discussion,
		NotVoted,
		Voted,
		Results,
		Proceeding
	}
}
