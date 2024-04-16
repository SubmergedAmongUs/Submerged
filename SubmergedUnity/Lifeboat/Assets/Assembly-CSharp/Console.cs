using System;
using Assets.CoreScripts;
using UnityEngine;

public class Console : MonoBehaviour, IUsable
{
	public float usableDistance = 1f;

	public int ConsoleId;

	public bool onlyFromBelow;

	public bool onlySameRoom;

	public bool checkWalls;

	public bool GhostsIgnored;

	public bool AllowImpostor;

	public SystemTypes Room;

	public TaskTypes[] TaskTypes;

	public TaskSet[] ValidTasks;

	public SpriteRenderer Image;

	public ImageNames UseIcon
	{
		get
		{
			return ImageNames.UseButton;
		}
	}

	public float UsableDistance
	{
		get
		{
			return this.usableDistance;
		}
	}

	public float PercentCool
	{
		get
		{
			return 0f;
		}
	}

	public void SetOutline(bool on, bool mainTarget)
	{
		if (this.Image)
		{
			this.Image.material.SetFloat("_Outline", (float)(on ? 1 : 0));
			this.Image.material.SetColor("_OutlineColor", Color.yellow);
			this.Image.material.SetColor("_AddColor", mainTarget ? Color.yellow : Color.clear);
		}
	}

	public float CanUse(GameData.PlayerInfo pc, out bool canUse, out bool couldUse)
	{
		float num = float.MaxValue;
		PlayerControl @object = pc.Object;
		Vector2 truePosition = @object.GetTruePosition();
		Vector3 position = base.transform.position;
		couldUse = ((!pc.IsDead || (PlayerControl.GameOptions.GhostsDoTasks && !this.GhostsIgnored)) && @object.CanMove && (this.AllowImpostor || !pc.IsImpostor) && (!this.onlySameRoom || this.InRoom(truePosition)) && (!this.onlyFromBelow || truePosition.y < position.y) && this.FindTask(@object));
		canUse = couldUse;
		if (canUse)
		{
			num = Vector2.Distance(truePosition, base.transform.position);
			canUse &= (num <= this.UsableDistance);
			if (this.checkWalls)
			{
				canUse &= !PhysicsHelpers.AnythingBetween(truePosition, position, Constants.ShadowMask, false);
			}
		}
		return num;
	}

	private bool InRoom(Vector2 truePos)
	{
		PlainShipRoom plainShipRoom = ShipStatus.Instance.FastRooms[this.Room];
		if (!plainShipRoom || !plainShipRoom.roomArea)
		{
			return false;
		}
		bool result;
		try
		{
			result = plainShipRoom.roomArea.OverlapPoint(truePos);
		}
		catch
		{
			result = false;
		}
		return result;
	}

	private PlayerTask FindTask(PlayerControl pc)
	{
		for (int i = 0; i < pc.myTasks.Count; i++)
		{
			PlayerTask playerTask = pc.myTasks[i];
			if (!playerTask.IsComplete && playerTask.ValidConsole(this))
			{
				return playerTask;
			}
		}
		return null;
	}

	public virtual void Use()
	{
		bool flag;
		bool flag2;
		this.CanUse(PlayerControl.LocalPlayer.Data, out flag, out flag2);
		if (!flag)
		{
			return;
		}
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		PlayerTask playerTask = this.FindTask(localPlayer);
		if (playerTask.MinigamePrefab)
		{
			Minigame minigame = UnityEngine.Object.Instantiate<Minigame>(playerTask.GetMinigamePrefab());
			minigame.transform.SetParent(Camera.main.transform, false);
			minigame.transform.localPosition = new Vector3(0f, 0f, -50f);
			minigame.Console = this;
			minigame.Begin(playerTask);
			DestroyableSingleton<Telemetry>.Instance.WriteUse(localPlayer.PlayerId, playerTask.TaskType, base.transform.position);
		}
	}
}
