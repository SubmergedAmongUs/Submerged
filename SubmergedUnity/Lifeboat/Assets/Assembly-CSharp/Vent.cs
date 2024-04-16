using System;
using PowerTools;
using UnityEngine;

public class Vent : MonoBehaviour, IUsable
{
	public static Vent currentVent;

	public int Id;

	public Vent Left;

	public Vent Right;

	public Vent Center;

	public ButtonBehavior[] Buttons;

	public GameObject[] CleaningIndicators;

	public AnimationClip EnterVentAnim;

	public AnimationClip ExitVentAnim;

	public Vector3 Offset = new Vector3(0f, 0.3636057f, 0f);

	public float spreadAmount;

	public float spreadShift;

	private SpriteRenderer myRend;

	public ImageNames UseIcon
	{
		get
		{
			return ImageNames.VentButton;
		}
	}

	public float UsableDistance
	{
		get
		{
			return 0.75f;
		}
	}

	public float PercentCool
	{
		get
		{
			return 0f;
		}
	}

	private Vent[] NearbyVents
	{
		get
		{
			return new Vent[]
			{
				this.Right,
				this.Left,
				this.Center
			};
		}
	}

	private void Start()
	{
		this.SetButtons(false);
		this.myRend = base.GetComponent<SpriteRenderer>();
	}

	public void SetButtons(bool enabled)
	{
		Vent[] nearbyVents = this.NearbyVents;
		Vector2 vector;
		if (this.Right && this.Left)
		{
			vector = (this.Right.transform.position + this.Left.transform.position) / 2f - base.transform.position;
		}
		else
		{
			vector = Vector2.zero;
		}
		for (int i = 0; i < this.Buttons.Length; i++)
		{
			ButtonBehavior buttonBehavior = this.Buttons[i];
			if (enabled)
			{
				Vent vent = nearbyVents[i];
				if (vent)
				{
					VentilationSystem ventilationSystem = ShipStatus.Instance.Systems[SystemTypes.Ventilation] as VentilationSystem;
					bool ventBeingCleaned = ventilationSystem != null && ventilationSystem.IsVentCurrentlyBeingCleaned(vent.Id);
					buttonBehavior.gameObject.SetActive(true);
					this.ToggleNeighborVentBeingCleaned(ventBeingCleaned, buttonBehavior, this.CleaningIndicators[i]);
					Vector3 vector2 = vent.transform.position - base.transform.position;
					Vector3 vector3 = vector2.normalized * (0.7f + this.spreadShift);
					vector3.x *= Mathf.Sign(ShipStatus.Instance.transform.localScale.x);
					vector3.y -= 0.08f;
					vector3.z = -10f;
					buttonBehavior.transform.localPosition = vector3;
					buttonBehavior.transform.LookAt2d(vent.transform);
					vector3 = vector3.RotateZ((vector.AngleSigned(vector2) > 0f) ? this.spreadAmount : (-this.spreadAmount));
					buttonBehavior.transform.localPosition = vector3;
					buttonBehavior.transform.Rotate(0f, 0f, (vector.AngleSigned(vector2) > 0f) ? this.spreadAmount : (-this.spreadAmount));
				}
				else
				{
					buttonBehavior.gameObject.SetActive(false);
				}
			}
			else
			{
				buttonBehavior.gameObject.SetActive(false);
			}
		}
	}

