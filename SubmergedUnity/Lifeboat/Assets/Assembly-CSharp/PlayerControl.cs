using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.CoreScripts;
using Beebyte.Obfuscator;
using Hazel;
using InnerNet;
using PowerTools;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

[SkipRename]
public class PlayerControl : InnerNetObject
{
	private int LastStartCounter;

	public byte PlayerId = byte.MaxValue;

	public float MaxReportDistance = 5f;

	public bool moveable = true;

	public bool inVent;

	public static PlayerControl LocalPlayer;

	private GameData.PlayerInfo _cachedData;

	public AudioSource FootSteps;

	public AudioClip KillSfx;

	public KillAnimation[] KillAnimations;

	[SerializeField]
	private float killTimer;

	public int RemainingEmergencies;

	public TextMeshPro nameText;

	public LightSource LightPrefab;

	private LightSource myLight;

	[HideInInspector]
	public Collider2D Collider;

	[HideInInspector]
	public PlayerPhysics MyPhysics;

	[HideInInspector]
	public CustomNetworkTransform NetTransform;

	public PetBehaviour CurrentPet;

	public HatParent HatRenderer;

	[SerializeField]
	private SpriteRenderer myRend;

	[SerializeField]
	private SpriteAnim myAnim;

	private Collider2D[] hitBuffer = new Collider2D[40];

	public static GameOptionsData GameOptions = new GameOptionsData();

	public List<PlayerTask> myTasks = new List<PlayerTask>();

	public SpriteAnim[] ScannerAnims;

	public SpriteRenderer[] ScannersImages;

	private IUsable closest;

	private bool isNew = true;

	public bool isDummy;

	public bool notRealPlayer;

	public static List<PlayerControl> AllPlayerControls = new List<PlayerControl>();

	private Dictionary<Collider2D, IUsable[]> cache = new Dictionary<Collider2D, IUsable[]>(PlayerControl.ColliderComparer.Instance);

	private List<IUsable> itemsInRange = new List<IUsable>();

	private List<IUsable> newItemsInRange = new List<IUsable>();

	private byte scannerCount;

	private bool infectedSet;
	
	public void RpcSetScanner(bool value)
	{
		byte b = (byte) (this.scannerCount + 1);
		this.scannerCount = b;
		byte b2 = b;
		if (AmongUsClient.Instance.AmClient)
		{
			this.SetScanner(value, b2);
		}
		if (!PlayerControl.GameOptions.VisualTasks)
		{
			return;
		}

		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(this.NetId, 15, SendOption.Reliable);
		messageWriter.Write(value);
		messageWriter.Write(b2);
		messageWriter.EndMessage();
	}

	public void RpcUsePlatform()
	{
		if (AmongUsClient.Instance.AmHost)
		{
			AirshipStatus airshipStatus = ShipStatus.Instance as AirshipStatus;
			if (airshipStatus)
			{
				airshipStatus.GapPlatform.Use(this);
				return;
			}
		}
		else
		{
			AmongUsClient.Instance.StartRpc(this.NetId, 32, SendOption.Reliable).EndMessage();
		}
	}

