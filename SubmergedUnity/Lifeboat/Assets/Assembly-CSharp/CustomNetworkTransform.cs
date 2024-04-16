using System;
using Hazel;
using InnerNet;
using UnityEngine;

[DisallowMultipleComponent]
public class CustomNetworkTransform : InnerNetObject
{
	private const float LocalMovementThreshold = 0.0001f;

	private const float LocalVelocityThreshold = 0.0001f;

	private const float MoveAheadRatio = 0.1f;

	private readonly FloatRange XRange = new FloatRange(-50f, 50f);

	private readonly FloatRange YRange = new FloatRange(-50f, 50f);

	[SerializeField]
	private float sendInterval = 0.1f;

	[SerializeField]
	private float snapThreshold = 5f;

	[SerializeField]
	private float interpolateMovement = 1f;

	private Rigidbody2D body;

	private Vector2 targetSyncPosition;

	private Vector2 targetSyncVelocity;

	private ushort lastSequenceId;

	private Vector2 prevPosSent;

	private Vector2 prevVelSent;

	private void Awake()
	{
		this.body = base.GetComponent<Rigidbody2D>();
		this.targetSyncPosition = (this.prevPosSent = base.transform.position);
		this.targetSyncVelocity = (this.prevVelSent = Vector2.zero);
	}

	public void OnEnable()
	{
		DummyBehaviour component = base.GetComponent<DummyBehaviour>();
		if (component && component.enabled)
		{
			base.enabled = false;
		}
		base.SetDirtyBit(3U);
	}

	public void Halt()
	{
		ushort minSid = (ushort) (this.lastSequenceId + 1);
		this.SnapTo(base.transform.position, minSid);
	}

	public void RpcSnapTo(Vector2 position)
	{
		ushort minSid = (ushort) (this.lastSequenceId + 5);
		if (AmongUsClient.Instance.AmClient)
		{
			this.SnapTo(position, minSid);
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(this.NetId, 21, SendOption.Reliable);
		this.WriteVector2(position, messageWriter);
		messageWriter.Write(this.lastSequenceId);
		messageWriter.EndMessage();
	}

	public void SnapTo(Vector2 position)
	{
		ushort minSid = (ushort) (this.lastSequenceId + 3);
		this.SnapTo(position, minSid);
	}

	private void SnapTo(Vector2 position, ushort minSid)
	{
		if (!NetHelpers.SidGreaterThan(minSid, this.lastSequenceId))
		{
			return;
		}
		this.lastSequenceId = minSid;
		Transform transform = base.transform;
		this.body.position = position;
		this.targetSyncPosition = position;
		transform.position = position;
		this.targetSyncVelocity = (this.body.velocity = Vector2.zero);
		this.prevPosSent = position;
		this.prevVelSent = Vector2.zero;
	}

	private void FixedUpdate()
	{
		if (base.AmOwner)
		{
			if (this.HasMoved())
			{
				base.SetDirtyBit(3U);
				return;
			}
		}
		else
		{
			if (this.interpolateMovement != 0f)
			{
				Vector2 vector = this.targetSyncPosition - this.body.position;
				if (vector.sqrMagnitude >= 0.0001f)
				{
					float num = this.interpolateMovement / this.sendInterval;
					vector.x *= num;
					vector.y *= num;
					if (PlayerControl.LocalPlayer)
					{
						vector = Vector2.ClampMagnitude(vector, PlayerControl.LocalPlayer.MyPhysics.TrueSpeed);
					}
					this.body.velocity = vector;
				}
				else
				{
					this.body.velocity = Vector2.zero;
				}
			}
			this.targetSyncPosition += this.targetSyncVelocity * Time.fixedDeltaTime * 0.1f;
		}
	}

	private bool HasMoved()
	{
		float num;
		if (this.body != null)
		{
			num = Vector2.Distance(this.body.position, this.prevPosSent);
		}
		else
		{
			num = Vector2.Distance(base.transform.position, this.prevPosSent);
		}
		if (num > 0.0001f)
		{
			return true;
		}
		if (this.body != null)
		{
			num = Vector2.Distance(this.body.velocity, this.prevVelSent);
		}
		return num > 0.0001f;
	}

	public override void HandleRpc(byte callId, MessageReader reader)
	{
		if (!base.isActiveAndEnabled)
		{
			return;
		}
		if (callId == 21)
		{
			Vector2 position = this.ReadVector2(reader);
			ushort minSid = reader.ReadUInt16();
			this.SnapTo(position, minSid);
		}
	}

	public override bool Serialize(MessageWriter writer, bool initialState)
	{
		if (initialState)
		{
			writer.Write(this.lastSequenceId);
			this.WriteVector2(this.body.position, writer);
			this.WriteVector2(this.body.velocity, writer);
			return true;
		}
		if (!base.isActiveAndEnabled)
		{
			base.ClearDirtyBits();
			return false;
		}
		this.lastSequenceId += 1;
		writer.Write(this.lastSequenceId);
		this.WriteVector2(this.body.position, writer);
		this.WriteVector2(this.body.velocity, writer);
		this.prevPosSent = this.body.position;
		this.prevVelSent = this.body.velocity;
		this.DirtyBits -= 1U;
		return true;
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
		if (initialState)
		{
			this.lastSequenceId = reader.ReadUInt16();
			this.targetSyncPosition = (base.transform.position = this.ReadVector2(reader));
			this.targetSyncVelocity = this.ReadVector2(reader);
			return;
		}
		if (base.AmOwner)
		{
			return;
		}
		ushort newSid = reader.ReadUInt16();
		if (!NetHelpers.SidGreaterThan(newSid, this.lastSequenceId))
		{
			return;
		}
		this.lastSequenceId = newSid;
		if (!base.isActiveAndEnabled)
		{
			return;
		}
		this.targetSyncPosition = this.ReadVector2(reader);
		this.targetSyncVelocity = this.ReadVector2(reader);
		if (Vector2.Distance(this.body.position, this.targetSyncPosition) > this.snapThreshold)
		{
			if (this.body)
			{
				this.body.position = this.targetSyncPosition;
				this.body.velocity = this.targetSyncVelocity;
			}
			else
			{
				base.transform.position = this.targetSyncPosition;
			}
		}
		if (this.interpolateMovement == 0f && this.body)
		{
			this.body.position = this.targetSyncPosition;
		}
	}

	private void WriteVector2(Vector2 vec, MessageWriter writer)
	{
		ushort num = (ushort)(this.XRange.ReverseLerp(vec.x) * 65535f);
		ushort num2 = (ushort)(this.YRange.ReverseLerp(vec.y) * 65535f);
		writer.Write(num);
		writer.Write(num2);
	}

	private Vector2 ReadVector2(MessageReader reader)
	{
		float v = (float)reader.ReadUInt16() / 65535f;
		float v2 = (float)reader.ReadUInt16() / 65535f;
		return new Vector2(this.XRange.Lerp(v), this.YRange.Lerp(v2));
	}
}
