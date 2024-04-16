using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public abstract class PlayerTask : MonoBehaviour
{
	public SystemTypes StartAt;

	public TaskTypes TaskType;

	public Minigame MinigamePrefab;

	public bool HasLocation;

	public bool LocationDirty = true;

	public int Index { get; internal set; }

	public uint Id { get; internal set; }

	public PlayerControl Owner { get; internal set; }

	public abstract int TaskStep { get; }

	public abstract bool IsComplete { get; }

	public List<Vector2> Locations
	{
		get
		{
			this.LocationDirty = false;
			return this.FindConsolesPos();
		}
	}

	public abstract void Initialize();

	public virtual void OnRemove()
	{
	}

	public abstract bool ValidConsole(global::Console console);

	public abstract void Complete();

	public abstract void AppendTaskText(StringBuilder sb);

	internal static bool TaskIsEmergency(PlayerTask arg)
	{
		return arg is NoOxyTask || arg is IHudOverrideTask || arg is ReactorTask || arg is ElectricTask;
	}

	protected List<global::Console> FindConsoles()
	{
		List<global::Console> list = new List<global::Console>();
		global::Console[] allConsoles = ShipStatus.Instance.AllConsoles;
		for (int i = 0; i < allConsoles.Length; i++)
		{
			if (this.ValidConsole(allConsoles[i]))
			{
				list.Add(allConsoles[i]);
			}
		}
		return list;
	}

	public static bool PlayerHasTaskOfType<T>(PlayerControl localPlayer)
	{
		if (!localPlayer)
		{
			return true;
		}
		for (int i = 0; i < localPlayer.myTasks.Count; i++)
		{
			if (localPlayer.myTasks[i] is T)
			{
				return true;
			}
		}
		return false;
	}

	protected List<Vector2> FindValidConsolesPositions()
	{
		List<Vector2> list = new List<Vector2>();
		global::Console[] allConsoles = ShipStatus.Instance.AllConsoles;
		for (int i = 0; i < allConsoles.Length; i++)
		{
			if (this.ValidConsole(allConsoles[i]))
			{
				list.Add(allConsoles[i].transform.position);
			}
		}
		return list;
	}

	protected global::Console FindSpecialConsole(Func<global::Console, bool> func)
	{
		global::Console[] allConsoles = ShipStatus.Instance.AllConsoles;
		for (int i = 0; i < allConsoles.Length; i++)
		{
			if (func(allConsoles[i]))
			{
				return allConsoles[i];
			}
		}
		return null;
	}

	protected global::Console FindObjectPos()
	{
		global::Console[] allConsoles = ShipStatus.Instance.AllConsoles;
		for (int i = 0; i < allConsoles.Length; i++)
		{
			if (this.ValidConsole(allConsoles[i]))
			{
				return allConsoles[i];
			}
		}
		Debug.LogError("Couldn't find location for task: " + base.name);
		return null;
	}

	protected List<Vector2> FindConsolesPos()
	{
		List<Vector2> list = this.FindValidConsolesPositions();
		if (list.Count == 0)
		{
			return null;
		}
		return list;
	}

	public virtual Minigame GetMinigamePrefab()
	{
		return this.MinigamePrefab;
	}

	protected static bool AllTasksCompleted(PlayerControl player)
	{
		for (int i = 0; i < player.myTasks.Count; i++)
		{
			PlayerTask playerTask = player.myTasks[i];
			if (playerTask is NormalPlayerTask && !playerTask.IsComplete)
			{
				return false;
			}
		}
		return true;
	}
}