	public void RpcPlayAnimation(byte animType)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			this.PlayAnimation(animType);
		}
		if (!PlayerControl.GameOptions.VisualTasks)
		{
			return;
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(this.NetId, 0, 0);
		messageWriter.Write(animType);
		messageWriter.EndMessage();
	}

	public void RpcSetStartCounter(int secondsLeft)
	{
		int lastStartCounter = this.LastStartCounter;
		this.LastStartCounter = lastStartCounter + 1;
		int num = lastStartCounter;
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(this.NetId, 18, SendOption.Reliable);
		messageWriter.WritePacked(num);
		messageWriter.Write((sbyte)secondsLeft);
		messageWriter.EndMessage();
	}

	public void RpcCompleteTask(uint idx)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			this.CompleteTask(idx);
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(this.NetId, 1, SendOption.Reliable);
		messageWriter.WritePacked(idx);
		messageWriter.EndMessage();
	}

	public void RpcSyncSettings(GameOptionsData gameOptions)
	{
		if (!AmongUsClient.Instance.AmHost || DestroyableSingleton<TutorialManager>.InstanceExists)
		{
			return;
		}
		PlayerControl.GameOptions = gameOptions;
		SaveManager.GameHostOptions = gameOptions;
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(this.NetId, 2, SendOption.Reliable);
		messageWriter.WriteBytesAndSize(gameOptions.ToBytes(4));
		messageWriter.EndMessage();
	}

	public void RpcSetInfected(GameData.PlayerInfo[] infected)
	{
		byte[] array = (from p in infected
		select p.PlayerId).ToArray<byte>();
		if (AmongUsClient.Instance.AmClient)
		{
			this.SetInfected(array);
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(this.NetId, 3, SendOption.Reliable);
		messageWriter.WriteBytesAndSize(array);
		messageWriter.EndMessage();
	}

	public void CmdCheckName(string name)
	{
		if (AmongUsClient.Instance.AmHost)
		{
			this.CheckName(name);
			return;
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(this.NetId, 5, SendOption.Reliable, AmongUsClient.Instance.HostId);
		messageWriter.Write(name);
		AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
	}

	public void RpcSetSkin(uint skinId)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			this.SetSkin(skinId);
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(this.NetId, 10, SendOption.Reliable);
		messageWriter.WritePacked(skinId);
		messageWriter.EndMessage();
	}

	public void RpcSetHat(uint hatId)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			GameData.PlayerInfo data = this.Data;
			int colorId = (data != null) ? data.ColorId : 0;
			this.SetHat(hatId, colorId);
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(this.NetId, 9, SendOption.Reliable);
		messageWriter.WritePacked(hatId);
		messageWriter.EndMessage();
	}

	public void RpcSetPet(uint petId)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			this.SetPet(petId);
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(this.NetId, 17, SendOption.Reliable);
		messageWriter.WritePacked(petId);
		messageWriter.EndMessage();
	}

	public void RpcSetName(string name)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			this.SetName(name, false);
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(this.NetId, 6, SendOption.Reliable);
		messageWriter.Write(name);
		messageWriter.EndMessage();
	}

	public void CmdCheckColor(byte bodyColor)
	{
		if (AmongUsClient.Instance.AmHost)
		{
			this.CheckColor(bodyColor);
			return;
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(this.NetId, 7, SendOption.Reliable, AmongUsClient.Instance.HostId);
		messageWriter.Write(bodyColor);
		AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
	}

	public void RpcSetColor(byte bodyColor)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			this.SetColor((int)bodyColor);
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(this.NetId, 8, SendOption.Reliable);
		messageWriter.Write(bodyColor);
		messageWriter.EndMessage();
	}

	public bool RpcSendChat(string chatText)
	{
		chatText = Regex.Replace(chatText, "<.*?>", string.Empty);
		if (string.IsNullOrWhiteSpace(chatText))
		{
			return false;
		}
		if (AmongUsClient.Instance.AmClient && DestroyableSingleton<HudManager>.Instance)
		{
			DestroyableSingleton<HudManager>.Instance.Chat.AddChat(this, chatText);
		}
		if (chatText.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
		{
			DestroyableSingleton<Telemetry>.Instance.SendWho();
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(this.NetId, 13, SendOption.Reliable);
		messageWriter.Write(chatText);
		messageWriter.EndMessage();
		return true;
	}

	public bool RpcSendQuickChat(string chatText, QuickChatNetData chatData)
	{
		if (string.IsNullOrWhiteSpace(chatText) || chatData == null)
		{
			return false;
		}
		if (AmongUsClient.Instance.AmClient && DestroyableSingleton<HudManager>.Instance)
		{
			DestroyableSingleton<HudManager>.Instance.Chat.AddChat(this, chatText);
		}
		if (chatText.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
		{
			DestroyableSingleton<Telemetry>.Instance.SendWho();
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(this.NetId, 33, SendOption.Reliable);
		chatData.Serialize(messageWriter);
		messageWriter.EndMessage();
		return true;
	}

	public void RpcSendChatNote(byte srcPlayerId, ChatNoteTypes noteType)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(srcPlayerId);
			DestroyableSingleton<HudManager>.Instance.Chat.AddChatNote(playerById, noteType);
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(this.NetId, 16, SendOption.Reliable);
		messageWriter.Write(srcPlayerId);
		messageWriter.Write((byte)noteType);
		messageWriter.EndMessage();
	}

	public void CmdReportDeadBody(GameData.PlayerInfo target)
	{
		if (AmongUsClient.Instance.AmHost)
		{
			this.ReportDeadBody(target);
			return;
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(this.NetId, 11, SendOption.Reliable);
		messageWriter.Write((target != null) ? target.PlayerId : byte.MaxValue);
		messageWriter.EndMessage();
	}

	public void RpcStartMeeting(GameData.PlayerInfo info)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			base.StartCoroutine(this.CoStartMeeting(info));
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(this.NetId, 14, SendOption.Reliable, -1);
		messageWriter.Write((info != null) ? info.PlayerId : byte.MaxValue);
		AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
	}

	public void RpcMurderPlayer(PlayerControl target)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			this.MurderPlayer(target);
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(this.NetId, 12, SendOption.Reliable, -1);
		messageWriter.WriteNetObject(target);
		AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
	}

	public override bool Serialize(MessageWriter writer, bool initialState)
	{
		if (initialState)
		{
			writer.Write(this.isNew);
		}
		writer.Write(this.PlayerId);
		return true;
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
		if (initialState)
		{
			this.isNew = reader.ReadBoolean();
		}
		this.PlayerId = reader.ReadByte();
	}

	public override void HandleRpc(byte callId, MessageReader reader)
	{
		switch (callId)
		{
		case 0:
			this.PlayAnimation(reader.ReadByte());
			return;
		case 1:
			this.CompleteTask(reader.ReadPackedUInt32());
			return;
		case 2:
			PlayerControl.GameOptions = GameOptionsData.FromBytes(reader.ReadBytesAndSize());
			return;
		case 3:
			this.SetInfected(reader.ReadBytesAndSize());
			return;
		case 4:
			this.Exiled();
			return;
		case 5:
			this.CheckName(reader.ReadString());
			return;
		case 6:
			this.SetName(reader.ReadString(), false);
			return;
		case 7:
			this.CheckColor(reader.ReadByte());
			return;
		case 8:
			this.SetColor((int)reader.ReadByte());
			return;
		case 9:
		{
			GameData.PlayerInfo data = this.Data;
			int colorId = (data != null) ? data.ColorId : 0;
			this.SetHat(reader.ReadPackedUInt32(), colorId);
			return;
		}
		case 10:
			this.SetSkin(reader.ReadPackedUInt32());
			return;
		case 11:
		{
			GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(reader.ReadByte());
			this.ReportDeadBody(playerById);
			return;
		}
		case 12:
		{
			PlayerControl target = reader.ReadNetObject<PlayerControl>();
			this.MurderPlayer(target);
			return;
		}
		case 13:
		{
			string chatText = reader.ReadString();
			if (DestroyableSingleton<HudManager>.Instance)
			{
				DestroyableSingleton<HudManager>.Instance.Chat.AddChat(this, chatText);
				return;
			}
			break;
		}
		case 14:
		{
			GameData.PlayerInfo playerById2 = GameData.Instance.GetPlayerById(reader.ReadByte());
			base.StartCoroutine(this.CoStartMeeting(playerById2));
			return;
		}
		case 15:
			this.SetScanner(reader.ReadBoolean(), reader.ReadByte());
			break;
		case 16:
		{
			GameData.PlayerInfo playerById3 = GameData.Instance.GetPlayerById(reader.ReadByte());
			DestroyableSingleton<HudManager>.Instance.Chat.AddChatNote(playerById3, (ChatNoteTypes)reader.ReadByte());
			return;
		}
		case 17:
			this.SetPet(reader.ReadPackedUInt32());
			return;
		case 18:
		{
			int num = reader.ReadPackedInt32();
			sbyte startCounter = reader.ReadSByte();
			if (DestroyableSingleton<GameStartManager>.InstanceExists && this.LastStartCounter < num)
			{
				this.LastStartCounter = num;
				DestroyableSingleton<GameStartManager>.Instance.SetStartCounter(startCounter);
				return;
			}
			break;
		}
		case 19:
		case 20:
		case 21:
		case 22:
		case 23:
		case 24:
		case 25:
		case 26:
		case 27:
		case 28:
		case 29:
		case 30:
		case 31:
			break;
		case 32:
			if (AmongUsClient.Instance.AmHost)
			{
				AirshipStatus airshipStatus = ShipStatus.Instance as AirshipStatus;
				if (airshipStatus)
				{
					airshipStatus.GapPlatform.Use(this);
					base.SetDirtyBit(4096U);
					return;
				}
			}
			break;
		case 33:
		{
			string chatText2 = QuickChatNetData.Deserialize(reader);
			if (DestroyableSingleton<HudManager>.Instance)
			{
				DestroyableSingleton<HudManager>.Instance.Chat.AddChat(this, chatText2);
				return;
			}
			break;
		}
		default:
			return;
		}
	}

	public bool CanMove
	{
		get
		{
			return this.moveable && !Minigame.Instance && (!DestroyableSingleton<HudManager>.InstanceExists || (!DestroyableSingleton<HudManager>.Instance.Chat.IsOpen && !DestroyableSingleton<HudManager>.Instance.KillOverlay.IsOpen && !DestroyableSingleton<HudManager>.Instance.GameMenu.IsOpen)) && (!ControllerManager.Instance || !ControllerManager.Instance.IsUiControllerActive) && (!MapBehaviour.Instance || !MapBehaviour.Instance.IsOpenStopped) && !MeetingHud.Instance && !CustomPlayerMenu.Instance && !ExileController.Instance && !IntroCutscene.Instance;
		}
	}

	public GameData.PlayerInfo Data
	{
		get
		{
			if (this._cachedData == null)
			{
				if (!GameData.Instance)
				{
					return null;
				}
				this._cachedData = GameData.Instance.GetPlayerById(this.PlayerId);
			}
			return this._cachedData;
		}
	}

	public void SetKillTimer(float time)
	{
		if (PlayerControl.GameOptions.KillCooldown <= 0f)
		{
			return;
		}
		this.killTimer = Mathf.Clamp(time, 0f, PlayerControl.GameOptions.KillCooldown);
		DestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(this.killTimer, PlayerControl.GameOptions.KillCooldown);
	}

	public SpriteRenderer MyRend
	{
		get
		{
			return this.myRend;
		}
	}

	public SpriteAnim MyAnim
	{
		get
		{
			return this.myAnim;
		}
	}

	public bool Visible
	{
		get
		{
			return this.myRend.enabled;
		}
		set
		{
			this.myRend.enabled = value;
			this.MyPhysics.Skin.Visible = value;
			this.HatRenderer.gameObject.SetActive(value);
			if (this.CurrentPet)
			{
				this.CurrentPet.Visible = value;
			}
			this.nameText.gameObject.SetActive(value);
		}
	}

	private void Awake()
	{
		this.MyPhysics = base.GetComponent<PlayerPhysics>();
		this.NetTransform = base.GetComponent<CustomNetworkTransform>();
		this.Collider = base.GetComponent<Collider2D>();
		if (!this.notRealPlayer)
		{
			PlayerControl.AllPlayerControls.Add(this);
		}
	}

	private IEnumerator Start()
	{
		while (this.PlayerId == 255)
		{
			yield return null;
		}
		this.RemainingEmergencies = PlayerControl.GameOptions.NumEmergencyMeetings;
		if (base.AmOwner)
		{
			this.myLight = UnityEngine.Object.Instantiate<LightSource>(this.LightPrefab);
			this.myLight.transform.SetParent(base.transform);
			this.myLight.transform.localPosition = this.Collider.offset;
			PlayerControl.LocalPlayer = this;
			Camera.main.GetComponent<FollowerCamera>().SetTarget(this);
			this.SetName(SaveManager.PlayerName, false);
			this.SetColor((int)SaveManager.BodyColor);
			if (Application.targetFrameRate > 30)
			{
				this.MyPhysics.EnableInterpolation();
			}
			this.CmdCheckName(SaveManager.PlayerName);
			this.CmdCheckColor(SaveManager.BodyColor);
			this.RpcSetPet(SaveManager.LastPet);
			this.RpcSetHat((uint) HatManager.Instance.AllHats.RandomIdx());
			this.RpcSetSkin(SaveManager.LastSkin);
			yield return null;
			this.UpdatePlatformIcon();
		}
		else
		{
			base.StartCoroutine(this.ClientInitialize());
		}
		if (this.isNew)
		{
			this.isNew = false;
			base.StartCoroutine(this.MyPhysics.CoSpawnPlayer(LobbyBehaviour.Instance));
		}
		yield break;
	}

	private IEnumerator ClientInitialize()
	{
		this.Visible = false;
		while (!GameData.Instance || this.Data == null || this.Data.IsIncomplete)
		{
			yield return null;
		}
		this.SetName(this.Data.PlayerName, this.isDummy);
		this.SetColor(this.Data.ColorId);
		this.SetHat(this.Data.HatId, this.Data.ColorId);
		this.SetSkin(this.Data.SkinId);
		this.SetPet(this.Data.PetId);
		this.Visible = true;
		yield return null;
		this.UpdatePlatformIcon();
		yield break;
	}

	public override void OnDestroy()
	{
		if (this.CurrentPet)
		{
			 UnityEngine.Object.Destroy(this.CurrentPet.gameObject);
		}
		if (!this.notRealPlayer)
		{
			PlayerControl.AllPlayerControls.Remove(this);
		}
		base.OnDestroy();
	}

	private void FixedUpdate()
	{
		if (!GameData.Instance)
		{
			return;
		}
		GameData.PlayerInfo data = this.Data;
		if (data == null)
		{
			return;
		}
		if (data.IsDead && PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.Data != null)
		{
			this.Visible = PlayerControl.LocalPlayer.Data.IsDead;
		}
		if (base.AmOwner)
		{
			if (ShipStatus.Instance)
			{
				this.myLight.LightRadius = ShipStatus.Instance.CalculateLightRadius(data);
			}
			if (data.IsImpostor && this.CanMove && !data.IsDead)
			{
				this.SetKillTimer(this.killTimer - Time.fixedDeltaTime);
				PlayerControl target = this.FindClosestTarget();
				DestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(target);
			}
			else
			{
				DestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(null);
			}
			if (this.CanMove || this.inVent)
			{
				this.newItemsInRange.Clear();
				bool flag = (PlayerControl.GameOptions.GhostsDoTasks || !data.IsDead) && (!AmongUsClient.Instance || !AmongUsClient.Instance.IsGameOver) && this.CanMove;
				Vector2 truePosition = this.GetTruePosition();
				int num = Physics2D.OverlapCircleNonAlloc(truePosition, this.MaxReportDistance, this.hitBuffer, Constants.Usables);
				IUsable usable = null;
				float num2 = float.MaxValue;
				bool flag2 = false;
				for (int i = 0; i < num; i++)
				{
					Collider2D collider2D = this.hitBuffer[i];
					IUsable[] array;
					if (!this.cache.TryGetValue(collider2D, out array))
					{
						array = (this.cache[collider2D] = collider2D.GetComponents<IUsable>());
					}
					if (array != null && (flag || this.inVent))
					{
						foreach (IUsable usable2 in array)
						{
							bool flag3;
							bool flag4;
							float num3 = usable2.CanUse(data, out flag3, out flag4);
							if (flag3 || flag4)
							{
								this.newItemsInRange.Add(usable2);
							}
							if (flag3 && num3 < num2)
							{
								num2 = num3;
								usable = usable2;
							}
						}
					}
					if (flag && !data.IsDead && !flag2 && collider2D.tag == "DeadBody")
					{
						DeadBody component = collider2D.GetComponent<DeadBody>();
						if (component.enabled && Vector2.Distance(truePosition, component.TruePosition) <= this.MaxReportDistance && !PhysicsHelpers.AnythingBetween(truePosition, component.TruePosition, Constants.ShipAndObjectsMask, false))
						{
							flag2 = true;
						}
					}
				}
				for (int k = this.itemsInRange.Count - 1; k > -1; k--)
				{
					IUsable item = this.itemsInRange[k];
					int num4 = this.newItemsInRange.FindIndex((IUsable j) => j == item);
					if (num4 == -1)
					{
						item.SetOutline(false, false);
						this.itemsInRange.RemoveAt(k);
					}
					else
					{
						this.newItemsInRange.RemoveAt(num4);
						item.SetOutline(true, usable == item);
					}
				}
				for (int l = 0; l < this.newItemsInRange.Count; l++)
				{
					IUsable usable3 = this.newItemsInRange[l];
					usable3.SetOutline(true, usable == usable3);
					this.itemsInRange.Add(usable3);
				}
				this.closest = usable;
				DestroyableSingleton<HudManager>.Instance.UseButton.SetTarget(usable);
				DestroyableSingleton<HudManager>.Instance.ReportButton.SetActive(flag2);
				return;
			}
			this.closest = null;
			DestroyableSingleton<HudManager>.Instance.UseButton.SetTarget(null);
			DestroyableSingleton<HudManager>.Instance.ReportButton.SetActive(false);
		}
	}

	public void UseClosest()
	{
		if (this.closest != null)
		{
			this.closest.Use();
		}
		this.closest = null;
		DestroyableSingleton<HudManager>.Instance.UseButton.SetTarget(null);
	}

	public void ReportClosest()
	{
		if (AmongUsClient.Instance.IsGameOver)
		{
			return;
		}
		if (PlayerControl.LocalPlayer.Data.IsDead)
		{
			return;
		}
		foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(this.GetTruePosition(), this.MaxReportDistance, Constants.PlayersOnlyMask))
		{
			if (!(collider2D.tag != "DeadBody"))
			{
				DeadBody component = collider2D.GetComponent<DeadBody>();
				if (component && !component.Reported)
				{
					component.OnClick();
					if (component.Reported)
					{
						break;
					}
				}
			}
		}
	}

	public void PlayStepSound()
	{
		if (!Constants.ShouldPlaySfx())
		{
			return;
		}
		if (PlayerControl.LocalPlayer != this)
		{
			return;
		}
		if (LobbyBehaviour.Instance)
		{
			for (int i = 0; i < LobbyBehaviour.Instance.AllRooms.Length; i++)
			{
				SoundGroup soundGroup = LobbyBehaviour.Instance.AllRooms[i].MakeFootstep(this);
				if (soundGroup)
				{
					AudioClip clip = soundGroup.Random();
					this.FootSteps.clip = clip;
					this.FootSteps.Play();
					break;
				}
			}
		}
		if (!ShipStatus.Instance)
		{
			return;
		}
		for (int j = 0; j < ShipStatus.Instance.AllStepWatchers.Length; j++)
		{
			SoundGroup soundGroup2 = ShipStatus.Instance.AllStepWatchers[j].MakeFootstep(this);
			if (soundGroup2)
			{
				AudioClip clip2 = soundGroup2.Random();
				this.FootSteps.clip = clip2;
				this.FootSteps.Play();
				return;
			}
		}
	}

	private void SetScanner(bool on, byte cnt)
	{
		if (cnt < this.scannerCount)
		{
			return;
		}
		this.scannerCount = cnt;
		for (int i = 0; i < this.ScannerAnims.Length; i++)
		{
			SpriteAnim spriteAnim = this.ScannerAnims[i];
			if (on && !this.Data.IsDead)
			{
				spriteAnim.gameObject.SetActive(true);
				spriteAnim.Play(null, 1f);
				this.ScannersImages[i].flipX = !this.myRend.flipX;
			}
			else
			{
				if (spriteAnim.isActiveAndEnabled)
				{
					spriteAnim.Stop();
				}
				spriteAnim.gameObject.SetActive(false);
			}
		}
	}

	public Vector2 GetTruePosition()
	{
		return (Vector2) base.transform.position + this.Collider.offset;
	}

	private PlayerControl FindClosestTarget()
	{
		PlayerControl result = null;
		float num = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];
		if (!ShipStatus.Instance)
		{
			return null;
		}
		Vector2 truePosition = this.GetTruePosition();
		List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
		for (int i = 0; i < allPlayers.Count; i++)
		{
			GameData.PlayerInfo playerInfo = allPlayers[i];
			if (!playerInfo.Disconnected && playerInfo.PlayerId != this.PlayerId && !playerInfo.IsDead && !playerInfo.IsImpostor)
			{
				PlayerControl @object = playerInfo.Object;
				if (@object && @object.Collider.enabled)
				{
					Vector2 vector = @object.GetTruePosition() - truePosition;
					float magnitude = vector.magnitude;
					if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask))
					{
						result = @object;
						num = magnitude;
					}
				}
			}
		}
		return result;
	}

	public void SetTasks(List<GameData.TaskInfo> tasks)
	{
		base.StartCoroutine(this.CoSetTasks(tasks));
	}

	private IEnumerator CoSetTasks(List<GameData.TaskInfo> tasks)
	{
		while (!ShipStatus.Instance)
		{
			yield return null;
		}
		if (base.AmOwner)
		{
			DestroyableSingleton<HudManager>.Instance.TaskStuff.SetActive(true);
			StatsManager instance = StatsManager.Instance;
			uint num = instance.GamesStarted;
			instance.GamesStarted = num + 1U;
			DestroyableSingleton<AchievementManager>.Instance.OnMatchStart(this.Data.IsImpostor ? RoleTypes.Impostor : RoleTypes.Crewmate);
			if (this.Data.IsImpostor)
			{
				StatsManager instance2 = StatsManager.Instance;
				num = instance2.TimesImpostor;
				instance2.TimesImpostor = num + 1U;
				StatsManager.Instance.CrewmateStreak = 0U;
			}
			else
			{
				StatsManager instance3 = StatsManager.Instance;
				num = instance3.TimesCrewmate;
				instance3.TimesCrewmate = num + 1U;
				StatsManager instance4 = StatsManager.Instance;
				num = instance4.CrewmateStreak;
				instance4.CrewmateStreak = num + 1U;
				DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
			}
		}
		this.myTasks.DestroyAll<PlayerTask>();
		if (this.Data.IsImpostor)
		{
			ImportantTextTask importantTextTask = new GameObject("_Player").AddComponent<ImportantTextTask>();
			importantTextTask.transform.SetParent(PlayerControl.LocalPlayer.transform, false);
			importantTextTask.Text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ImpostorTask, Array.Empty<object>()) + "\r\n<color=#FFFFFFFF>" + DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.FakeTasks, Array.Empty<object>()) + "</color>";
			this.myTasks.Insert(0, importantTextTask);
		}
		for (int i = 0; i < tasks.Count; i++)
		{
			GameData.TaskInfo taskInfo = tasks[i];
			NormalPlayerTask normalPlayerTask = UnityEngine.Object.Instantiate<NormalPlayerTask>(ShipStatus.Instance.GetTaskById(taskInfo.TypeId), base.transform);
			normalPlayerTask.Id = taskInfo.Id;
			normalPlayerTask.Owner = this;
			normalPlayerTask.Initialize();
			this.myTasks.Add(normalPlayerTask);
		}
		yield break;
	}

	public void AddSystemTask(SystemTypes system)
	{
		PlayerTask playerTask;
		if (system <= SystemTypes.Electrical)
		{
			if (system != SystemTypes.Reactor)
			{
				if (system != SystemTypes.Electrical)
				{
					return;
				}
				playerTask = ShipStatus.Instance.SpecialTasks[1];
			}
			else
			{
				playerTask = ShipStatus.Instance.SpecialTasks[0];
			}
		}
		else if (system != SystemTypes.LifeSupp)
		{
			if (system != SystemTypes.Comms)
			{
				if (system != SystemTypes.Laboratory)
				{
					return;
				}
				playerTask = ShipStatus.Instance.SpecialTasks[4];
			}
			else
			{
				playerTask = ShipStatus.Instance.SpecialTasks[2];
			}
		}
		else
		{
			playerTask = ShipStatus.Instance.SpecialTasks[3];
		}
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		PlayerTask playerTask2 = UnityEngine.Object.Instantiate<PlayerTask>(playerTask, localPlayer.transform);
		playerTask2.Id = 255U;
		playerTask2.Owner = localPlayer;
		playerTask2.Initialize();
		localPlayer.myTasks.Add(playerTask2);
	}

	public void RemoveTask(PlayerTask task)
	{
		task.OnRemove();
		this.myTasks.Remove(task);
		GameData.Instance.TutOnlyRemoveTask(this.PlayerId, task.Id);
		DestroyableSingleton<HudManager>.Instance.UseButton.SetTarget(null);
		 UnityEngine.Object.Destroy(task.gameObject);
	}

	private void ClearTasks()
	{
		for (int i = 0; i < this.myTasks.Count; i++)
		{
			PlayerTask playerTask = this.myTasks[i];
			playerTask.OnRemove();
			 UnityEngine.Object.Destroy(playerTask.gameObject);
		}
		this.myTasks.Clear();
	}

	public void RemoveInfected()
	{
		GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(this.PlayerId);
		if (playerById.IsImpostor)
		{
			playerById.Object.nameText.color = Color.white;
			playerById.IsImpostor = false;
			this.myTasks.RemoveAt(0);
			DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
		}
	}

	public void Die(DeathReason reason)
	{
		if (!DestroyableSingleton<TutorialManager>.InstanceExists)
		{
			StatsManager.Instance.LastGameStarted = DateTime.MinValue;
			StatsManager instance = StatsManager.Instance;
			float banPoints = instance.BanPoints;
			instance.BanPoints = banPoints - 1f;
		}
		TempData.LastDeathReason = reason;
		if (this.CurrentPet)
		{
			this.CurrentPet.SetMourning();
		}
		this.Data.IsDead = true;
		base.gameObject.layer = LayerMask.NameToLayer("Ghost");
		this.nameText.GetComponent<MeshRenderer>().material.SetInt("_Mask", 0);
		if (base.AmOwner)
		{
			DestroyableSingleton<HudManager>.Instance.Chat.SetVisible(true);
		}
	}

	public void Revive()
	{
		this.Data.IsDead = false;
		base.gameObject.layer = LayerMask.NameToLayer("Players");
		this.MyPhysics.ResetMoveState(true);
		if (this.CurrentPet)
		{
			this.CurrentPet.Source = this;
		}
		this.nameText.GetComponent<MeshRenderer>().material.SetInt("_Mask", 4);
		if (base.AmOwner)
		{
			DestroyableSingleton<HudManager>.Instance.ShadowQuad.gameObject.SetActive(true);
			DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(this.Data.IsImpostor);
			DestroyableSingleton<HudManager>.Instance.Chat.ForceClosed();
			DestroyableSingleton<HudManager>.Instance.Chat.SetVisible(false);
		}
	}

	public void PlayAnimation(byte animType)
	{
		if (animType == 1)
		{
			ShipStatus.Instance.StartShields();
			return;
		}
		if (animType == 6)
		{
			ShipStatus.Instance.FireWeapon();
			return;
		}
		if (animType - 9 > 1)
		{
			return;
		}
		ShipStatus.Instance.OpenHatch();
	}

	public void CompleteTask(uint idx)
	{
		PlayerTask playerTask = this.myTasks.Find((PlayerTask p) => p.Id == idx);
		if (playerTask)
		{
			GameData.Instance.CompleteTask(this, idx);
			playerTask.Complete();
			return;
		}
		Debug.LogWarning(this.PlayerId.ToString() + ": Server didn't have task: " + idx.ToString());
	}

	private void SetInfected(byte[] infected)
	{
		if (!DestroyableSingleton<TutorialManager>.InstanceExists && this.infectedSet)
		{
			return;
		}
		this.infectedSet = true;
		StatsManager instance = StatsManager.Instance;
		float banPoints = instance.BanPoints;
		instance.BanPoints = banPoints + 1f;
		StatsManager.Instance.LastGameStarted = DateTime.UtcNow;
		for (int i = 0; i < infected.Length; i++)
		{
			GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(infected[i]);
			if (playerById != null)
			{
				playerById.IsImpostor = true;
			}
		}
		DestroyableSingleton<HudManager>.Instance.MapButton.gameObject.SetActive(true);
		DestroyableSingleton<HudManager>.Instance.ReportButton.gameObject.SetActive(true);
		DestroyableSingleton<HudManager>.Instance.UseButton.gameObject.SetActive(true);
		PlayerControl.LocalPlayer.RemainingEmergencies = PlayerControl.GameOptions.NumEmergencyMeetings;
		GameData.PlayerInfo data = PlayerControl.LocalPlayer.Data;
		if (data.IsImpostor)
		{
			ImportantTextTask importantTextTask = new GameObject("_Player").AddComponent<ImportantTextTask>();
			importantTextTask.transform.SetParent(PlayerControl.LocalPlayer.transform, false);
			importantTextTask.Text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ImpostorTask, Array.Empty<object>()) + "\r\n<color=#FFFFFFFF>" + DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.FakeTasks, Array.Empty<object>()) + "</color>";
			this.myTasks.Insert(0, importantTextTask);
			DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(true);
			PlayerControl.LocalPlayer.SetKillTimer(10f);
			for (int j = 0; j < infected.Length; j++)
			{
				GameData.PlayerInfo playerById2 = GameData.Instance.GetPlayerById(infected[j]);
				if (playerById2 != null)
				{
					playerById2.Object.nameText.color = Palette.ImpostorRed;
				}
			}
		}
		if (!DestroyableSingleton<TutorialManager>.InstanceExists)
		{
			List<PlayerControl> yourTeam;
			if (data.IsImpostor)
			{
				yourTeam = (from pcd in GameData.Instance.AllPlayers
				where !pcd.Disconnected
				where pcd.IsImpostor
				select pcd.Object).OrderBy(delegate(PlayerControl pc)
				{
					if (!(pc == PlayerControl.LocalPlayer))
					{
						return 1;
					}
					return 0;
				}).ToList<PlayerControl>();
			}
			else
			{
				yourTeam = (from pcd in GameData.Instance.AllPlayers
				where !pcd.Disconnected
				select pcd.Object).OrderBy(delegate(PlayerControl pc)
				{
					if (!(pc == PlayerControl.LocalPlayer))
					{
						return 1;
					}
					return 0;
				}).ToList<PlayerControl>();
			}
			base.StopAllCoroutines();
			DestroyableSingleton<HudManager>.Instance.StartCoroutine(DestroyableSingleton<HudManager>.Instance.CoShowIntro(yourTeam));
		}
	}

	public void Exiled()
	{
		this.Die(DeathReason.Exile);
		if (base.AmOwner)
		{
			StatsManager instance = StatsManager.Instance;
			uint timesEjected = instance.TimesEjected;
			instance.TimesEjected = timesEjected + 1U;
			DestroyableSingleton<HudManager>.Instance.ShadowQuad.gameObject.SetActive(false);
			ImportantTextTask importantTextTask = new GameObject("_Player").AddComponent<ImportantTextTask>();
			importantTextTask.transform.SetParent(base.transform, false);
			if (this.Data.IsImpostor)
			{
				this.ClearTasks();
				importantTextTask.Text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GhostImpostor, Array.Empty<object>());
			}
			else if (!PlayerControl.GameOptions.GhostsDoTasks)
			{
				this.ClearTasks();
				importantTextTask.Text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GhostIgnoreTasks, Array.Empty<object>());
			}
			else
			{
				importantTextTask.Text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GhostDoTasks, Array.Empty<object>());
			}
			this.myTasks.Insert(0, importantTextTask);
		}
	}

	public void CheckName(string name)
	{
		List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
		bool flag = allPlayers.Any((GameData.PlayerInfo i) => i.PlayerId != this.PlayerId && i.PlayerName.Equals(name, StringComparison.OrdinalIgnoreCase));
		if (flag)
		{
			for (int k = 1; k < 100; k++)
			{
				string text = name + " " + k.ToString();
				flag = false;
				for (int j = 0; j < allPlayers.Count; j++)
				{
					if (allPlayers[j].PlayerId != this.PlayerId && allPlayers[j].PlayerName.Equals(text, StringComparison.OrdinalIgnoreCase))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					name = text;
					break;
				}
			}
		}
		this.RpcSetName(name);
		GameData.Instance.UpdateName(this.PlayerId, name, false);
	}

	public void SetName(string name, bool dontCensor = false)
	{
		if (GameData.Instance)
		{
			GameData.Instance.UpdateName(this.PlayerId, name, dontCensor);
		}
		base.gameObject.name = name;
		if (name == "")
		{
			this.nameText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.PlayerName, Array.Empty<object>());
		}
		else
		{
			this.nameText.text = name;
		}
		this.nameText.GetComponent<MeshRenderer>().material.SetInt("_Mask", 4);
	}

	public void CheckColor(byte bodyColor)
	{
		List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
		int num = 0;
		while (num++ < 100 && ((int)bodyColor >= Palette.PlayerColors.Length || allPlayers.Any((GameData.PlayerInfo p) => !p.Disconnected && p.PlayerId != this.PlayerId && p.ColorId == (int)bodyColor)))
		{
			bodyColor = (byte)((int)(bodyColor + 1) % Palette.PlayerColors.Length);
		}
		this.RpcSetColor(bodyColor);
	}

	public void SetHatAlpha(float a)
	{
		Color white = Color.white;
		white.a = a;
		this.HatRenderer.color = white;
	}

	public void SetColor(int bodyColor)
	{
		if (GameData.Instance)
		{
			GameData.Instance.UpdateColor(this.PlayerId, bodyColor);
		}
		if (this.myRend == null)
		{
			base.GetComponent<SpriteRenderer>();
		}
		PlayerControl.SetPlayerMaterialColors(bodyColor, this.myRend);
		this.HatRenderer.SetColor(bodyColor);
		if (this.CurrentPet)
		{
			PlayerControl.SetPlayerMaterialColors(bodyColor, this.CurrentPet.rend);
		}
	}

	public void SetSkin(uint skinId)
	{
		if (GameData.Instance)
		{
			GameData.Instance.UpdateSkin(this.PlayerId, skinId);
		}
		this.MyPhysics.SetSkin(skinId);
	}

	public void SetHat(uint hatId, int colorId)
	{
		if (hatId == 4294967295U)
		{
			return;
		}
		if (GameData.Instance)
		{
			GameData.Instance.UpdateHat(this.PlayerId, hatId);
		}
		this.HatRenderer.SetHat(hatId, colorId);
		this.nameText.transform.localPosition = new Vector3(0f, ((hatId == 0U) ? 0.7f : 1.05f) * 2f, -0.5f);
	}

	public void SetPet(uint petId)
	{
		if (this.CurrentPet)
		{
			 UnityEngine.Object.Destroy(this.CurrentPet.gameObject);
		}
		this.CurrentPet = UnityEngine.Object.Instantiate<PetBehaviour>(DestroyableSingleton<HatManager>.Instance.GetPetById(petId));
		this.CurrentPet.transform.position = base.transform.position;
		this.CurrentPet.Source = this;
		GameData.PlayerInfo data = this.Data;
		if (this.Data != null)
		{
			GameData.Instance.UpdatePet(this.PlayerId, petId);
			this.Data.PetId = petId;
			PlayerControl.SetPlayerMaterialColors(this.Data.ColorId, this.CurrentPet.rend);
		}
	}

	public static void SetPetImage(uint petId, int colorId, SpriteRenderer target)
	{
		if (!DestroyableSingleton<HatManager>.InstanceExists)
		{
			return;
		}
		PlayerControl.SetPetImage(DestroyableSingleton<HatManager>.Instance.GetPetById(petId), colorId, target);
	}

	public static void SetPetImage(PetBehaviour pet, int colorId, SpriteRenderer target)
	{
		target.sprite = pet.rend.sprite;
		if (target != pet.rend)
		{
			target.material = new Material(pet.rend.sharedMaterial);
			PlayerControl.SetPlayerMaterialColors(colorId, target);
		}
	}

	public static void SetSkinImage(uint skinId, SpriteRenderer target)
	{
		if (!DestroyableSingleton<HatManager>.InstanceExists)
		{
			return;
		}
		PlayerControl.SetSkinImage(DestroyableSingleton<HatManager>.Instance.GetSkinById(skinId), target);
	}

	public static void SetSkinImage(SkinData skin, SpriteRenderer target)
	{
		target.sprite = skin.IdleFrame;
	}

	private void ReportDeadBody(GameData.PlayerInfo target)
	{
		if (AmongUsClient.Instance.IsGameOver)
		{
			return;
		}
		if (MeetingHud.Instance)
		{
			return;
		}
		if (target == null && PlayerControl.LocalPlayer.myTasks.Any(new Func<PlayerTask, bool>(PlayerTask.TaskIsEmergency)))
		{
			return;
		}
		if (this.Data.IsDead)
		{
			return;
		}
		MeetingRoomManager.Instance.AssignSelf(this, target);
		if (!AmongUsClient.Instance.AmHost)
		{
			return;
		}
		if (ShipStatus.Instance.CheckTaskCompletion())
		{
			return;
		}
		DestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(this);
		this.RpcStartMeeting(target);
	}

	public IEnumerator CoStartMeeting(GameData.PlayerInfo target)
	{
		bool isEmergency = target == null;
		DestroyableSingleton<Telemetry>.Instance.WriteMeetingStarted(isEmergency);
		while (!MeetingHud.Instance)
		{
			yield return null;
		}
		MeetingRoomManager.Instance.RemoveSelf();
		for (int i = 0; i < PlayerControl.AllPlayerControls.Count; i++)
		{
			PlayerControl playerControl = PlayerControl.AllPlayerControls[i];
			if (!playerControl.GetComponent<DummyBehaviour>().enabled)
			{
				playerControl.MyPhysics.ExitAllVents();
				ShipStatus.Instance.SpawnPlayer(playerControl, GameData.Instance.PlayerCount, false);
			}
			playerControl.NetTransform.enabled = true;
			playerControl.MyPhysics.ResetMoveState(true);
		}
		if (base.AmOwner)
		{
			if (isEmergency)
			{
				this.RemainingEmergencies--;
				StatsManager instance = StatsManager.Instance;
				uint num = instance.EmergenciesCalled;
				instance.EmergenciesCalled = num + 1U;
			}
			else
			{
				StatsManager instance2 = StatsManager.Instance;
				uint num = instance2.BodiesReported;
				instance2.BodiesReported = num + 1U;
			}
		}
		if (MapBehaviour.Instance)
		{
			MapBehaviour.Instance.Close();
		}
		if (Minigame.Instance)
		{
			Minigame.Instance.ForceClose();
		}
		ShipStatus.Instance.OnMeetingCalled();
		KillAnimation.SetMovement(this, true);
		DeadBody[] array = Object.FindObjectsOfType<DeadBody>();
		GameData.PlayerInfo[] deadBodies = (from b in array
		select GameData.Instance.GetPlayerById(b.ParentId)).ToArray<GameData.PlayerInfo>();
		for (int j = 0; j < array.Length; j++)
		{
			 UnityEngine.Object.Destroy(array[j].gameObject);
		}
		MeetingHud.Instance.StartCoroutine(MeetingHud.Instance.CoIntro(this.Data, target, deadBodies));
		yield break;
	}

	public void MurderPlayer(PlayerControl target)
	{
		if (AmongUsClient.Instance.IsGameOver)
		{
			return;
		}
		if (!target || this.Data.IsDead || !this.Data.IsImpostor || this.Data.Disconnected)
		{
			int num = target ? ((int)target.PlayerId) : -1;
			Debug.LogWarning(string.Format("Bad kill from {0} to {1}", this.PlayerId, num));
			return;
		}
		GameData.PlayerInfo data = target.Data;
		if (data == null || data.IsDead)
		{
			Debug.LogWarning("Missing target data for kill");
			return;
		}
		if (base.AmOwner)
		{
			StatsManager instance = StatsManager.Instance;
			uint num2 = instance.ImpostorKills;
			instance.ImpostorKills = num2 + 1U;
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(this.KillSfx, false, 0.8f);
			}
			this.SetKillTimer(PlayerControl.GameOptions.KillCooldown);
		}
		DestroyableSingleton<Telemetry>.Instance.WriteMurder();
		target.gameObject.layer = LayerMask.NameToLayer("Ghost");
		if (target.AmOwner)
		{
			StatsManager instance2 = StatsManager.Instance;
			uint num2 = instance2.TimesMurdered;
			instance2.TimesMurdered = num2 + 1U;
			if (Minigame.Instance)
			{
				try
				{
					Minigame.Instance.Close();
					Minigame.Instance.Close();
				}
				catch
				{
				}
			}
			DestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(this.Data, data);
			DestroyableSingleton<HudManager>.Instance.ShadowQuad.gameObject.SetActive(false);
			target.nameText.GetComponent<MeshRenderer>().material.SetInt("_Mask", 0);
			target.RpcSetScanner(false);
			ImportantTextTask importantTextTask = new GameObject("_Player").AddComponent<ImportantTextTask>();
			importantTextTask.transform.SetParent(base.transform, false);
			if (!PlayerControl.GameOptions.GhostsDoTasks)
			{
				target.ClearTasks();
				importantTextTask.Text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GhostIgnoreTasks, Array.Empty<object>());
			}
			else
			{
				importantTextTask.Text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GhostDoTasks, Array.Empty<object>());
			}
			target.myTasks.Insert(0, importantTextTask);
		}
		DestroyableSingleton<AchievementManager>.Instance.OnMurder(base.AmOwner, target.AmOwner);
		this.MyPhysics.StartCoroutine(this.KillAnimations.Random<KillAnimation>().CoPerformKill(this, target));
	}

	public void SetPlayerMaterialColors(Renderer rend)
	{
		GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(this.PlayerId);
		PlayerControl.SetPlayerMaterialColors((playerById != null) ? playerById.ColorId : 0, rend);
	}

	public static void SetPlayerMaterialColors(int colorId, Renderer rend)
	{
		if (!rend || colorId < 0 || colorId >= Palette.PlayerColors.Length)
		{
			return;
		}
		rend.material.SetColor("_BackColor", Palette.ShadowColors[colorId]);
		rend.material.SetColor("_BodyColor", Palette.PlayerColors[colorId]);
		rend.material.SetColor("_VisorColor", Palette.VisorColor);
	}

	public static void SetPlayerMaterialColors(Color color, Renderer rend)
	{
		if (!rend)
		{
			return;
		}
		rend.material.SetColor("_BackColor", color);
		rend.material.SetColor("_BodyColor", color);
		rend.material.SetColor("_VisorColor", Palette.VisorColor);
	}

	public static void HideCursorTemporarily()
	{
		if (PlayerControl.LocalPlayer.AmOwner)
		{
			PlayerControl.LocalPlayer.MyPhysics.inputHandler.enabled = true;
		}
	}

	public void SetAppearanceFromSaveData()
	{
		this.MyPhysics.ResetMoveState(true);
		this.SetName(SaveManager.PlayerName, false);
		this.SetColor((int)SaveManager.BodyColor);
		this.SetHat(SaveManager.LastHat, (int)SaveManager.BodyColor);
		this.SetSkin(SaveManager.LastSkin);
		this.SetPet(SaveManager.LastPet);
		SpriteAnimNodeSync[] componentsInChildren = base.GetComponentsInChildren<SpriteAnimNodeSync>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].LateUpdate();
		}
	}

	public void UpdatePlatformIcon()
	{
		PlatformIdentifierIcon componentInChildren = base.GetComponentInChildren<PlatformIdentifierIcon>();
		ClientData client = AmongUsClient.Instance.GetClient(this.OwnerId);
		if (client != null && client.platformID != (RuntimePlatform) (-1))
		{
			componentInChildren.SetIcon(client.platformID);
		}
	}

	public class ColliderComparer : IEqualityComparer<Collider2D>
	{
		public static readonly PlayerControl.ColliderComparer Instance = new PlayerControl.ColliderComparer();

		public bool Equals(Collider2D x, Collider2D y)
		{
			return x == y;
		}

		public int GetHashCode(Collider2D obj)
		{
			return obj.GetInstanceID();
		}
	}

	public class UsableComparer : IEqualityComparer<IUsable>
	{
		public static readonly PlayerControl.UsableComparer Instance = new PlayerControl.UsableComparer();

		public bool Equals(IUsable x, IUsable y)
		{
			return x == y;
		}

		public int GetHashCode(IUsable obj)
		{
			return obj.GetHashCode();
		}
	}
}
