using System;
using TMPro;
using UnityEngine;

public class TaskAddButton : MonoBehaviour
{
	public TextMeshPro Text;

	public SpriteRenderer Overlay;

	public Sprite CheckImage;

	public Sprite ExImage;

	public PlayerTask MyTask;

	public bool ImpostorTask;

	[HideInInspector]
	public PassiveButton Button;

	private void Awake()
	{
		this.Button = base.GetComponent<PassiveButton>();
	}

	public void Start()
	{
		if (this.ImpostorTask)
		{
			GameData.PlayerInfo data = PlayerControl.LocalPlayer.Data;
			this.Overlay.enabled = data.IsImpostor;
			this.Overlay.sprite = this.CheckImage;
			return;
		}
		PlayerTask playerTask = this.FindTaskByType();
		if (playerTask)
		{
			this.Overlay.enabled = true;
			this.Overlay.sprite = (playerTask.IsComplete ? this.CheckImage : this.ExImage);
			return;
		}
		this.Overlay.enabled = false;
	}

	public void AddTask()
	{
		if (this.ImpostorTask)
		{
			GameData.PlayerInfo data = PlayerControl.LocalPlayer.Data;
			if (data.IsImpostor)
			{
				PlayerControl.LocalPlayer.RemoveInfected();
				this.Overlay.enabled = false;
				return;
			}
			PlayerControl.LocalPlayer.RpcSetInfected(new GameData.PlayerInfo[]
			{
				data
			});
			this.Overlay.enabled = true;
			return;
		}
		else
		{
			PlayerTask playerTask = this.FindTaskByType();
			if (!playerTask)
			{
				PlayerTask playerTask2 = UnityEngine.Object.Instantiate<PlayerTask>(this.MyTask, PlayerControl.LocalPlayer.transform);
				PlayerControl.LocalPlayer.myTasks.Add(playerTask2);
				playerTask2.Id = GameData.Instance.TutOnlyAddTask(PlayerControl.LocalPlayer.PlayerId);
				playerTask2.Owner = PlayerControl.LocalPlayer;
				playerTask2.Initialize();
				this.Overlay.sprite = this.ExImage;
				this.Overlay.enabled = true;
				return;
			}
			PlayerControl.LocalPlayer.RemoveTask(playerTask);
			this.Overlay.enabled = false;
			return;
		}
	}

	private PlayerTask FindTaskByType()
	{
		for (int i = PlayerControl.LocalPlayer.myTasks.Count - 1; i > -1; i--)
		{
			PlayerTask playerTask = PlayerControl.LocalPlayer.myTasks[i];
			if (playerTask.TaskType == this.MyTask.TaskType)
			{
				if (playerTask.TaskType == TaskTypes.DivertPower)
				{
					if (((DivertPowerTask)playerTask).TargetSystem == ((DivertPowerTask)this.MyTask).TargetSystem)
					{
						return playerTask;
					}
				}
				else if (playerTask.TaskType == TaskTypes.EmptyGarbage || playerTask.TaskType == TaskTypes.RecordTemperature || playerTask.TaskType == TaskTypes.UploadData)
				{
					if (playerTask.StartAt == this.MyTask.StartAt)
					{
						return playerTask;
					}
				}
				else
				{
					if (playerTask.TaskType != TaskTypes.ActivateWeatherNodes)
					{
						return playerTask;
					}
					if (((WeatherNodeTask)playerTask).NodeId == ((WeatherNodeTask)this.MyTask).NodeId)
					{
						return playerTask;
					}
				}
			}
		}
		return null;
	}
}
