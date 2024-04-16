using System;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using UnityEngine;

public class HeliSabotageSystem : MonoBehaviour, ISystemType, ICriticalSabotage, IActivatable
{
	public const float CountdownStopped = 10000f;

	public const byte TagMask = 240;

	public const byte IdMask = 15;

	public SpriteRenderer Helicopter;

	public AnimationCurve ScaleCurve;

	private const float CharlesDuration = 90f;

	private const float CodeActiveDuration = 10f;

	private const float SyncRate = 1f;

	private HashSet<HeliSabotageSystem.ActiveConsoleData> ActiveConsoles = new HashSet<HeliSabotageSystem.ActiveConsoleData>();

	private HashSet<byte> CompletedConsoles = new HashSet<byte>();

	private float codeResetTimer;

	private float syncTimer;

	private bool wasActive;

	public float Countdown { get; private set; } = 10000f;

	public bool IsActive
	{
		get
		{
			return this.CompletedConsoles.Count < 2;
		}
	}

	public float PercentActive
	{
		get
		{
			return this.codeResetTimer / 10f;
		}
	}

	public int TargetCode { get; private set; }

	public bool IsDirty { get; private set; }

	public int UserCount
	{
		get
		{
			return 0;
		}
	}

	public HeliSabotageSystem()
	{
		this.ClearSabotage();
	}

	public void ClearSabotage()
	{
		this.Countdown = 10000f;
		this.CompletedConsoles.Add(0);
		this.CompletedConsoles.Add(1);
		this.IsDirty = true;
	}

	public void Detoriorate(float deltaTime)
	{
		if (this.IsActive)
		{
			if (!this.wasActive)
			{
				this.codeResetTimer = 0f;
			}
			this.wasActive = true;
			this.UpdateHeliSize();
			this.Countdown -= deltaTime;
			this.codeResetTimer -= deltaTime;
			if (this.codeResetTimer <= 0f)
			{
				this.TargetCode = IntRange.Next(0, 99999);
				this.codeResetTimer = 10f;
				this.CompletedConsoles.Clear();
			}
			if (!PlayerTask.PlayerHasTaskOfType<ReactorTask>(PlayerControl.LocalPlayer))
			{
				PlayerControl.LocalPlayer.AddSystemTask(SystemTypes.Reactor);
			}
			this.syncTimer -= deltaTime;
			if (this.syncTimer < 0f)
			{
				this.syncTimer = 1f;
				this.IsDirty = true;
				return;
			}
		}
		else
		{
			this.wasActive = false;
			this.Helicopter.gameObject.SetActive(false);
			DestroyableSingleton<HudManager>.Instance.StopReactorFlash();
		}
	}

	private void UpdateHeliSize()
	{
		float num = this.ScaleCurve.Evaluate(1f - this.Countdown / 90f);
		this.Helicopter.gameObject.SetActive(true);
		if (num > 0.8f)
		{
			num -= 0.8f;
			float num2 = Mathf.Lerp(0.05f, 1.2f, num);
			this.Helicopter.transform.localScale = new Vector3(num2, num2, num2);
			Vector3 localPosition = this.Helicopter.transform.localPosition;
			localPosition.y = Mathf.Lerp(2.17f, 1.5f, num);
			this.Helicopter.transform.localPosition = localPosition;
			return;
		}
		this.Helicopter.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
		Vector3 localPosition2 = this.Helicopter.transform.localPosition;
		localPosition2.y = 2.17f;
		this.Helicopter.transform.localPosition = localPosition2;
	}

	internal bool IsConsoleActive(int consoleId)
	{
		return this.ActiveConsoles.Any((HeliSabotageSystem.ActiveConsoleData s) => s.ConsoleId == (byte)consoleId);
	}

	internal bool IsConsoleOkay(int consoleId)
	{
		return this.CompletedConsoles.Contains((byte)consoleId);
	}

	public void RepairDamage(PlayerControl player, byte amount)
	{
		byte b = (byte) (amount & 15);
		HeliSabotageSystem.Tags tags = (HeliSabotageSystem.Tags)(amount & 240);
		if (tags <= HeliSabotageSystem.Tags.DeactiveBit)
		{
			if (tags != HeliSabotageSystem.Tags.FixBit)
			{
				if (tags == HeliSabotageSystem.Tags.DeactiveBit)
				{
					this.ActiveConsoles.Remove(new HeliSabotageSystem.ActiveConsoleData(player.PlayerId, b));
				}
			}
			else
			{
				this.codeResetTimer = 10f;
				this.CompletedConsoles.Add(b);
			}
		}
		else if (tags != HeliSabotageSystem.Tags.ActiveBit)
		{
			if (tags == HeliSabotageSystem.Tags.DamageBit)
			{
				this.codeResetTimer = -1f;
				this.Countdown = 90f;
				this.CompletedConsoles.Clear();
				this.ActiveConsoles.Clear();
			}
		}
		else
		{
			this.ActiveConsoles.Add(new HeliSabotageSystem.ActiveConsoleData(player.PlayerId, b));
		}
		this.IsDirty = true;
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.Write(this.Countdown);
		writer.Write(this.codeResetTimer);
		writer.WritePacked(this.ActiveConsoles.Count);
		foreach (HeliSabotageSystem.ActiveConsoleData activeConsoleData in this.ActiveConsoles)
		{
			writer.Write(activeConsoleData.PlayerId);
			writer.Write(activeConsoleData.ConsoleId);
		}
		writer.WritePacked(this.CompletedConsoles.Count);
		foreach (byte b in this.CompletedConsoles)
		{
			writer.Write(b);
		}
		this.IsDirty = initialState;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		this.Countdown = reader.ReadSingle();
		this.codeResetTimer = reader.ReadSingle();
		int num = reader.ReadPackedInt32();
		this.ActiveConsoles.Clear();
		for (int i = 0; i < num; i++)
		{
			this.ActiveConsoles.Add(new HeliSabotageSystem.ActiveConsoleData(reader.ReadByte(), reader.ReadByte()));
		}
		int num2 = reader.ReadPackedInt32();
		this.CompletedConsoles.Clear();
		for (int j = 0; j < num2; j++)
		{
			this.CompletedConsoles.Add(reader.ReadByte());
		}
	}

	private struct ActiveConsoleData
	{
		public readonly byte PlayerId;

		public readonly byte ConsoleId;

		public ActiveConsoleData(byte playerId, byte consoleId)
		{
			this.PlayerId = playerId;
			this.ConsoleId = consoleId;
		}
	}

	public enum Tags
	{
		DamageBit = 128,
		ActiveBit = 64,
		DeactiveBit = 32,
		FixBit = 16
	}
}
