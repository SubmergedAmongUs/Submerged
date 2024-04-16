using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using InnerNet;
using PowerTools;
using UnityEngine;
using Object = UnityEngine.Object;

public class ShipStatus : InnerNetObject
{
	public static ShipStatus Instance;

	public Color CameraColor = Color.black;

	public float MaxLightRadius = 5f;

	public float MinLightRadius = 1f;

	public float MapScale = 4.4f;

	public MapBehaviour MapPrefab;

	public ExileController ExileCutscenePrefab;

	public MeetingCalledAnimation EmergencyOverlay;

	public MeetingCalledAnimation ReportOverlay;

	public Vector2 InitialSpawnCenter;

	public Vector2 MeetingSpawnCenter;

	public Vector2 MeetingSpawnCenter2;

	public float SpawnRadius = 1.55f;

	public NormalPlayerTask[] CommonTasks;

	public NormalPlayerTask[] LongTasks;

	public NormalPlayerTask[] NormalTasks;

	public PlayerTask[] SpecialTasks;

	public Transform[] DummyLocations;

	public SurvCamera[] AllCameras;

	public PlainDoor[] AllDoors;

	public global::Console[] AllConsoles;

	public Dictionary<SystemTypes, ISystemType> Systems;

	public StringNames[] SystemNames;

	public AudioClip SabotageSound;

	public AnimationClip[] WeaponFires;

	public SpriteAnim WeaponsImage;

	public AudioClip[] VentMoveSounds;

	public AudioClip VentEnterSound;

	public AnimationClip HatchActive;

	public SpriteAnim Hatch;

	public ParticleSystem HatchParticles;

	public AnimationClip ShieldsActive;

	public SpriteAnim[] ShieldsImages;

	public SpriteRenderer ShieldBorder;

	public Sprite ShieldBorderOn;

	public MedScannerBehaviour MedScanner;

	private int WeaponFireIdx;

	public float Timer;

	public float EmergencyCooldown;

	public ShipStatus.MapType Type;

	private int numScans;

