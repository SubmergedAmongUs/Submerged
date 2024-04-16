using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

public class AirshipStatus : ShipStatus
{
	public Ladder[] Ladders;

	public SpawnInMinigame SpawnInGame;

	public MovingPlatformBehaviour GapPlatform;

	public ParticleSystem ShowerParticles;

	public LightAffector[] LightAffectors;

	protected override void OnEnable()
	{
		if (this.Systems != null)
		{
			return;
		}
		this.Ladders = base.GetComponentsInChildren<Ladder>();
		ElectricalDoors componentInChildren = base.GetComponentInChildren<ElectricalDoors>();
		if (AmongUsClient.Instance.AmHost)
		{
			componentInChildren.Initialize();
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
			},
			{
				SystemTypes.Doors,
				new DoorsSystemType()
			},
			{
				SystemTypes.Comms,
				new HudOverrideSystemType()
			},
			{
				SystemTypes.GapRoom,
				base.GetComponentInChildren<MovingPlatformBehaviour>()
			},
			{
				SystemTypes.Reactor,
				base.GetComponentInChildren<HeliSabotageSystem>()
			},
			{
				SystemTypes.Decontamination,
				componentInChildren
			},
			{
				SystemTypes.Decontamination2,
				new AutoDoorsSystemType()
			},
			{
				SystemTypes.Security,
				new SecurityCameraSystemType()
			},
			{
				SystemTypes.Ventilation,
				new VentilationSystem()
			}
		};
		Camera main = Camera.main;
		main.backgroundColor = this.CameraColor;
		FollowerCamera component = main.GetComponent<FollowerCamera>();
		component.shakeAmount = 0f;
		component.shakePeriod = 0f;
		this.Systems.Add(SystemTypes.Sabotage, new SabotageSystemType((from i in this.Systems.Values
		where i is IActivatable
		select i).Cast<IActivatable>().ToArray<IActivatable>()));
	}

	public override void RepairGameOverSystems()
	{
		(ShipStatus.Instance.Systems[SystemTypes.Reactor] as HeliSabotageSystem).ClearSabotage();
	}

	public override float CalculateLightRadius(GameData.PlayerInfo player)
	{
		float num = base.CalculateLightRadius(player);
		if (!player.IsImpostor)
		{
			foreach (LightAffector lightAffector in this.LightAffectors)
			{
				if (player.Object && player.Object.Collider.IsTouching(lightAffector.Hitbox))
				{
					num *= lightAffector.Multiplier;
				}
			}
		}
		return num;
	}

	public override void OnMeetingCalled()
	{
		this.GapPlatform.MeetingCalled();
	}

	public override void SpawnPlayer(PlayerControl player, int numPlayers, bool initialSpawn)
	{
		if (DestroyableSingleton<TutorialManager>.InstanceExists)
		{
			player.NetTransform.SnapTo(new Vector2(-0.66f, -0.5f));
			return;
		}
		if (initialSpawn)
		{
			player.NetTransform.SnapTo(this.InitialSpawnCenter);
			return;
		}
		player.NetTransform.Halt();
	}

	public override IEnumerator PrespawnStep()
	{
		SpawnInMinigame spawnInMinigame = UnityEngine.Object.Instantiate<SpawnInMinigame>(this.SpawnInGame, Camera.main!.transform, false);
		spawnInMinigame.transform.localPosition = new Vector3(0f, 0f, -600f);
		spawnInMinigame.Begin(null);
		yield return spawnInMinigame.WaitForFinish();
		yield break;
	}
}
