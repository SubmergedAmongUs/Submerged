using System;
using System.Collections;
using Hazel;
using UnityEngine;

public class DeconSystem : MonoBehaviour, ISystemType
{
	private const byte HeadUpCmd = 1;

	private const byte HeadDownCmd = 2;

	private const byte HeadUpInsideCmd = 3;

	private const byte HeadDownInsideCmd = 4;

	public SomeKindaDoor UpperDoor;

	public SomeKindaDoor LowerDoor;

	public float DoorOpenTime = 3f;

	public float DeconTime = 3f;

	public AudioClip SpraySound;

	public ParticleSystem[] Particles;

	public SystemTypes TargetSystem = SystemTypes.Decontamination;

	private float timer;

	public Collider2D RoomArea;

	public DecontamNumController FloorText;

	private Coroutine sprayers;

	public DeconSystem.States CurState { get; private set; }

	public bool IsDirty { get; private set; }

	public void Detoriorate(float dt)
	{
		if (this.sprayers == null && this.CurState.HasFlag(DeconSystem.States.Closed))
		{
			this.sprayers = base.StartCoroutine(this.CoRunSprayers());
		}
		int num = Mathf.CeilToInt(this.timer);
		this.timer = Mathf.Max(0f, this.timer - dt);
		int num2 = Mathf.CeilToInt(this.timer);
		if (num != num2)
		{
			if (num2 == 0)
			{
				if (this.CurState.HasFlag(DeconSystem.States.Enter))
				{
					this.CurState = ((this.CurState & ~DeconSystem.States.Enter) | DeconSystem.States.Closed);
					this.timer = this.DeconTime;
				}
				else if (this.CurState.HasFlag(DeconSystem.States.Closed))
				{
					this.CurState = ((this.CurState & ~DeconSystem.States.Closed) | DeconSystem.States.Exit);
					this.timer = this.DoorOpenTime;
				}
				else if (this.CurState.HasFlag(DeconSystem.States.Exit))
				{
					this.CurState = DeconSystem.States.Idle;
				}
			}
			this.UpdateDoorsViaState();
			this.IsDirty = true;
		}
	}

	private IEnumerator CoRunSprayers()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlayDynamicSound("DeconSpray", this.SpraySound, false, new DynamicSound.GetDynamicsFunction(this.SoundDynamics), true);
		}
		this.Particles.ForEach(delegate(ParticleSystem p)
		{
			p.Play();
		});
		yield return Effects.Wait(this.DeconTime);
		this.sprayers = null;
		yield break;
	}

	private void SoundDynamics(AudioSource source, float dt)
	{
		if (this.sprayers == null || !PlayerControl.LocalPlayer)
		{
			source.volume = 0f;
			return;
		}
		Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
		if (this.RoomArea && this.RoomArea.OverlapPoint(truePosition))
		{
			float num = this.timer / this.DeconTime;
			if ((double)num > 0.5)
			{
				source.volume = 1f - (num - 0.5f) / 0.5f;
			}
			else
			{
				source.volume = 1f;
			}
			float num2 = source.volume * 0.075f;
			VibrationManager.Vibrate(num2, num2);
			return;
		}
		source.volume = 0f;
	}

	public void OpenDoor(bool upper)
	{
		if (this.CurState == DeconSystem.States.Idle)
		{
			ShipStatus.Instance.RpcRepairSystem(this.TargetSystem, upper ? 2 : 1);
		}
	}

	public void OpenFromInside(bool upper)
	{
		if (this.CurState == DeconSystem.States.Idle)
		{
			ShipStatus.Instance.RpcRepairSystem(this.TargetSystem, upper ? 3 : 4);
		}
	}

	public void RepairDamage(PlayerControl player, byte amount)
	{
		if (this.CurState != DeconSystem.States.Idle)
		{
			return;
		}
		switch (amount)
		{
		case 1:
			this.CurState = (DeconSystem.States.Enter | DeconSystem.States.HeadingUp);
			this.timer = this.DoorOpenTime;
			break;
		case 2:
			this.CurState = DeconSystem.States.Enter;
			this.timer = this.DoorOpenTime;
			break;
		case 3:
			this.CurState = (DeconSystem.States.Exit | DeconSystem.States.HeadingUp);
			this.timer = this.DoorOpenTime;
			break;
		case 4:
			this.CurState = DeconSystem.States.Exit;
			this.timer = this.DoorOpenTime;
			break;
		}
		this.UpdateDoorsViaState();
		this.IsDirty = true;
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.Write((byte)Mathf.CeilToInt(this.timer));
		writer.Write((byte)this.CurState);
		this.IsDirty = initialState;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		this.timer = (float)reader.ReadByte();
		this.CurState = (DeconSystem.States)reader.ReadByte();
		this.UpdateDoorsViaState();
	}

	private void UpdateDoorsViaState()
	{
		int num = Mathf.CeilToInt(this.timer);
		if (this.CurState.HasFlag(DeconSystem.States.Enter))
		{
			bool flag = this.CurState.HasFlag(DeconSystem.States.HeadingUp);
			this.LowerDoor.SetDoorway(flag);
			this.UpperDoor.SetDoorway(!flag);
			if (this.FloorText)
			{
				this.FloorText.SetSecond((float)num, this.DoorOpenTime);
				return;
			}
		}
		else if (this.CurState.HasFlag(DeconSystem.States.Closed) || this.CurState == DeconSystem.States.Idle)
		{
			this.LowerDoor.SetDoorway(false);
			this.UpperDoor.SetDoorway(false);
			if (this.FloorText)
			{
				this.FloorText.SetSecond(this.DeconTime - (float)num, this.DeconTime);
				return;
			}
		}
		else if (this.CurState.HasFlag(DeconSystem.States.Exit))
		{
			bool flag2 = this.CurState.HasFlag(DeconSystem.States.HeadingUp);
			this.LowerDoor.SetDoorway(!flag2);
			this.UpperDoor.SetDoorway(flag2);
			if (this.FloorText)
			{
				this.FloorText.SetSecond((float)num, this.DoorOpenTime);
				return;
			}
		}
		else
		{
			Debug.LogWarning("What is this state: " + this.CurState.ToString());
		}
	}

	[Flags]
	public enum States : byte
	{
		Idle = 0,
		Enter = 1,
		Closed = 2,
		Exit = 4,
		HeadingUp = 8
	}
}