	public void RpcCloseDoorsOfType(SystemTypes type)
	{
		if (AmongUsClient.Instance.AmHost)
		{
			this.CloseDoorsOfType(type);
			return;
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(this.NetId, 27, SendOption.Reliable, AmongUsClient.Instance.HostId);
		messageWriter.Write((byte)type);
		AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
	}

	public void RpcRepairSystem(SystemTypes systemType, int amount)
	{
		if (AmongUsClient.Instance.AmHost)
		{
			this.RepairSystem(systemType, PlayerControl.LocalPlayer, (byte)amount);
			return;
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(this.NetId, 28, SendOption.Reliable, AmongUsClient.Instance.HostId);
		messageWriter.Write((byte)systemType);
		messageWriter.WriteNetObject(PlayerControl.LocalPlayer);
		messageWriter.Write((byte)amount);
		AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
	}

	public void RpcUpdateSystem(SystemTypes systemType, MessageWriter msgWriter)
	{
		if (AmongUsClient.Instance.AmHost)
		{
			MessageReader msgReader = MessageReader.Get(msgWriter.ToByteArray(false));
			this.UpdateSystem(systemType, PlayerControl.LocalPlayer, msgReader);
			return;
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(this.NetId, 35, SendOption.Reliable, AmongUsClient.Instance.HostId);
		messageWriter.Write((byte)systemType);
		messageWriter.WriteNetObject(PlayerControl.LocalPlayer);
		messageWriter.Write(msgWriter, false);
		AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
	}

	public override bool Serialize(MessageWriter writer, bool initialState)
	{
		bool result = false;
		short num = 0;
		while ((int)num < SystemTypeHelpers.AllTypes.Length)
		{
			SystemTypes systemTypes = SystemTypeHelpers.AllTypes[(int)num];
			ISystemType systemType;
			if (this.Systems.TryGetValue(systemTypes, out systemType) && (initialState || systemType.IsDirty))
			{
				result = true;
				writer.StartMessage((byte)systemTypes);
				systemType.Serialize(writer, initialState);
				writer.EndMessage();
			}
			num += 1;
		}
		return result;
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
		while (reader.Position < reader.Length)
		{
			MessageReader messageReader = reader.ReadMessage();
			SystemTypes tag = (SystemTypes)messageReader.Tag;
			ISystemType systemType;
			if (this.Systems.TryGetValue(tag, out systemType))
			{
				systemType.Deserialize(messageReader, initialState);
			}
		}
	}

	public override void HandleRpc(byte callId, MessageReader reader)
	{
		if (callId == 27)
		{
			this.CloseDoorsOfType((SystemTypes)reader.ReadByte());
			return;
		}
		if (callId == 28)
		{
			this.RepairSystem((SystemTypes)reader.ReadByte(), reader.ReadNetObject<PlayerControl>(), reader.ReadByte());
			return;
		}
		if (callId != 35)
		{
			return;
		}
		this.UpdateSystem((SystemTypes)reader.ReadByte(), reader.ReadNetObject<PlayerControl>(), reader);
	}

	public IStepWatcher[] AllStepWatchers { get; private set; }

	public PlainShipRoom[] AllRooms { get; private set; }

	public Dictionary<SystemTypes, PlainShipRoom> FastRooms { get; private set; }

	public Vent[] AllVents { get; private set; }

	public bool BeginCalled { get; set; }

	public override bool IsDirty
	{
		get
		{
			return true;
		}
	}

	protected virtual void OnEnable()
	{
		if (this.Systems != null)
		{
			return;
		}
		this.Systems = new Dictionary<SystemTypes, ISystemType>(ShipStatus.SystemTypeComparer.Instance)
		{
			{
				SystemTypes.Electrical,
				new SwitchSystem()
			},
			{
				SystemTypes.MedBay,
				new MedScanSystem()
			}
		};
		Camera main = Camera.main;
		main.backgroundColor = this.CameraColor;
		FollowerCamera component = main.GetComponent<FollowerCamera>();
		switch (this.Type)
		{
		case ShipStatus.MapType.Ship:
			this.Systems.Add(SystemTypes.Doors, new AutoDoorsSystemType());
			this.Systems.Add(SystemTypes.Comms, new HudOverrideSystemType());
			this.Systems.Add(SystemTypes.Security, new SecurityCameraSystemType());
			this.Systems.Add(SystemTypes.Reactor, new ReactorSystemType(30f, SystemTypes.Reactor));
			this.Systems.Add(SystemTypes.LifeSupp, new LifeSuppSystemType(30f));
			this.Systems.Add(SystemTypes.Ventilation, new VentilationSystem());
			if (component)
			{
				component.shakeAmount = 0.02f;
				component.shakePeriod = 0.3f;
			}
			break;
		case ShipStatus.MapType.Hq:
			this.Systems.Add(SystemTypes.Comms, new HqHudSystemType());
			this.Systems.Add(SystemTypes.Reactor, new ReactorSystemType(45f, SystemTypes.Reactor));
			this.Systems.Add(SystemTypes.LifeSupp, new LifeSuppSystemType(45f));
			this.Systems.Add(SystemTypes.Ventilation, new VentilationSystem());
			if (component)
			{
				component.shakeAmount = 0f;
				component.shakePeriod = 0f;
			}
			break;
		case ShipStatus.MapType.Pb:
			DestroyableSingleton<HudManager>.Instance.ShadowQuad.material.SetInt("_Mask", 7);
			this.Systems.Add(SystemTypes.Doors, new DoorsSystemType());
			this.Systems.Add(SystemTypes.Comms, new HudOverrideSystemType());
			this.Systems.Add(SystemTypes.Security, new SecurityCameraSystemType());
			this.Systems.Add(SystemTypes.Laboratory, new ReactorSystemType(60f, SystemTypes.Laboratory));
			if (component)
			{
				component.shakeAmount = 0f;
				component.shakePeriod = 0f;
			}
			break;
		}
		this.Systems.Add(SystemTypes.Sabotage, new SabotageSystemType((from i in this.Systems.Values
		where i is IActivatable
		select i).Cast<IActivatable>().ToArray<IActivatable>()));
	}

	public virtual void RepairGameOverSystems()
	{
		this.RepairSystem(SystemTypes.Laboratory, PlayerControl.LocalPlayer, 16);
		this.RepairSystem(SystemTypes.Reactor, PlayerControl.LocalPlayer, 16);
		this.RepairSystem(SystemTypes.LifeSupp, PlayerControl.LocalPlayer, 16);
	}

	private void Awake()
	{
		this.AllStepWatchers = (from s in base.GetComponentsInChildren<IStepWatcher>()
		orderby s.Priority descending
		select s).ToArray<IStepWatcher>();
		this.AllRooms = base.GetComponentsInChildren<PlainShipRoom>();
		this.FastRooms = (from p in this.AllRooms
		where p.RoomId > SystemTypes.Hallway
		select p).ToDictionary((PlainShipRoom d) => d.RoomId);
		this.AllCameras = base.GetComponentsInChildren<SurvCamera>();
		this.AllConsoles = base.GetComponentsInChildren<global::Console>();
		this.AllVents = base.GetComponentsInChildren<Vent>();
		this.AssignTaskIndexes();
		ShipStatus.Instance = this;
	}

	protected virtual void Start()
	{
		if (DestroyableSingleton<HudManager>.InstanceExists)
		{
			DestroyableSingleton<HudManager>.Instance.Chat.ForceClosed();
			DestroyableSingleton<HudManager>.Instance.Chat.SetVisible(false);
			DestroyableSingleton<HudManager>.Instance.GameSettings.gameObject.SetActive(false);
		}
		foreach (DeconSystem deconSystem in base.GetComponentsInChildren<DeconSystem>())
		{
			this.Systems.Add(deconSystem.TargetSystem, deconSystem);
		}
		foreach (PlayerControl playerControl in PlayerControl.AllPlayerControls)
		{
			playerControl.UpdatePlatformIcon();
		}
	}

	public override void OnDestroy()
	{
		if (SoundManager.Instance)
		{
			SoundManager.Instance.StopAllSound();
		}
		base.OnDestroy();
	}

	public virtual void SpawnPlayer(PlayerControl player, int numPlayers, bool initialSpawn)
	{
		Vector2 vector = Vector2.up;
		vector = vector.Rotate((float)(player.PlayerId - 1) * (360f / (float)numPlayers));
		vector *= this.SpawnRadius;
		Vector2 position = (initialSpawn ? this.InitialSpawnCenter : this.MeetingSpawnCenter) + vector + new Vector2(0f, 0.3636f);
		player.NetTransform.SnapTo(position);
	}

	public void StartShields()
	{
		for (int i = 0; i < this.ShieldsImages.Length; i++)
		{
			this.ShieldsImages[i].Play(this.ShieldsActive, 1f);
		}
		this.ShieldBorder.sprite = this.ShieldBorderOn;
	}

	public void FireWeapon()
	{
		if (this.WeaponsImage && !this.WeaponsImage.IsPlaying())
		{
			this.WeaponsImage.Play(this.WeaponFires[this.WeaponFireIdx], 1f);
			this.WeaponFireIdx = (this.WeaponFireIdx + 1) % this.WeaponFires.Length;
		}
	}

	public NormalPlayerTask GetTaskById(byte idx)
	{
		NormalPlayerTask result;
		if ((result = this.CommonTasks.FirstOrDefault((NormalPlayerTask t) => t.Index == (int)idx)) == null)
		{
			result = (this.LongTasks.FirstOrDefault((NormalPlayerTask t) => t.Index == (int)idx) ?? this.NormalTasks.FirstOrDefault((NormalPlayerTask t) => t.Index == (int)idx));
		}
		return result;
	}

	public void OpenHatch()
	{
		if (this.Hatch && !this.Hatch.IsPlaying())
		{
			this.Hatch.Play(this.HatchActive, 1f);
			this.HatchParticles.Play();
		}
	}

	public void CloseDoorsOfType(SystemTypes room)
	{
		((IDoorSystem)this.Systems[SystemTypes.Doors]).CloseDoorsOfType(room);
	}

	public void RepairSystem(SystemTypes systemType, PlayerControl player, byte amount)
	{
		ISystemType systemType2;
		if (this.Systems.TryGetValue(systemType, out systemType2))
		{
			systemType2.RepairDamage(player, amount);
		}
	}

	public void UpdateSystem(SystemTypes systemType, PlayerControl player, MessageReader msgReader)
	{
		ISystemType systemType2;
		if (this.Systems.TryGetValue(systemType, out systemType2))
		{
			systemType2.UpdateSystem(player, msgReader);
		}
	}

	internal void SelectInfected()
	{
		List<GameData.PlayerInfo> list = (from pcd in GameData.Instance.AllPlayers
		where !pcd.Disconnected
		select pcd into pc
		where !pc.IsDead
		select pc).ToList<GameData.PlayerInfo>();
		int adjustedNumImpostors = PlayerControl.GameOptions.GetAdjustedNumImpostors(GameData.Instance.PlayerCount);
		GameData.PlayerInfo[] array = new GameData.PlayerInfo[Mathf.Min(list.Count, adjustedNumImpostors)];
		for (int i = 0; i < array.Length; i++)
		{
			int index = HashRandom.FastNext(list.Count);
			array[i] = list[index];
			list.RemoveAt(index);
		}
		PlayerControl.LocalPlayer.RpcSetInfected(array);
	}

	private void AssignTaskIndexes()
	{
		int num = 0;
		for (int i = 0; i < this.CommonTasks.Length; i++)
		{
			this.CommonTasks[i].Index = num++;
		}
		for (int j = 0; j < this.LongTasks.Length; j++)
		{
			this.LongTasks[j].Index = num++;
		}
		for (int k = 0; k < this.NormalTasks.Length; k++)
		{
			this.NormalTasks[k].Index = num++;
		}
	}

	public virtual void OnMeetingCalled()
	{
	}

	public virtual IEnumerator PrespawnStep()
	{
		yield break;
	}

	public void Begin()
	{
		this.numScans = 0;
		this.AssignTaskIndexes();
		GameOptionsData gameOptions = PlayerControl.GameOptions;
		List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
		HashSet<TaskTypes> hashSet = new HashSet<TaskTypes>();
		List<byte> list = new List<byte>(10);
		List<NormalPlayerTask> list2 = this.CommonTasks.ToList<NormalPlayerTask>();
		list2.Shuffle(0);
		int num = 0;
		this.AddTasksFromList(ref num, gameOptions.NumCommonTasks, list, hashSet, list2);
		for (int i = 0; i < gameOptions.NumCommonTasks; i++)
		{
			if (list2.Count == 0)
			{
				Debug.LogWarning("Not enough common tasks");
				break;
			}
			int index = list2.RandomIdx<NormalPlayerTask>();
			list.Add((byte)list2[index].Index);
			list2.RemoveAt(index);
		}
		List<NormalPlayerTask> list3 = this.LongTasks.ToList<NormalPlayerTask>();
		list3.Shuffle(0);
		List<NormalPlayerTask> list4 = this.NormalTasks.ToList<NormalPlayerTask>();
		list4.Shuffle(0);
		int num2 = 0;
		int num3 = 0;
		int count = gameOptions.NumShortTasks;
		if (gameOptions.NumCommonTasks + gameOptions.NumLongTasks + gameOptions.NumShortTasks == 0)
		{
			count = 1;
		}
		byte b = 0;
		while ((int)b < allPlayers.Count)
		{
			hashSet.Clear();
			list.RemoveRange(gameOptions.NumCommonTasks, list.Count - gameOptions.NumCommonTasks);
			this.AddTasksFromList(ref num2, gameOptions.NumLongTasks, list, hashSet, list3);
			this.AddTasksFromList(ref num3, count, list, hashSet, list4);
			GameData.PlayerInfo playerInfo = allPlayers[(int)b];
			if (playerInfo.Object && !playerInfo.Object.GetComponent<DummyBehaviour>().enabled)
			{
				byte[] taskTypeIds = list.ToArray();
				GameData.Instance.RpcSetTasks(playerInfo.PlayerId, taskTypeIds);
			}
			b += 1;
		}
		this.BeginCalled = true;
	}

	private void AddTasksFromList(ref int start, int count, List<byte> tasks, HashSet<TaskTypes> usedTaskTypes, List<NormalPlayerTask> unusedTasks)
	{
		int num = 0;
		int num2 = 0;
		Func<NormalPlayerTask, bool> func1 = (NormalPlayerTask t) => usedTaskTypes.Contains(t.TaskType);
		while (num2 < count && num++ != 1000)
		{
			if (start >= unusedTasks.Count)
			{
				start = 0;
				unusedTasks.Shuffle(0);
				Func<NormalPlayerTask, bool> predicate;
				if ((predicate = func1) == null)
				{
					predicate = func1;
				}
				if (unusedTasks.All(predicate))
				{
					Debug.Log("Not enough task types");
					usedTaskTypes.Clear();
				}
			}
			int num3 = start;
			start = num3 + 1;
			NormalPlayerTask normalPlayerTask = unusedTasks[num3];
			if (!usedTaskTypes.Add(normalPlayerTask.TaskType))
			{
				num2--;
			}
			else
			{
				tasks.Add((byte)normalPlayerTask.Index);
				if (!PlayerControl.GameOptions.VisualTasks && normalPlayerTask.TaskType == TaskTypes.SubmitScan)
				{
					num3 = this.numScans;
					this.numScans = num3 + 1;
					if (num3 > 1)
					{
						unusedTasks.Remove(normalPlayerTask);
					}
				}
			}
			num2++;
		}
	}

	public void FixedUpdate()
	{
		if (!AmongUsClient.Instance)
		{
			return;
		}
		if (!PlayerControl.LocalPlayer)
		{
			return;
		}
		this.Timer += Time.fixedDeltaTime;
		this.EmergencyCooldown -= Time.fixedDeltaTime;
		if (GameData.Instance)
		{
			GameData.Instance.RecomputeTaskCounts();
		}
		if (AmongUsClient.Instance.AmHost && this.BeginCalled)
		{
			this.CheckEndCriteria();
		}
		if (AmongUsClient.Instance.AmClient)
		{
			for (int i = 0; i < SystemTypeHelpers.AllTypes.Length; i++)
			{
				SystemTypes key = SystemTypeHelpers.AllTypes[i];
				ISystemType systemType;
				if (this.Systems.TryGetValue(key, out systemType))
				{
					systemType.Detoriorate(Time.fixedDeltaTime);
				}
			}
		}
	}

	public virtual float CalculateLightRadius(GameData.PlayerInfo player)
	{
		if (player == null || player.IsDead)
		{
			return this.MaxLightRadius;
		}
		SwitchSystem switchSystem = (SwitchSystem)this.Systems[SystemTypes.Electrical];
		if (player.IsImpostor)
		{
			return this.MaxLightRadius * PlayerControl.GameOptions.ImpostorLightMod;
		}
		float num = (float)switchSystem.Value / 255f;
		return Mathf.Lerp(this.MinLightRadius, this.MaxLightRadius, num) * PlayerControl.GameOptions.CrewLightMod;
	}

	private void CheckEndCriteria()
	{
		if (!GameData.Instance)
		{
			return;
		}
		ISystemType systemType;
		if (this.Systems.TryGetValue(SystemTypes.LifeSupp, out systemType))
		{
			LifeSuppSystemType lifeSuppSystemType = (LifeSuppSystemType)systemType;
			if (lifeSuppSystemType.Countdown < 0f)
			{
				this.EndGameForSabotage();
				lifeSuppSystemType.Countdown = 10000f;
			}
		}
		ISystemType systemType2;
		if ((this.Systems.TryGetValue(SystemTypes.Reactor, out systemType2) || this.Systems.TryGetValue(SystemTypes.Laboratory, out systemType2)) && systemType2 is ICriticalSabotage)
		{
			ICriticalSabotage criticalSabotage = (ICriticalSabotage)systemType2;
			if (criticalSabotage.Countdown < 0f)
			{
				this.EndGameForSabotage();
				criticalSabotage.ClearSabotage();
			}
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < GameData.Instance.PlayerCount; i++)
		{
			GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
			if (!playerInfo.Disconnected)
			{
				if (playerInfo.IsImpostor)
				{
					num3++;
				}
				if (!playerInfo.IsDead)
				{
					if (playerInfo.IsImpostor)
					{
						num2++;
					}
					else
					{
						num++;
					}
				}
			}
		}
		if (num2 <= 0 && (!DestroyableSingleton<TutorialManager>.InstanceExists || num3 > 0))
		{
			if (!DestroyableSingleton<TutorialManager>.InstanceExists)
			{
				this.BeginCalled = false;
				ShipStatus.RpcEndGame(GameOverReason.HumansByVote, !SaveManager.BoughtNoAds);
				return;
			}
			if (!Minigame.Instance)
			{
				DestroyableSingleton<HudManager>.Instance.ShowPopUp(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameOverImpostorDead, Array.Empty<object>()));
				ShipStatus.ReviveEveryone();
				return;
			}
		}
		else if (num <= num2)
		{
			if (!DestroyableSingleton<TutorialManager>.InstanceExists)
			{
				this.BeginCalled = false;
				GameOverReason endReason;
				switch (TempData.LastDeathReason)
				{
				case DeathReason.Exile:
					endReason = GameOverReason.ImpostorByVote;
					break;
				case DeathReason.Kill:
					endReason = GameOverReason.ImpostorByKill;
					break;
				default:
					endReason = GameOverReason.ImpostorByVote;
					break;
				}
				ShipStatus.RpcEndGame(endReason, !SaveManager.BoughtNoAds);
				return;
			}
			DestroyableSingleton<HudManager>.Instance.ShowPopUp(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameOverImpostorKills, Array.Empty<object>()));
			ShipStatus.ReviveEveryone();
			return;
		}
		else if (!DestroyableSingleton<TutorialManager>.InstanceExists)
		{
			if (GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
			{
				base.enabled = false;
				ShipStatus.RpcEndGame(GameOverReason.HumansByTask, !SaveManager.BoughtNoAds);
				return;
			}
		}
		else if (PlayerControl.LocalPlayer.myTasks.All((PlayerTask t) => t.IsComplete))
		{
			DestroyableSingleton<HudManager>.Instance.ShowPopUp(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameOverTaskWin, Array.Empty<object>()));
			this.Begin();
		}
	}

	private void EndGameForSabotage()
	{
		if (!DestroyableSingleton<TutorialManager>.InstanceExists)
		{
			this.BeginCalled = false;
			ShipStatus.RpcEndGame(GameOverReason.ImpostorBySabotage, !SaveManager.BoughtNoAds);
			return;
		}
		DestroyableSingleton<HudManager>.Instance.ShowPopUp(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameOverSabotage, Array.Empty<object>()));
	}

