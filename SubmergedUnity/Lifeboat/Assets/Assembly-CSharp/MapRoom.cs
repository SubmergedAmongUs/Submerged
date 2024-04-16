using System;
using UnityEngine;

public class MapRoom : MonoBehaviour
{
	public SystemTypes room;

	public SpriteRenderer door;

	public SpriteRenderer special;

	public InfectedOverlay Parent { get; set; }

	public void Start()
	{
		if (this.door)
		{
			this.door.SetCooldownNormalizedUvs();
		}
		if (this.special)
		{
			this.special.SetCooldownNormalizedUvs();
		}
	}

	public void OOBUpdate()
	{
		if (this.door && ShipStatus.Instance)
		{
			float timer = ((RunTimer)ShipStatus.Instance.Systems[SystemTypes.Doors]).GetTimer(this.room);
			float num = this.Parent.CanUseDoors ? (timer / 30f) : 1f;
			this.door.material.SetFloat("_Percent", num);
		}
	}

	internal void SetSpecialActive(float perc)
	{
		if (this.special)
		{
			this.special.material.SetFloat("_Percent", perc);
		}
	}

	public void SabotageReactor()
	{
		if (!this.Parent.CanUseSpecial)
		{
			return;
		}
		ShipStatus.Instance.RpcRepairSystem(SystemTypes.Sabotage, 3);
	}

	public void SabotageComms()
	{
		if (!this.Parent.CanUseSpecial)
		{
			return;
		}
		ShipStatus.Instance.RpcRepairSystem(SystemTypes.Sabotage, 14);
	}

	public void SabotageOxygen()
	{
		if (!this.Parent.CanUseSpecial)
		{
			return;
		}
		ShipStatus.Instance.RpcRepairSystem(SystemTypes.Sabotage, 8);
	}

	public void SabotageLights()
	{
		if (!this.Parent.CanUseSpecial)
		{
			return;
		}
		ShipStatus.Instance.RpcRepairSystem(SystemTypes.Sabotage, 7);
	}

	public void SabotageSeismic()
	{
		if (!this.Parent.CanUseSpecial)
		{
			return;
		}
		ShipStatus.Instance.RpcRepairSystem(SystemTypes.Sabotage, 21);
	}

	public void SabotageDoors()
	{
		if (!this.Parent.CanUseDoors)
		{
			return;
		}
		if (((RunTimer)ShipStatus.Instance.Systems[SystemTypes.Doors]).GetTimer(this.room) > 0f)
		{
			return;
		}
		ShipStatus.Instance.RpcCloseDoorsOfType(this.room);
	}
}
