using System;
using System.Collections;
using Discord;
using InnerNet;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DiscordManager : DestroyableSingleton<DiscordManager>
{
	private const long ClientId = 477175586805252107L;

	private const string DeeplinkScheme = "amongus";

	private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	public GenericPopup discordPopup;

	[NonSerialized]
	private Discord.Discord presence;

	private DateTime? StartTime;

	public void Start()
	{
		if (DestroyableSingleton<DiscordManager>.Instance != this)
		{
			return;
		}
		try
		{
			this.presence = new Discord.Discord(477175586805252107L, 1UL);
			ActivityManager activityManager = this.presence.GetActivityManager();
			activityManager.RegisterSteam(945360U);
			activityManager.OnActivityJoin += new ActivityManager.ActivityJoinHandler(this.HandleJoinRequest);
			this.SetInMenus();
			SceneManager.sceneLoaded += delegate(Scene scene, LoadSceneMode mode)
			{
				this.OnSceneChange(scene.name);
			};
		}
		catch
		{
			Debug.LogWarning("DiscordManager: Discord messed up");
		}
	}

	private void OnSceneChange(string name)
	{
		if (name != null && (name == "MatchMaking" || name == "MMOnline" || name == "MainMenu"))
		{
			this.SetInMenus();
		}
	}

	public void FixedUpdate()
	{
		if (this.presence == null)
		{
			return;
		}
		try
		{
			this.presence.RunCallbacks();
		}
		catch (ResultException)
		{
			this.presence.Dispose();
			this.presence = null;
		}
	}

	public bool HasValidPartyID()
	{
		return false;
	}

	public bool CanLoginWithDiscord()
	{
		return false;
	}

	public void LoginWithDiscord()
	{
		Debug.Log("DiscordManager: Logging in with discord");
	}

	public void Logout()
	{
	}

	public void SetInMenus()
	{
		this.StartTime = null;
		if (this.presence == null)
		{
			return;
		}
		this.ClearPresence();
		Activity activity = default(Activity);
		activity.State = "In Menus";
		activity.Assets.LargeImage = "icon";
		this.presence.GetActivityManager().UpdateActivity(activity, delegate(Result r)
		{
		});
	}

	public void SetPlayingGame()
	{
		if (this.StartTime == null)
		{
			this.StartTime = new DateTime?(DateTime.UtcNow);
		}
		if (this.presence == null)
		{
			return;
		}
		Activity activity = default(Activity);
		activity.State = "In Game";
		activity.Details = "Playing";
		activity.Assets.LargeImage = "icon";
		activity.Timestamps.Start = DiscordManager.ToUnixTime(this.StartTime.Value);
		this.presence.GetActivityManager().UpdateActivity(activity, delegate(Result r)
		{
		});
	}

	public void SetHowToPlay()
	{
		if (this.presence == null)
		{
			return;
		}
		this.ClearPresence();
		Activity activity = default(Activity);
		activity.State = "In Freeplay";
		activity.Assets.LargeImage = "icon";
		this.presence.GetActivityManager().UpdateActivity(activity, delegate(Result r)
		{
		});
	}

	public void SetInLobbyClient(int numPlayers, int maxPlayers, int gameId)
	{
		if (this.StartTime == null)
		{
			this.StartTime = new DateTime?(DateTime.UtcNow);
		}
		string id = GameCode.IntToGameName(gameId);
		if (this.presence == null)
		{
			return;
		}
		this.ClearPresence();
		Activity activity = default(Activity);
		activity.State = "In Lobby";
		activity.Assets.LargeImage = "icon";
		activity.Timestamps.Start = DiscordManager.ToUnixTime(this.StartTime.Value);
		activity.Party.Size.CurrentSize = numPlayers;
		activity.Party.Size.MaxSize = maxPlayers;
		activity.Party.Id = id;
		this.presence.GetActivityManager().UpdateActivity(activity, delegate(Result r)
		{
		});
	}

	private void ClearPresence()
	{
		if (this.presence == null)
		{
			return;
		}
		this.presence.GetActivityManager().ClearActivity(delegate(Result r)
		{
		});
	}

	public void SetInLobbyHost(int numPlayers, int maxPlayers, int gameId)
	{
		if (this.StartTime == null)
		{
			this.StartTime = new DateTime?(DateTime.UtcNow);
		}
		string text = GameCode.IntToGameName(gameId);
		if (this.presence == null)
		{
			return;
		}
		Activity activity = default(Activity);
		activity.State = "In Lobby";
		activity.Details = "Hosting a game";
		activity.Assets.LargeImage = "icon";
		activity.Assets.LargeText = "Ask to play!";
		activity.Party.Size.CurrentSize = numPlayers;
		activity.Party.Size.MaxSize = maxPlayers;
		activity.Secrets.Join = "join" + DiscordManager.ReverseString(text);
		activity.Secrets.Match = "match" + DiscordManager.ReverseString(text);
		activity.Party.Id = text;
		
		// activity. = 7U;
		this.presence.GetActivityManager().UpdateActivity(activity, delegate(Result r)
		{
		});
	}

	public bool CanShareGameOnDiscord()
	{
		return false;
	}

	public void ShareGameOnDiscord()
	{
	}

	private void HandleJoinRequest(string joinSecret)
	{
		if (!joinSecret.StartsWith("join"))
		{
			Debug.LogWarning("DiscordManager: Invalid join secret: " + joinSecret);
			return;
		}
		base.StopAllCoroutines();
		base.StartCoroutine(this.CoJoinGame(joinSecret));
	}

	private IEnumerator CoJoinGame(string joinSecret)
	{
		while (!DestroyableSingleton<EOSManager>.InstanceExists)
		{
			yield return null;
		}
		yield return DestroyableSingleton<EOSManager>.Instance.WaitForLoginFlow();
		while (!DestroyableSingleton<ServerManager>.InstanceExists)
		{
			yield return null;
		}
		yield return DestroyableSingleton<ServerManager>.Instance.WaitForServers();
		while (!AmongUsClient.Instance)
		{
			yield return null;
		}
		if (AmongUsClient.Instance.mode != MatchMakerModes.None)
		{
			Debug.LogWarning("DiscordManager: Already connected");
			yield break;
		}
		Debug.Log("DiscordManager: Joining game: " + joinSecret);
		AmongUsClient.Instance.GameMode = GameModes.OnlineGame;
		AmongUsClient.Instance.GameId = GameCode.GameNameToInt(DiscordManager.ReverseString(joinSecret.Substring(4)));
		AmongUsClient.Instance.SetEndpoint(DestroyableSingleton<ServerManager>.Instance.OnlineNetAddress, 22023);
		AmongUsClient.Instance.MainMenuScene = "MMOnline";
		AmongUsClient.Instance.OnlineScene = "OnlineGame";
		AmongUsClient.Instance.Connect(MatchMakerModes.Client);
		yield return AmongUsClient.Instance.WaitForConnectionOrFail();
		if (AmongUsClient.Instance.ClientId < 0)
		{
			SceneManager.LoadScene("MMOnline");
		}
		yield break;
	}

	public void RequestRespondYes(long userId)
	{
		if (this.presence == null)
		{
			return;
		}
		this.presence.GetActivityManager().SendRequestReply(userId, ActivityJoinRequestReply.Yes, delegate(Result r)
		{
		});
	}

	public void RequestRespondNo(long userId)
	{
		if (this.presence == null)
		{
			return;
		}
		Debug.Log("DiscordManager: responding no to Ask to Join request");
		this.presence.GetActivityManager().SendRequestReply(userId, 0, delegate(Result r)
		{
		});
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (this.presence != null)
		{
			this.presence.Dispose();
		}
	}

	private static string ReverseString(string source)
	{
		char[] array = source.ToCharArray();
		Array.Reverse(array);
		return new string(array);
	}

	private static long ToUnixTime(DateTime time)
	{
		return (long)(time - DiscordManager.epoch).TotalSeconds;
	}
}