	public bool IsGameOverDueToDeath()
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < GameData.Instance.PlayerCount; i++)
		{
			GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
			if (!playerInfo.Disconnected)
			{
				if (playerInfo.IsImpostor)
				{
					num3++;
				}
				if (!playerInfo.IsDead)
				{
					if (playerInfo.IsImpostor)
					{
						num2++;
					}
					else
					{
						num++;
					}
				}
			}
		}
		return (num2 <= 0 && (!DestroyableSingleton<TutorialManager>.InstanceExists || num3 > 0)) || num <= num2;
	}

	private static void RpcEndGame(GameOverReason endReason, bool showAd)
	{
		Debug.Log("Endgame for " + endReason.ToString());
		MessageWriter messageWriter = AmongUsClient.Instance.StartEndGame();
		messageWriter.Write((byte)endReason);
		messageWriter.Write(showAd);
		AmongUsClient.Instance.FinishEndGame(messageWriter);
	}

	private static void ReviveEveryone()
	{
		for (int i = 0; i < GameData.Instance.PlayerCount; i++)
		{
			GameData.Instance.AllPlayers[i].Object.Revive();
		}
		Object.FindObjectsOfType<DeadBody>().ForEach(delegate(DeadBody b)
		{
			 UnityEngine.Object.Destroy(b.gameObject);
		});
	}

	public bool CheckTaskCompletion()
	{
		if (DestroyableSingleton<TutorialManager>.InstanceExists)
		{
			if (PlayerControl.LocalPlayer.myTasks.All((PlayerTask t) => t.IsComplete))
			{
				DestroyableSingleton<HudManager>.Instance.ShowPopUp(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameOverTaskWin, Array.Empty<object>()));
				this.Begin();
			}
			return false;
		}
		GameData.Instance.RecomputeTaskCounts();
		if (GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
		{
			this.BeginCalled = false;
			ShipStatus.RpcEndGame(GameOverReason.HumansByTask, !SaveManager.BoughtNoAds);
			return true;
		}
		return false;
	}

	public enum MapType
	{
		Ship,
		Hq,
		Pb
	}

	public class SystemTypeComparer : IEqualityComparer<SystemTypes>
	{
		public static readonly ShipStatus.SystemTypeComparer Instance = new ShipStatus.SystemTypeComparer();

		public bool Equals(SystemTypes x, SystemTypes y)
		{
			return x == y;
		}

		public int GetHashCode(SystemTypes obj)
		{
			return (int)obj;
		}
	}
}
