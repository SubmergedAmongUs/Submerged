using System;
using Hazel;

public interface ISystemType
{
	bool IsDirty { get; }

	void Detoriorate(float deltaTime);

	void RepairDamage(PlayerControl player, byte amount);

	void UpdateSystem(PlayerControl player, MessageReader msgReader);

	void Serialize(MessageWriter writer, bool initialState);

	void Deserialize(MessageReader reader, bool initialState);
}
