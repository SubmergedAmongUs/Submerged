using System;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using UnityEngine;

public class VentilationSystem : ISystemType
{
	private const ushort OP_ID_START = 0;

	private readonly Dictionary<byte, SequenceBuffer<VentilationSystem.VentMoveInfo>> SeqBuffers = new Dictionary<byte, SequenceBuffer<VentilationSystem.VentMoveInfo>>();

	private readonly Dictionary<byte, byte> PlayersCleaningVents = new Dictionary<byte, byte>();

	private readonly Dictionary<byte, byte> PlayersInsideVents = new Dictionary<byte, byte>();

	private readonly HashSet<byte> ToRemove = new HashSet<byte>();

	public bool IsDirty { get; private set; }

	public static void Update(VentilationSystem.Operation op, int ventId)
	{
		VentilationSystem ventilationSystem = ShipStatus.Instance.Systems[SystemTypes.Ventilation] as VentilationSystem;
		if (ventilationSystem == null)
		{
			return;
		}
		byte playerId = PlayerControl.LocalPlayer.PlayerId;
		SequenceBuffer<VentilationSystem.VentMoveInfo> valueOrSetDefault = ventilationSystem.SeqBuffers.GetValueOrSetDefault(playerId, () => new SequenceBuffer<VentilationSystem.VentMoveInfo>(0));
		ushort num = (ushort) (valueOrSetDefault.LastSid + 1);
		MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
		messageWriter.Write(num);
		messageWriter.Write((byte)op);
		messageWriter.Write((byte)ventId);
		ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Ventilation, messageWriter);
		messageWriter.Recycle();
		valueOrSetDefault.LastSid = num;
	}

	public void Detoriorate(float deltaTime)
	{
		if (GameData.Instance)
		{
			foreach (KeyValuePair<byte, byte> keyValuePair in this.PlayersCleaningVents)
			{
				byte key = keyValuePair.Key;
				GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(key);
				if (playerById == null || playerById.Disconnected)
				{
					this.ToRemove.Add(key);
				}
			}
			if (this.ToRemove.Count > 0)
			{
				foreach (byte key2 in this.ToRemove)
				{
					this.PlayersCleaningVents.Remove(key2);
					this.PlayersInsideVents.Remove(key2);
				}
				this.ToRemove.Clear();
				this.UpdateVentArrows();
				this.IsDirty = true;
			}
		}
	}

	public bool IsVentCurrentlyBeingCleaned(int id)
	{
		foreach (KeyValuePair<byte, byte> keyValuePair in this.PlayersCleaningVents)
		{
			if ((int)keyValuePair.Value == id)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsImpostorInsideVent(int ventId)
	{
		foreach (KeyValuePair<byte, byte> keyValuePair in this.PlayersInsideVents)
		{
			if ((int)keyValuePair.Value == ventId)
			{
				return true;
			}
		}
		return false;
	}

	public void RepairDamage(PlayerControl player, byte opCode)
	{
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		ushort opId = msgReader.ReadUInt16();
		VentilationSystem.Operation operation = (VentilationSystem.Operation)msgReader.ReadByte();
		byte b = msgReader.ReadByte();
		byte playerId = player.PlayerId;
		SequenceBuffer<VentilationSystem.VentMoveInfo> valueOrSetDefault = this.SeqBuffers.GetValueOrSetDefault(playerId, () => new SequenceBuffer<VentilationSystem.VentMoveInfo>((ushort) (opId - 1)));
		if (valueOrSetDefault.IsInvalidSid(opId))
		{
			Debug.LogError(string.Format("Invalid Sid {0} received; Op {1} on vent {2} by player {3}. Last Sid was: {4}", new object[]
			{
				opId,
				operation,
				b,
				playerId,
				valueOrSetDefault.LastSid
			}));
			return;
		}
		if (valueOrSetDefault.IsNextSid(opId))
		{
			this.PerformVentOp(playerId, operation, b, valueOrSetDefault);
		}
		else
		{
			valueOrSetDefault.Add(opId, new VentilationSystem.VentMoveInfo
			{
				Op = operation,
				VentId = b,
				PlayerId = playerId
			});
		}
		foreach (VentilationSystem.VentMoveInfo ventMoveInfo in valueOrSetDefault.SubsequentObjs())
		{
			this.PerformVentOp(ventMoveInfo.PlayerId, ventMoveInfo.Op, ventMoveInfo.VentId, valueOrSetDefault);
		}
	}

	private void PerformVentOp(byte playerId, VentilationSystem.Operation op, byte ventId, SequenceBuffer<VentilationSystem.VentMoveInfo> seqBuffer)
	{
		seqBuffer.BumpSid();
		switch (op)
		{
		case VentilationSystem.Operation.StartCleaning:
			this.BootImpostorsFromVent((int)ventId);
			this.PlayersCleaningVents[playerId] = ventId;
			break;
		case VentilationSystem.Operation.StopCleaning:
			foreach (KeyValuePair<byte, byte> keyValuePair in this.PlayersCleaningVents.ToArray<KeyValuePair<byte, byte>>())
			{
				if (keyValuePair.Value == ventId)
				{
					this.PlayersCleaningVents.Remove(keyValuePair.Key);
				}
			}
			break;
		case VentilationSystem.Operation.Enter:
			this.PlayersInsideVents[playerId] = ventId;
			break;
		case VentilationSystem.Operation.Exit:
			this.PlayersInsideVents.Remove(playerId);
			break;
		case VentilationSystem.Operation.Move:
			this.PlayersInsideVents[playerId] = ventId;
			break;
		case VentilationSystem.Operation.BootImpostors:
			this.BootImpostorsFromVent((int)ventId);
			break;
		}
		this.IsDirty = true;
		this.UpdateVentArrows();
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.WritePacked(this.PlayersCleaningVents.Count);
		foreach (KeyValuePair<byte, byte> keyValuePair in this.PlayersCleaningVents)
		{
			writer.Write(keyValuePair.Key);
			writer.Write(keyValuePair.Value);
		}
		writer.WritePacked(this.PlayersInsideVents.Count);
		foreach (KeyValuePair<byte, byte> keyValuePair2 in this.PlayersInsideVents)
		{
			writer.Write(keyValuePair2.Key);
			writer.Write(keyValuePair2.Value);
		}
		this.IsDirty = initialState;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		this.PlayersCleaningVents.Clear();
		int num = reader.ReadPackedInt32();
		for (int i = 0; i < num; i++)
		{
			byte key = reader.ReadByte();
			byte value = reader.ReadByte();
			this.PlayersCleaningVents[key] = value;
		}
		this.PlayersInsideVents.Clear();
		int num2 = reader.ReadPackedInt32();
		for (int j = 0; j < num2; j++)
		{
			byte key2 = reader.ReadByte();
			byte value2 = reader.ReadByte();
			this.PlayersInsideVents[key2] = value2;
		}
		this.UpdateVentArrows();
	}

	private void BootImpostorsFromVent(int ventId)
	{
		foreach (KeyValuePair<byte, byte> keyValuePair in this.PlayersInsideVents.ToArray<KeyValuePair<byte, byte>>())
		{
			if ((int)keyValuePair.Value == ventId)
			{
				byte key = keyValuePair.Key;
				this.PlayersInsideVents.Remove(key);
				this.BootImpostorFromVent(ventId, key);
				this.IsDirty = true;
			}
		}
		if (this.IsDirty)
		{
			this.UpdateVentArrows();
		}
	}

	private void BootImpostorFromVent(int ventId, byte playerId)
	{
		foreach (PlayerControl playerControl in PlayerControl.AllPlayerControls)
		{
			if (playerControl.PlayerId == playerId)
			{
				playerControl.MyPhysics.RpcBootFromVent(ventId);
				break;
			}
		}
	}

	private void UpdateVentArrows()
	{
		Vent[] allVents = ShipStatus.Instance.AllVents;
		for (int i = 0; i < allVents.Length; i++)
		{
			allVents[i].UpdateArrows(this);
		}
	}

	public struct VentMoveInfo
	{
		public VentilationSystem.Operation Op;

		public byte VentId;

		public byte PlayerId;
	}

	public enum Operation
	{
		StartCleaning,
		StopCleaning,
		Enter,
		Exit,
		Move,
		BootImpostors
	}
}
