using System;
using System.Collections;
using InnerNet;
using UnityEngine;

public class TutorialManager : DestroyableSingleton<TutorialManager>
{
	public PlayerControl PlayerPrefab;

	public override void Awake()
	{
		base.Awake();
		StatsManager.Instance = new TutorialStatsManager();
		base.StartCoroutine(this.RunTutorial());
	}

	public override void OnDestroy()
	{
		StatsManager.Instance = new StatsManager();
		base.OnDestroy();
	}

	private IEnumerator RunTutorial()
	{
		while (!ShipStatus.Instance)
		{
			yield return null;
		}
		ShipStatus.Instance.Timer = 15f;
		while (!PlayerControl.LocalPlayer)
		{
			yield return null;
		}
		if (DestroyableSingleton<DiscordManager>.InstanceExists)
		{
			DestroyableSingleton<DiscordManager>.Instance.SetHowToPlay();
		}
		PlayerControl.GameOptions = new GameOptionsData
		{
			NumImpostors = 0,
			DiscussionTime = 0,
			NumEmergencyMeetings = 9
		};
		PlayerControl.LocalPlayer.RpcSetInfected(new GameData.PlayerInfo[0]);
		for (int i = 0; i < ShipStatus.Instance.DummyLocations.Length; i++)
		{
			PlayerControl playerControl = UnityEngine.Object.Instantiate<PlayerControl>(this.PlayerPrefab);
			playerControl.PlayerId = (byte)GameData.Instance.GetAvailableId();
			GameData.PlayerInfo playerInfo = GameData.Instance.AddPlayer(playerControl);
			AmongUsClient.Instance.Spawn(playerControl, -2, SpawnFlags.None);
			playerInfo.dontCensorName = true;
			playerControl.isDummy = true;
			playerControl.transform.position = ShipStatus.Instance.DummyLocations[i].position;
			playerControl.GetComponent<DummyBehaviour>().enabled = true;
			playerControl.NetTransform.enabled = false;
			playerControl.SetName(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Dummy, Array.Empty<object>()) + " " + (i + 1).ToString(), true);
			byte b = (byte)((i < (int)SaveManager.BodyColor) ? i : (i + 1));
			playerControl.SetColor((int)b);
			playerControl.SetHat(0U, (int)b);
			playerControl.SetSkin(0U);
			playerControl.SetPet(0U);
			GameData.Instance.RpcSetTasks(playerControl.PlayerId, new byte[0]);
		}
		ShipStatus.Instance.Begin();
		yield break;
	}
}
