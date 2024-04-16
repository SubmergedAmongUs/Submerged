using System;
using System.Collections.Generic;
using Hazel;
using InnerNet;

public class VoteBanSystem : InnerNetObject
{
	public static VoteBanSystem Instance;

	public Dictionary<int, int[]> Votes = new Dictionary<int, int[]>();

	public void Awake()
	{
		VoteBanSystem.Instance = this;
	}

	public void CmdAddVote(int clientId)
	{
		this.AddVote(AmongUsClient.Instance.ClientId, clientId);
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(this.NetId, 26, SendOption.Reliable);
		messageWriter.Write(AmongUsClient.Instance.ClientId);
		messageWriter.Write(clientId);
		messageWriter.EndMessage();
	}

	private void AddVote(int srcClient, int clientId)
	{
		int[] array;
		if (!this.Votes.TryGetValue(clientId, out array))
		{
			array = (this.Votes[clientId] = new int[3]);
		}
		int num = -1;
		for (int i = 0; i < array.Length; i++)
		{
			int num2 = array[i];
			if (num2 == srcClient)
			{
				break;
			}
			if (num2 == 0)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			array[num] = srcClient;
			base.SetDirtyBit(1U);
			if (num == array.Length - 1)
			{
				AmongUsClient.Instance.KickPlayer(clientId, false);
			}
		}
	}

	public bool HasMyVote(int clientId)
	{
		int[] array;
		return this.Votes.TryGetValue(clientId, out array) && Array.IndexOf<int>(array, AmongUsClient.Instance.ClientId) != -1;
	}

	public override void HandleRpc(byte callId, MessageReader reader)
	{
		if (callId == 26)
		{
			int srcClient = reader.ReadInt32();
			int clientId = reader.ReadInt32();
			this.AddVote(srcClient, clientId);
		}
	}

	public override bool Serialize(MessageWriter writer, bool initialState)
	{
		writer.Write((byte)this.Votes.Count);
		foreach (KeyValuePair<int, int[]> keyValuePair in this.Votes)
		{
			writer.Write(keyValuePair.Key);
			for (int i = 0; i < 3; i++)
			{
				writer.WritePacked(keyValuePair.Value[i]);
			}
		}
		base.ClearDirtyBits();
		return true;
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
		int num = (int)reader.ReadByte();
		for (int i = 0; i < num; i++)
		{
			int key = reader.ReadInt32();
			int[] array;
			if (!this.Votes.TryGetValue(key, out array))
			{
				array = (this.Votes[key] = new int[3]);
			}
			for (int j = 0; j < 3; j++)
			{
				array[j] = reader.ReadPackedInt32();
			}
		}
	}
}
