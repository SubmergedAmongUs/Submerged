using System;
using System.Collections.Generic;
using System.Linq;
using Hazel;

public class SabotageSystemType : ISystemType
{
	public const float SpecialSabDelay = 30f;

	private List<IActivatable> specials;

	private SabotageSystemType.DummySab dummy = new SabotageSystemType.DummySab();

	public float Timer { get; set; }

	public float PercentCool
	{
		get
		{
			return this.Timer / 30f;
		}
	}

	public bool AnyActive
	{
		get
		{
			return this.specials.Any((IActivatable s) => s.IsActive);
		}
	}

	public bool IsDirty { get; private set; }

	public SabotageSystemType(IActivatable[] specials)
	{
		this.specials = new List<IActivatable>(specials);
		this.specials.RemoveAll((IActivatable d) => d is IDoorSystem);
		this.specials.Add(this.dummy);
	}

	public void Detoriorate(float deltaTime)
	{
		this.dummy.timer -= deltaTime;
		if (this.Timer > 0f && !this.AnyActive)
		{
			this.Timer -= deltaTime;
			this.IsDirty = true;
		}
	}

	public void ForceSabTime(float t)
	{
		this.dummy.timer = t;
	}

	public void RepairDamage(PlayerControl player, byte amount)
	{
		this.IsDirty = true;
		if (this.Timer > 0f)
		{
			return;
		}
		if (MeetingHud.Instance)
		{
			return;
		}
		if (AmongUsClient.Instance.AmHost)
		{
			if (amount <= 7)
			{
				if (amount != 3)
				{
					if (amount == 7)
					{
						byte b = 4;
						for (int i = 0; i < 5; i++)
						{
							if (BoolRange.Next(0.5f))
							{
								b |= (byte)(1 << i);
							}
						}
						ShipStatus.Instance.RpcRepairSystem(SystemTypes.Electrical, (int)(b | 128));
					}
				}
				else
				{
					ShipStatus.Instance.RepairSystem(SystemTypes.Reactor, player, 128);
				}
			}
			else if (amount != 8)
			{
				if (amount != 14)
				{
					if (amount == 21)
					{
						ShipStatus.Instance.RepairSystem(SystemTypes.Laboratory, player, 128);
					}
				}
				else
				{
					ShipStatus.Instance.RepairSystem(SystemTypes.Comms, player, 128);
				}
			}
			else
			{
				ShipStatus.Instance.RepairSystem(SystemTypes.LifeSupp, player, 128);
			}
		}
		this.Timer = 30f;
		this.IsDirty = true;
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		writer.Write(this.Timer);
		this.IsDirty = initialState;
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
		this.Timer = reader.ReadSingle();
	}

	public class DummySab : IActivatable
	{
		public float timer;

		public bool IsActive
		{
			get
			{
				return this.timer > 0f;
			}
		}
	}
}
