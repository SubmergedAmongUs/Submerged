using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class NormalPlayerTask : PlayerTask
{
	public int taskStep;

	public int MaxStep;

	public bool ShowTaskStep = true;

	public bool ShowTaskTimer;

	public NormalPlayerTask.TimerState TimerStarted;

	public float TaskTimer;

	public byte[] Data;

	public ArrowBehaviour Arrow;

	protected bool arrowSuspended;

	public override int TaskStep
	{
		get
		{
			return this.taskStep;
		}
	}

	public override bool IsComplete
	{
		get
		{
			return this.taskStep >= this.MaxStep;
		}
	}

	public override void Initialize()
	{
		if (this.Arrow && !base.Owner.AmOwner)
		{
			this.Arrow.gameObject.SetActive(false);
		}
		this.HasLocation = true;
		this.LocationDirty = true;
		TaskTypes taskType = this.TaskType;
		if (taskType <= TaskTypes.EnterIdCode)
		{
			switch (taskType)
			{
			case TaskTypes.PrimeShields:
			{
				this.Data = new byte[1];
				int num = 0;
				for (int i = 0; i < 7; i++)
				{
					byte b = (byte)(1 << i);
					if (BoolRange.Next(0.7f))
					{
						byte[] data = this.Data;
						int num2 = 0;
						data[num2] |= b;
						num++;
					}
				}
				byte[] data2 = this.Data;
				int num3 = 0;
				data2[num3] &= 118;
				return;
			}
			case TaskTypes.FuelEngines:
				this.Data = new byte[2];
				return;
			case TaskTypes.ChartCourse:
				this.Data = new byte[4];
				return;
			case TaskTypes.StartReactor:
				this.Data = new byte[6];
				return;
			case TaskTypes.SwipeCard:
			case TaskTypes.ClearAsteroids:
			case TaskTypes.UploadData:
			case TaskTypes.EmptyChute:
			case TaskTypes.EmptyGarbage:
				break;
			case TaskTypes.InspectSample:
				this.Data = new byte[2];
				return;
			case TaskTypes.AlignEngineOutput:
				this.Data = new byte[4];
				this.Data[0] = (byte)(IntRange.RandomSign() * IntRange.Next(25, 127) + 127);
				this.Data[1] = (byte)(IntRange.RandomSign() * IntRange.Next(25, 127) + 127);
				return;
			case TaskTypes.FixWiring:
			{
				this.Data = new byte[this.MaxStep];
				global::Console console = this.PickRandomConsoles(TaskTypes.FixWiring, this.Data).First((global::Console v) => v.ConsoleId == (int)this.Data[0]);
				this.StartAt = console.Room;
				return;
			}
			default:
				if (taskType != TaskTypes.EnterIdCode)
				{
					return;
				}
				this.Data = BitConverter.GetBytes(IntRange.Next(1, 99999));
				return;
			}
		}
		else
		{
			if (taskType == TaskTypes.WaterPlants)
			{
				this.Data = new byte[4];
				return;
			}
			switch (taskType)
			{
			case TaskTypes.OpenWaterways:
				this.Data = new byte[3];
				return;
			case TaskTypes.ReplaceWaterJug:
				this.Data = new byte[1];
				return;
			case TaskTypes.RepairDrill:
			case TaskTypes.AlignTelescope:
			case TaskTypes.RecordTemperature:
			case TaskTypes.RebootWifi:
			case TaskTypes.PolishRuby:
			case TaskTypes.MakeBurger:
			case TaskTypes.UnlockSafe:
			case TaskTypes.PutAwayPistols:
			case TaskTypes.DressMannequin:
				break;
			case TaskTypes.ResetBreakers:
			{
				this.Data = new byte[7];
				byte b2 = 0;
				while ((int)b2 < this.Data.Length)
				{
					this.Data[(int)b2] = b2;
					b2 += 1;
				}
				this.Data.Shuffle(0);
				return;
			}
			case TaskTypes.Decontaminate:
				this.Data = new byte[1];
				this.Data[0] = IntRange.NextByte(10, 30);
				return;
			case TaskTypes.SortRecords:
				this.Data = new byte[4];
				return;
			case TaskTypes.FixShower:
			{
				float value = BoolRange.Next(0.5f) ? FloatRange.Next(0f, 0.1f) : (1f - FloatRange.Next(0f, 0.1f));
				this.Data = BitConverter.GetBytes(value);
				return;
			}
			case TaskTypes.CleanToilet:
				this.Data = new byte[1];
				this.Data[0] = IntRange.NextByte(0, 4);
				return;
			case TaskTypes.PickUpTowels:
			{
				this.Data = new byte[8];
				int[] array = Enumerable.Range(0, 14).ToArray<int>();
				array.Shuffle(0);
				byte b3 = 0;
				while ((int)b3 < this.Data.Length)
				{
					this.Data[(int)b3] = (byte)array[(int)b3];
					b3 += 1;
				}
				return;
			}
			case TaskTypes.RewindTapes:
			{
				this.Data = new byte[8];
				float num4 = (float)(IntRange.Next(6, 18) * 3600 + IntRange.Next(0, 60) * 60 + IntRange.Next(0, 60));
				BitConverter.GetBytes(num4).CopyTo(this.Data, 0);
				BitConverter.GetBytes(num4 + (float)(IntRange.RandomSign() * (IntRange.Next(5, 7) * 60 + IntRange.Next(0, 60)))).CopyTo(this.Data, 4);
				return;
			}
			case TaskTypes.StartFans:
			{
				this.Data = new byte[4];
				byte b4 = 0;
				while ((int)b4 < this.Data.Length)
				{
					this.Data[(int)b4] = IntRange.NextByte(0, 4);
					b4 += 1;
				}
				this.Data[(int)IntRange.NextByte(0, 4)] = IntRange.NextByte(1, 4);
				return;
			}
			default:
			{
				if (taskType != TaskTypes.VentCleaning)
				{
					return;
				}
				byte[] consoleIds = new byte[this.MaxStep];
				global::Console console2 = this.PickRandomConsoles(TaskTypes.VentCleaning, consoleIds).First((global::Console v) => v.ConsoleId == (int)consoleIds[0]);
				this.StartAt = console2.Room;
				this.Data = new byte[1 + consoleIds.Length];
				consoleIds.CopyTo(this.Data, 0);
				int num5 = IntRange.Next(4, 8);
				this.Data[this.MaxStep] = (byte)num5;
				break;
			}
			}
		}
	}

	public void NextStep()
	{
		this.taskStep++;
		this.UpdateArrow();
		if (this.taskStep >= this.MaxStep)
		{
			this.taskStep = this.MaxStep;
			if (PlayerControl.LocalPlayer)
			{
				if (DestroyableSingleton<HudManager>.InstanceExists)
				{
					DestroyableSingleton<HudManager>.Instance.ShowTaskComplete();
					StatsManager instance = StatsManager.Instance;
					uint num = instance.TasksCompleted;
					instance.TasksCompleted = num + 1U;
					DestroyableSingleton<AchievementManager>.Instance.OnTaskComplete(this.TaskType);
					if (PlayerTask.AllTasksCompleted(PlayerControl.LocalPlayer))
					{
						StatsManager instance2 = StatsManager.Instance;
						num = instance2.CompletedAllTasks;
						instance2.CompletedAllTasks = num + 1U;
					}
				}
				PlayerControl.LocalPlayer.RpcCompleteTask(base.Id);
				return;
			}
		}
		else if (this.ShowTaskStep && Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(DestroyableSingleton<HudManager>.Instance.TaskUpdateSound, false, 1f);
		}
	}

	public virtual void UpdateArrow()
	{
		if (!this.Arrow)
		{
			return;
		}
		if (!base.Owner.AmOwner)
		{
			this.Arrow.gameObject.SetActive(false);
			return;
		}
		if (!this.IsComplete)
		{
			if (PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
			{
				this.arrowSuspended = true;
			}
			else
			{
				this.Arrow.gameObject.SetActive(true);
			}
			if (this.TaskType == TaskTypes.FixWiring)
			{
				global::Console console4 = base.FindSpecialConsole((global::Console c) => c.TaskTypes.Contains(TaskTypes.FixWiring) && c.ConsoleId == (int)this.Data[this.taskStep]);
				this.Arrow.target = console4.transform.position;
				this.StartAt = console4.Room;
			}
			else if (this.TaskType == TaskTypes.AlignEngineOutput)
			{
				if (AlignGame.IsSuccess(this.Data[2]))
				{
					this.Arrow.target = base.FindSpecialConsole((global::Console c) => c.TaskTypes.Contains(TaskTypes.AlignEngineOutput) && c.ConsoleId == 0).transform.position;
					this.StartAt = SystemTypes.UpperEngine;
				}
				else
				{
					this.Arrow.target = base.FindSpecialConsole((global::Console console) => console.TaskTypes.Contains(TaskTypes.AlignEngineOutput) && console.ConsoleId == 1).transform.position;
					this.StartAt = SystemTypes.LowerEngine;
				}
			}
			else if (this.TaskType == TaskTypes.StartFans)
			{
				global::Console console2 = base.FindSpecialConsole((global::Console c) => this.ValidConsole(c) && c.ConsoleId == 1);
				this.Arrow.target = console2.transform.position;
				this.StartAt = console2.Room;
			}
			else
			{
				global::Console console3 = base.FindObjectPos();
				if (console3)
				{
					this.Arrow.target = console3.transform.position;
					this.StartAt = console3.Room;
				}
			}
			this.LocationDirty = true;
			return;
		}
		this.Arrow.gameObject.SetActive(false);
	}

	protected virtual void FixedUpdate()
	{
		if (this.TimerStarted == NormalPlayerTask.TimerState.Started)
		{
			this.TaskTimer -= Time.deltaTime;
			if (this.TaskTimer <= 0f)
			{
				this.TaskTimer = 0f;
				this.TimerStarted = NormalPlayerTask.TimerState.Finished;
			}
		}
		if (this.Arrow)
		{
			if (this.Arrow.isActiveAndEnabled && PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
			{
				this.arrowSuspended = true;
				this.Arrow.gameObject.SetActive(false);
				return;
			}
			if (this.arrowSuspended && !PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
			{
				this.arrowSuspended = false;
				this.Arrow.gameObject.SetActive(true);
			}
		}
	}

	public override bool ValidConsole(global::Console console)
	{
		if (this.TaskType == TaskTypes.ResetBreakers)
		{
			if (!console.TaskTypes.Contains(TaskTypes.ResetBreakers))
			{
				return false;
			}
			int num = Array.IndexOf<byte>(this.Data, (byte)console.ConsoleId);
			return this.taskStep <= num;
		}
		else if (this.TaskType == TaskTypes.SortRecords)
		{
			if (!console.TaskTypes.Contains(this.TaskType))
			{
				return false;
			}
			int num2 = this.Data.IndexOf((byte b) => b != 0 && b != byte.MaxValue);
			if (num2 != -1)
			{
				return console.ConsoleId == (int)this.Data[num2];
			}
			return console.ConsoleId == 0;
		}
		else
		{
			if (this.TaskType == TaskTypes.CleanToilet)
			{
				return console.TaskTypes.Contains(this.TaskType) && console.ConsoleId == (int)this.Data[0];
			}
			if (this.TaskType == TaskTypes.EmptyGarbage)
			{
				return console.ValidTasks.Any((TaskSet set) => this.TaskType == set.taskType && set.taskStep.Contains(this.taskStep)) && ((this.taskStep == 0 && console.Room == this.StartAt) || this.taskStep == 1);
			}
			if (this.TaskType == TaskTypes.FixWiring)
			{
				return console.TaskTypes.Contains(this.TaskType) && console.ConsoleId == (int)this.Data[this.taskStep];
			}
			if (this.TaskType == TaskTypes.VentCleaning)
			{
				return console.TaskTypes.Contains(this.TaskType) && console.ConsoleId == (int)this.Data[this.taskStep];
			}
			if (this.TaskType == TaskTypes.AlignEngineOutput)
			{
				return console.TaskTypes.Contains(this.TaskType) && console.ConsoleId == this.taskStep;
			}
			if (this.TaskType == TaskTypes.FuelEngines)
			{
				return console.ValidTasks.Any((TaskSet set) => this.TaskType == set.taskType && set.taskStep.Contains((int)this.Data[1]));
			}
			if (this.TaskType == TaskTypes.RecordTemperature)
			{
				return console.Room == this.StartAt && console.TaskTypes.Any((TaskTypes tt) => tt == this.TaskType);
			}
			return console.TaskTypes.Any((TaskTypes tt) => tt == this.TaskType) || console.ValidTasks.Any((TaskSet set) => this.TaskType == set.taskType && set.taskStep.Contains(this.taskStep));
		}
	}

	public override void Complete()
	{
		this.taskStep = this.MaxStep;
	}

	public override void AppendTaskText(StringBuilder sb)
	{
		bool flag = this.ShouldYellowText();
		if (flag)
		{
			if (this.IsComplete)
			{
				sb.Append("<color=#00DD00FF>");
			}
			else
			{
				sb.Append("<color=#FFFF00FF>");
			}
		}
		sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(this.StartAt));
		sb.Append(": ");
		sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(this.TaskType));
		if (this.ShowTaskTimer && this.TimerStarted == NormalPlayerTask.TimerState.Started)
		{
			sb.Append(" (");
			sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.SecondsAbbv, new object[]
			{
				(int)this.TaskTimer
			}));
			sb.Append(")");
		}
		else if (this.ShowTaskStep)
		{
			sb.Append(" (");
			sb.Append(this.taskStep);
			sb.Append("/");
			sb.Append(this.MaxStep);
			sb.Append(")");
		}
		if (flag)
		{
			sb.Append("</color>");
		}
		sb.AppendLine();
	}

	private bool ShouldYellowText()
	{
		return (this.TaskType == TaskTypes.FuelEngines && this.Data[1] > 0) || this.taskStep > 0 || this.TimerStarted > NormalPlayerTask.TimerState.NotStarted;
	}

	private List<global::Console> PickRandomConsoles(TaskTypes taskType, byte[] consoleIds)
	{
		List<global::Console> list = (from t in ShipStatus.Instance.AllConsoles
		where t.TaskTypes.Contains(taskType)
		select t).ToList<global::Console>();
		List<global::Console> list2 = new List<global::Console>(list);
		for (int i = 0; i < consoleIds.Length; i++)
		{
			int index = list2.RandomIdx<global::Console>();
			consoleIds[i] = (byte)list2[index].ConsoleId;
			list2.RemoveAt(index);
		}
		Array.Sort<byte>(this.Data);
		return list;
	}

	public enum TimerState
	{
		NotStarted,
		Started,
		Finished
	}
}
