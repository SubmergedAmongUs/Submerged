using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Assets.CoreScripts;
using InnerNet;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class AmongUsClient : InnerNetClient
{
	public static AmongUsClient Instance;

	public string OnlineScene;

	public string MainMenuScene;

	public GameData GameDataPrefab;

	public PlayerControl PlayerPrefab;

	public List<AssetReference> ShipPrefabs;

	public int TutorialMapId;

	public float SpawnRadius = 1.75f;

	public DiscoveryState discoverState;

	public List<IDisconnectHandler> DisconnectHandlers = new List<IDisconnectHandler>();

	public List<IGameListHandler> GameListHandlers = new List<IGameListHandler>();

	public void Awake()
	{
		if (AmongUsClient.Instance)
		{
			if (AmongUsClient.Instance != this)
			{
				 UnityEngine.Object.Destroy(base.gameObject);
			}
			return;
		}
		AmongUsClient.Instance = this;
		Object.DontDestroyOnLoad(base.gameObject);
		Application.targetFrameRate = 60;
	}

	protected override byte[] GetConnectionData()
	{
		byte[] result;
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
			{
				binaryWriter.Write(Constants.GetBroadcastVersion());
				binaryWriter.Write(SaveManager.PlayerName);
				binaryWriter.Write(DestroyableSingleton<AuthManager>.Instance.LastNonceReceived.GetValueOrDefault());
				binaryWriter.Write(SaveManager.LastLanguage);
				binaryWriter.Write((byte)SaveManager.ChatModeType);
				binaryWriter.Flush();
				result = memoryStream.ToArray();
			}
		}
		return result;
	}

	public void StartGame()
	{
		base.SendStartGame();
		this.discoverState = DiscoveryState.Off;
	}

	public void ExitGame(DisconnectReasons reason = DisconnectReasons.ExitGame)
	{
		if (DestroyableSingleton<WaitForHostPopup>.InstanceExists)
		{
			DestroyableSingleton<WaitForHostPopup>.Instance.Hide();
		}
		SoundManager.Instance.StopAllSound();
		this.discoverState = DiscoveryState.Off;
		this.DisconnectHandlers.Clear();
		base.DisconnectInternal(reason, null);
		SceneManager.LoadScene(this.MainMenuScene);
	}

	public void ExitCurrentGameToMoveToADifferentOne(DisconnectReasons reason = DisconnectReasons.ExitGame)
	{
		if (DestroyableSingleton<WaitForHostPopup>.InstanceExists)
		{
			DestroyableSingleton<WaitForHostPopup>.Instance.Hide();
		}
		SoundManager.Instance.StopAllSound();
		this.discoverState = DiscoveryState.Off;
		this.DisconnectHandlers.Clear();
		base.DisconnectInternal(reason, null);
		SceneManager.LoadScene(this.MainMenuScene);
	}

	protected override void OnGetGameList(InnerNetClient.TotalGameData totalGames, List<GameListing> availableGames)
	{
		for (int i = 0; i < this.GameListHandlers.Count; i++)
		{
			try
			{
				this.GameListHandlers[i].HandleList(totalGames, availableGames);
			}
			catch (Exception ex)
			{
				Debug.LogError("AmongUsClient::OnGetGameList: Exception:");
				Debug.LogException(ex, this);
			}
		}
	}

	protected override void OnReportedPlayer(ReportOutcome outcome, int clientId, string playerName, ReportReasons reason)
	{
	}

	protected override void OnGameCreated(string gameIdString)
	{
	}

	protected override void OnWaitForHost(string gameIdString)
	{
		if (this.GameState != InnerNetClient.GameStates.Joined)
		{
			Debug.Log("Waiting for host: " + gameIdString);
			if (DestroyableSingleton<WaitForHostPopup>.InstanceExists)
			{
				DestroyableSingleton<WaitForHostPopup>.Instance.Show();
			}
		}
	}

	protected override IEnumerator CoStartGame()
	{
		DestroyableSingleton<Telemetry>.Instance.Init();
		string str = "Received game start: ";
		bool flag = base.AmHost;
		Debug.Log(str + flag.ToString());
		yield return null;
		while (!DestroyableSingleton<HudManager>.InstanceExists)
		{
			yield return null;
		}
		while (!PlayerControl.LocalPlayer)
		{
			yield return null;
		}
		PlayerControl.LocalPlayer.moveable = false;
		PlayerControl.LocalPlayer.MyPhysics.inputHandler.enabled = true;
		CustomPlayerMenu customPlayerMenu = UnityEngine.Object.FindObjectOfType<CustomPlayerMenu>();
		if (customPlayerMenu)
		{
			customPlayerMenu.Close(false);
		}
		if (DestroyableSingleton<GameStartManager>.InstanceExists)
		{
			this.DisconnectHandlers.Remove(DestroyableSingleton<GameStartManager>.Instance);
			 UnityEngine.Object.Destroy(DestroyableSingleton<GameStartManager>.Instance.gameObject);
		}
		if (DestroyableSingleton<DiscordManager>.InstanceExists)
		{
			DestroyableSingleton<DiscordManager>.Instance.SetPlayingGame();
		}
		yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.clear, Color.black, 0.2f);
		while (!GameData.Instance)
		{
			yield return null;
		}
		while (!base.AmHost)
		{
			while (PlayerControl.LocalPlayer.Data == null && !base.AmHost)
			{
				yield return null;
			}
			if (!base.AmHost)
			{
				base.SendClientReady();
				while (!ShipStatus.Instance && !base.AmHost)
				{
					yield return null;
				}
				if (!base.AmHost)
				{
					ShipStatus.Instance.BeginCalled = true;
					IL_3EB:
					for (int i = 0; i < GameData.Instance.PlayerCount; i++)
					{
						PlayerControl @object = GameData.Instance.AllPlayers[i].Object;
						if (@object)
						{
							@object.moveable = true;
							@object.NetTransform.enabled = true;
							@object.MyPhysics.enabled = true;
							@object.MyPhysics.Awake();
							@object.MyPhysics.ResetMoveState(true);
							@object.Collider.enabled = true;
							ShipStatus.Instance.SpawnPlayer(@object, GameData.Instance.PlayerCount, true);
						}
					}
					try
					{
						DestroyableSingleton<Telemetry>.Instance.StartGame(AmongUsClient.Instance.AmHost, GameData.Instance.PlayerCount, PlayerControl.GameOptions.NumImpostors, AmongUsClient.Instance.GameMode, StatsManager.Instance.TimesImpostor, StatsManager.Instance.GamesStarted, StatsManager.Instance.CrewmateStreak);
						GameData.PlayerInfo data = PlayerControl.LocalPlayer.Data;
						DestroyableSingleton<Telemetry>.Instance.StartGameCosmetics(data.ColorId, data.HatId, data.SkinId, data.PetId);
						yield break;
					}
					catch
					{
						yield break;
					}
					yield break;
				}
			}
		}
		GameData.Instance.SetDirty();
		base.SendClientReady();
		float timer = 0f;
		for (;;)
		{
			bool stopWaiting = true;
			List<ClientData> allClients = this.allClients;
			lock (allClients)
			{
				for (int j = 0; j < this.allClients.Count; j++)
				{
					ClientData clientData = this.allClients[j];
					if (!clientData.IsReady)
					{
						if (timer < 5f)
						{
							stopWaiting = false;
						}
						else
						{
							base.SendLateRejection(clientData.Id, DisconnectReasons.Error);
							clientData.IsReady = true;
							this.OnPlayerLeft(clientData, DisconnectReasons.Error);
						}
					}
				}
			}
			yield return null;
			if (stopWaiting)
			{
				break;
			}
			timer += Time.deltaTime;
		}
		if (LobbyBehaviour.Instance)
		{
			LobbyBehaviour.Instance.Despawn();
		}
		if (!ShipStatus.Instance)
		{
			int num = Mathf.Clamp((int)PlayerControl.GameOptions.MapId, 0, GameOptionsData.MapNames.Length - 1);
			try
			{
				if (num == 0 && Constants.ShouldFlipSkeld())
				{
					num = 3;
				}
				else if (num == 3 && !Constants.ShouldFlipSkeld())
				{
					num = 0;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("AmongUsClient::CoStartGame: Exception:");
				Debug.LogException(ex, this);
			}
			AsyncOperationHandle<GameObject> shipPrefab = this.ShipPrefabs[num].InstantiateAsync(null, false);
			yield return shipPrefab;
			ShipStatus.Instance = shipPrefab.Result.GetComponent<ShipStatus>();
			shipPrefab = default(AsyncOperationHandle<GameObject>);
		}
		base.Spawn(ShipStatus.Instance, -2, SpawnFlags.None);
		ShipStatus.Instance.SelectInfected();
		ShipStatus.Instance.Begin();
		// goto IL_3EB; TODO Fix among us client
	}

	protected override void OnBecomeHost()
	{
		ClientData clientData = base.FindClientById(this.ClientId);
		if (base.IsGameStarted)
		{
			if (!clientData.Character)
			{
				this.CreatePlayer(clientData);
			}
		}
		else if (!clientData.Character)
		{
			this.OnGameJoined(null, clientData);
		}
		Debug.Log("Became Host");
		base.RemoveUnownedObjects();
		if (!GameData.Instance)
		{
			Debug.LogWarning("Trying to recover from no game data");
			GameData.Instance = UnityEngine.Object.Instantiate<GameData>(this.GameDataPrefab);
			base.Spawn(GameData.Instance, -2, SpawnFlags.None);
		}
		if (LobbyBehaviour.Instance && ShipStatus.Instance)
		{
			LobbyBehaviour.Instance.Despawn();
		}
	}

	protected override void OnGameEnd(GameOverReason gameOverReason, bool showAd)
	{
		StatsManager.Instance.BanPoints -= 1.5f;
		StatsManager.Instance.LastGameStarted = DateTime.MinValue;
		DestroyableSingleton<AchievementManager>.Instance.OnMatchEnd(gameOverReason);
		this.DisconnectHandlers.Clear();
		if (Minigame.Instance)
		{
			try
			{
				Minigame.Instance.Close();
				Minigame.Instance.Close();
			}
			catch (Exception ex)
			{
				Debug.LogError("AmongUsClient::OnGameEnd Exception: 1");
				Debug.LogException(ex, this);
			}
		}
		try
		{
			DestroyableSingleton<Telemetry>.Instance.EndGame(gameOverReason);
		}
		catch (Exception ex2)
		{
			Debug.LogError("AmongUsClient::OnGameEnd Exception: 2");
			Debug.LogException(ex2, this);
		}
		TempData.EndReason = gameOverReason;
		TempData.showAd = showAd;
		bool flag = TempData.DidHumansWin(gameOverReason);
		TempData.winners = new List<WinningPlayerData>();
		for (int i = 0; i < GameData.Instance.PlayerCount; i++)
		{
			GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
			if (playerInfo != null && (gameOverReason == GameOverReason.HumansDisconnect || gameOverReason == GameOverReason.ImpostorDisconnect || flag != playerInfo.IsImpostor))
			{
				TempData.winners.Add(new WinningPlayerData(playerInfo));
			}
		}
		base.StartCoroutine(this.CoEndGame());
	}

	public IEnumerator CoEndGame()
	{
		yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.clear, Color.black, 0.5f);
		SceneManager.LoadScene("EndGame");
		yield break;
	}

	protected override void OnPlayerJoined(ClientData data)
	{
		if (DestroyableSingleton<GameStartManager>.InstanceExists)
		{
			DestroyableSingleton<GameStartManager>.Instance.ResetStartState();
		}
		if (base.AmHost && data.InScene)
		{
			this.CreatePlayer(data);
		}
	}

	protected override void OnGameJoined(string gameIdString, ClientData data)
	{
		if (DestroyableSingleton<WaitForHostPopup>.InstanceExists)
		{
			DestroyableSingleton<WaitForHostPopup>.Instance.Hide();
		}
		if (!string.IsNullOrWhiteSpace(this.OnlineScene))
		{
			SceneManager.LoadScene(this.OnlineScene);
		}
		if (this.GameMode == GameModes.FreePlay)
		{
			return;
		}
		base.SendSelfClientInfoToAll();
	}

	protected override void OnPlayerLeft(ClientData data, DisconnectReasons reason)
	{
		if (DestroyableSingleton<GameStartManager>.InstanceExists)
		{
			DestroyableSingleton<GameStartManager>.Instance.ResetStartState();
		}
		PlayerControl character = data.Character;
		if (character)
		{
			for (int i = this.DisconnectHandlers.Count - 1; i > -1; i--)
			{
				IDisconnectHandler disconnectHandler = this.DisconnectHandlers[i];
				if (disconnectHandler is MonoBehaviour && !(MonoBehaviour)disconnectHandler)
				{
					this.DisconnectHandlers.RemoveAt(i);
				}
				else
				{
					try
					{
						disconnectHandler.HandleDisconnect(character, reason);
					}
					catch (Exception ex)
					{
						Debug.LogError("AmongUsClient::OnPlayerLeft: Exception: 1");
						Debug.LogException(ex, this);
						this.DisconnectHandlers.RemoveAt(i);
					}
				}
			}
			 UnityEngine.Object.Destroy(character.gameObject);
		}
		else
		{
			Debug.LogWarning(string.Format("A player without a character disconnected: {0}: {1}", data.Id, data.InScene));
			for (int j = this.DisconnectHandlers.Count - 1; j > -1; j--)
			{
				IDisconnectHandler disconnectHandler2 = this.DisconnectHandlers[j];
				if (disconnectHandler2 is MonoBehaviour && !(MonoBehaviour)disconnectHandler2)
				{
					this.DisconnectHandlers.RemoveAt(j);
				}
				else
				{
					try
					{
						disconnectHandler2.HandleDisconnect();
					}
					catch (Exception ex2)
					{
						Debug.LogError("AmongUsClient::OnPlayerLeft: Exception: 2");
						Debug.LogException(ex2, this);
						this.DisconnectHandlers.RemoveAt(j);
					}
				}
			}
		}
		if (base.AmHost)
		{
			if (!GameData.Instance)
			{
				this.ExitGame(DisconnectReasons.Error);
				return;
			}
			if (PlayerControl.GameOptions.isDefaults)
			{
				PlayerControl.GameOptions.SetRecommendations(GameData.Instance.PlayerCount, AmongUsClient.Instance.GameMode);
				if (PlayerControl.LocalPlayer)
				{
					PlayerControl.LocalPlayer.RpcSyncSettings(PlayerControl.GameOptions);
				}
			}
		}
		base.RemoveUnownedObjects();
	}

	protected override void OnDisconnected()
	{
		SceneManager.LoadScene(this.MainMenuScene);
	}

	protected override IEnumerator CoOnPlayerChangedScene(ClientData client, string currentScene)
	{
		client.InScene = true;
		if (!base.AmHost)
		{
			yield break;
		}
		if (currentScene.Equals("Tutorial"))
		{
			GameData.Instance = UnityEngine.Object.Instantiate<GameData>(this.GameDataPrefab);
			base.Spawn(GameData.Instance, -2, SpawnFlags.None);
			int index = (this.TutorialMapId == 0 && Constants.ShouldFlipSkeld()) ? 3 : this.TutorialMapId;
			AsyncOperationHandle<GameObject> shipPrefab = this.ShipPrefabs[index].InstantiateAsync(null, false);
			yield return shipPrefab;
			GameObject result = shipPrefab.Result;
			base.Spawn(result.GetComponent<ShipStatus>(), -2, SpawnFlags.None);
			this.CreatePlayer(client);
			shipPrefab = default(AsyncOperationHandle<GameObject>);
		}
		else if (currentScene.Equals("OnlineGame"))
		{
			if (client.Id != this.ClientId)
			{
				base.SendInitialData(client.Id);
			}
			else
			{
				if (this.GameMode == GameModes.LocalGame)
				{
					base.StartCoroutine(this.CoBroadcastManager());
				}
				if (!GameData.Instance)
				{
					GameData.Instance = UnityEngine.Object.Instantiate<GameData>(this.GameDataPrefab);
					base.Spawn(GameData.Instance, -2, SpawnFlags.None);
				}
			}
			this.CreatePlayer(client);
		}
		yield break;
	}

	private void CreatePlayer(ClientData clientData)
	{
		if (clientData.Character)
		{
			return;
		}
		if (!base.AmHost)
		{
			Debug.Log("Waiting for host to make my player");
			return;
		}
		if (!GameData.Instance)
		{
			GameData.Instance = UnityEngine.Object.Instantiate<GameData>(this.GameDataPrefab);
			base.Spawn(GameData.Instance, -2, SpawnFlags.None);
		}
		sbyte availableId = GameData.Instance.GetAvailableId();
		if (availableId == -1)
		{
			base.SendLateRejection(clientData.Id, DisconnectReasons.GameFull);
			Debug.Log("Overfilled room.");
			return;
		}
		Vector2 zero = Vector2.zero;
		if (DestroyableSingleton<TutorialManager>.InstanceExists)
		{
			zero = new Vector2(-1.9f, 3.25f);
		}
		PlayerControl playerControl = UnityEngine.Object.Instantiate<PlayerControl>(this.PlayerPrefab, zero, Quaternion.identity);
		playerControl.PlayerId = (byte)availableId;
		clientData.Character = playerControl;
		if (ShipStatus.Instance)
		{
			ShipStatus.Instance.SpawnPlayer(playerControl, Palette.PlayerColors.Length, false);
		}
		base.Spawn(playerControl, clientData.Id, SpawnFlags.IsClientCharacter);
		GameData.Instance.AddPlayer(playerControl);
		if (PlayerControl.GameOptions.isDefaults)
		{
			PlayerControl.GameOptions.SetRecommendations(GameData.Instance.PlayerCount, AmongUsClient.Instance.GameMode);
		}
		playerControl.RpcSyncSettings(PlayerControl.GameOptions);
	}

	private IEnumerator CoBroadcastManager()
	{
		while (!GameData.Instance)
		{
			yield return null;
		}
		int lastPlayerCount = 0;
		this.discoverState = DiscoveryState.Broadcast;
		while (this.discoverState == DiscoveryState.Broadcast)
		{
			if (lastPlayerCount != GameData.Instance.PlayerCount)
			{
				lastPlayerCount = GameData.Instance.PlayerCount;
				string data = string.Format("{0}~Open~{1}~", SaveManager.PlayerName, GameData.Instance.PlayerCount);
				DestroyableSingleton<InnerDiscover>.Instance.Interval = 1f;
				DestroyableSingleton<InnerDiscover>.Instance.StartAsServer(data);
			}
			yield return null;
		}
		DestroyableSingleton<InnerDiscover>.Instance.StopServer();
		yield break;
	}
}