	public float CanUse(GameData.PlayerInfo pc, out bool canUse, out bool couldUse)
	{
		float num = float.MaxValue;
		PlayerControl @object = pc.Object;
		couldUse = (pc.IsImpostor && !pc.IsDead && (@object.CanMove || @object.inVent));
		ISystemType systemType;
		if (ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Ventilation, out systemType))
		{
			VentilationSystem ventilationSystem = systemType as VentilationSystem;
			if (ventilationSystem != null && ventilationSystem.IsVentCurrentlyBeingCleaned(this.Id))
			{
				couldUse = false;
			}
		}
		canUse = couldUse;
		if (canUse)
		{
			Vector3 center = @object.Collider.bounds.center;
			Vector3 position = base.transform.position;
			num = Vector2.Distance(center, position);
			canUse &= (num <= this.UsableDistance && !PhysicsHelpers.AnythingBetween(@object.Collider, center, position, Constants.ShipOnlyMask, false));
		}
		return num;
	}

	public void SetOutline(bool on, bool mainTarget)
	{
		this.myRend.material.SetFloat("_Outline", (float)(on ? 1 : 0));
		this.myRend.material.SetColor("_OutlineColor", Color.red);
		this.myRend.material.SetColor("_AddColor", mainTarget ? Color.red : Color.clear);
	}

	public void ClickRight()
	{
		if (this.Right && PlayerControl.LocalPlayer.inVent)
		{
			this.MoveToVent(this.Right);
		}
	}

	public void ClickLeft()
	{
		if (this.Left && PlayerControl.LocalPlayer.inVent)
		{
			this.MoveToVent(this.Left);
		}
	}

	public void ClickCenter()
	{
		if (this.Center && PlayerControl.LocalPlayer.inVent)
		{
			this.MoveToVent(this.Center);
		}
	}

	public void Use()
	{
		bool flag;
		bool flag2;
		this.CanUse(PlayerControl.LocalPlayer.Data, out flag, out flag2);
		if (!flag)
		{
			return;
		}
		DestroyableSingleton<AchievementManager>.Instance.OnConsoleUse(this);
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		if (localPlayer.inVent)
		{
			localPlayer.MyPhysics.RpcExitVent(this.Id);
			this.SetButtons(false);
			return;
		}
		localPlayer.MyPhysics.RpcEnterVent(this.Id);
		this.SetButtons(true);
	}

	public void UpdateArrows(VentilationSystem ventSystem)
	{
		if (this != Vent.currentVent || ventSystem == null)
		{
			return;
		}
		Vent[] nearbyVents = this.NearbyVents;
		for (int i = 0; i < nearbyVents.Length; i++)
		{
			Vent vent = nearbyVents[i];
			if (vent)
			{
				bool ventBeingCleaned = ventSystem.IsVentCurrentlyBeingCleaned(vent.Id);
				ButtonBehavior b = this.Buttons[i];
				GameObject c = this.CleaningIndicators[i];
				this.ToggleNeighborVentBeingCleaned(ventBeingCleaned, b, c);
			}
		}
	}

	internal void EnterVent(PlayerControl pc)
	{
		if (pc.AmOwner)
		{
			Vent.currentVent = this;
			ConsoleJoystick.SetMode_Vent();
		}
		if (!this.EnterVentAnim)
		{
			return;
		}
		base.GetComponent<SpriteAnim>().Play(this.EnterVentAnim, 1f);
		if (pc.AmOwner && Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.StopSound(ShipStatus.Instance.VentEnterSound);
			SoundManager.Instance.PlaySound(ShipStatus.Instance.VentEnterSound, false, 1f).pitch = FloatRange.Next(0.8f, 1.2f);
		}
	}

	internal void ExitVent(PlayerControl pc)
	{
		if (pc.AmOwner)
		{
			Vent.currentVent = null;
		}
		if (!this.ExitVentAnim)
		{
			return;
		}
		base.GetComponent<SpriteAnim>().Play(this.ExitVentAnim, 1f);
		if (pc.AmOwner && Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.StopSound(ShipStatus.Instance.VentEnterSound);
			SoundManager.Instance.PlaySound(ShipStatus.Instance.VentEnterSound, false, 1f).pitch = FloatRange.Next(0.8f, 1.2f);
		}
	}

	private void MoveToVent(Vent otherVent)
	{
		Vector3 vector = otherVent.transform.position;
		vector -= (Vector3) PlayerControl.LocalPlayer.Collider.offset;
		PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(vector);
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(ShipStatus.Instance.VentMoveSounds.Random<AudioClip>(), false, 1f).pitch = FloatRange.Next(0.8f, 1.2f);
		}
		this.SetButtons(false);
		otherVent.SetButtons(true);
		Vent.currentVent = otherVent;
		VentilationSystem.Update(VentilationSystem.Operation.Move, Vent.currentVent.Id);
	}

	private void ToggleNeighborVentBeingCleaned(bool ventBeingCleaned, ButtonBehavior b, GameObject c)
	{
		b.enabled = !ventBeingCleaned;
		c.SetActive(ventBeingCleaned);
	}
}
