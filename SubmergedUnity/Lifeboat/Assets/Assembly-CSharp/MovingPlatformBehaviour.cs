using System;
using System.Collections;
using Hazel;
using UnityEngine;

public class MovingPlatformBehaviour : MonoBehaviour, ISystemType
{
	public Vector3 LeftPosition;

	public Vector3 RightPosition;

	public Vector3 LeftUsePosition;

	public Vector3 RightUsePosition;

	public AudioClip MovingSound;

	private bool IsLeft;

	private PlayerControl Target;

	private byte useId;

	public bool InUse
	{
		get
		{
			return this.Target;
		}
	}

	public bool IsDirty { get; private set; }

	public void Use()
	{
		PlayerControl.LocalPlayer.RpcUsePlatform();
	}

	public void Use(PlayerControl player)
	{
		Vector3 vector = base.transform.position - player.transform.position;
		if (this.Target || vector.magnitude > 3f)
		{
			return;
		}
		this.IsDirty = true;
		base.StartCoroutine(this.UsePlatform(player));
	}

	public void Start()
	{
		this.SetSide(true);
	}

	private void SetSide(bool isLeft)
	{
		this.IsLeft = isLeft;
		base.transform.localPosition = (this.IsLeft ? this.LeftPosition : this.RightPosition);
		this.IsDirty = true;
	}

	private void SetTarget(uint playerNetId, bool isLeft)
	{
		if (this.Target)
		{
			this.MeetingCalled();
		}
		PlayerControl playerControl = AmongUsClient.Instance.FindObjectByNetId<PlayerControl>(playerNetId);
		if (!playerControl)
		{
			this.SetSide(isLeft);
			return;
		}
		base.StartCoroutine(this.UsePlatform(playerControl));
	}

	private IEnumerator UsePlatform(PlayerControl target)
	{
		this.Target = target;
		target.MyPhysics.ResetMoveState(true);
		if (target.AmOwner)
		{
			PlayerControl.HideCursorTemporarily();
		}
		target.Collider.enabled = false;
		target.moveable = false;
		target.NetTransform.enabled = false;
		Vector3 vector = this.IsLeft ? this.LeftUsePosition : this.RightUsePosition;
		Vector3 vector2 = (!this.IsLeft) ? this.LeftUsePosition : this.RightUsePosition;
		Vector3 sourcePos = this.IsLeft ? this.LeftPosition : this.RightPosition;
		Vector3 targetPos = (!this.IsLeft) ? this.LeftPosition : this.RightPosition;
		Vector3 vector3 = base.transform.parent.TransformPoint(vector);
		Vector3 worldUseTargetPos = base.transform.parent.TransformPoint(vector2);
		Vector3 worldSourcePos = base.transform.parent.TransformPoint(sourcePos);
		Vector3 worldTargetPos = base.transform.parent.TransformPoint(targetPos);
		yield return target.MyPhysics.WalkPlayerTo(vector3, 0.01f, 1f);
		yield return target.MyPhysics.WalkPlayerTo(worldSourcePos, 0.01f, 1f);
		yield return Effects.Wait(0.1f);
		target.MyPhysics.enabled = false;
		worldSourcePos -= (Vector3) target.Collider.offset;
		worldTargetPos -= (Vector3) target.Collider.offset;
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlayDynamicSound("PlatformMoving", this.MovingSound, true, new DynamicSound.GetDynamicsFunction(this.SoundDynamics), true);
		}
		this.IsLeft = !this.IsLeft;
		yield return Effects.All(new IEnumerator[]
		{
			Effects.Slide2D(base.transform, sourcePos, targetPos, target.MyPhysics.Speed),
			Effects.Slide2DWorld(target.transform, worldSourcePos, worldTargetPos, target.MyPhysics.Speed)
		});
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.StopNamedSound("PlatformMoving");
		}
		target.MyPhysics.enabled = true;
		yield return target.MyPhysics.WalkPlayerTo(worldUseTargetPos, 0.01f, 1f);
		target.CurrentPet.transform.position = target.transform.position;
		yield return Effects.Wait(0.1f);
		target.Collider.enabled = true;
		target.moveable = true;
		target.NetTransform.enabled = true;
		this.Target = null;
		yield break;
	}

	private void SoundDynamics(AudioSource source, float dt)
	{
		if (!PlayerControl.LocalPlayer)
		{
			source.volume = 0f;
			return;
		}
		Vector2 vector = base.transform.position;
		Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
		float num = Vector2.Distance(vector, truePosition);
		if (num > 6f)
		{
			source.volume = 0f;
			return;
		}
		float num2 = 1f - num / 6f;
		source.volume = Mathf.Lerp(source.volume, num2, dt);
		VibrationManager.Vibrate(0.15f, vector, 6f);
	}

	public void MeetingCalled()
	{
		base.StopAllCoroutines();
		VibrationManager.ClearAllVibration();
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.StopNamedSound("PlatformMoving");
		}
		if (this.Target)
		{
			this.Target.MyPhysics.enabled = true;
			this.Target.Collider.enabled = true;
			this.Target.moveable = true;
			this.Target.NetTransform.enabled = true;
			this.Target = null;
		}
		this.SetSide(this.IsLeft);
	}

	public void Detoriorate(float deltaTime)
	{
	}

	public void RepairDamage(PlayerControl player, byte amount)
	{
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		this.useId += 1;
		writer.Write(this.useId);
		PlayerControl target = this.Target;
		writer.Write((target != null) ? target.NetId : uint.MaxValue);
		writer.Write(this.IsLeft);
		this.IsDirty = initialState;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		if (initialState)
		{
			this.useId = reader.ReadByte();
			this.SetTarget(reader.ReadUInt32(), reader.ReadBoolean());
			return;
		}
		byte newSid = reader.ReadByte();
		if (NetHelpers.SidGreaterThan(newSid, this.useId))
		{
			this.useId = newSid;
			this.SetTarget(reader.ReadUInt32(), reader.ReadBoolean());
		}
	}
}
