using System;
using Rewired;
using UnityEngine;

public class InfectedOverlay : MonoBehaviour
{
	public MapRoom[] rooms;

	private IActivatable doors;

	private SabotageSystemType SabSystem;

	public ButtonBehavior[] allButtons;

	public ButtonBehavior selectedButton;

	private const float selectCooldown = 0.5f;

	private float currentCooldown;

	public bool CanUseDoors
	{
		get
		{
			return ShipStatus.Instance.Type == ShipStatus.MapType.Pb || !this.SabSystem.AnyActive;
		}
	}

	public bool CanUseSpecial
	{
		get
		{
			if (this.SabSystem.Timer <= 0f)
			{
				IActivatable activatable = this.doors;
				if (activatable == null || !activatable.IsActive)
				{
					return !this.SabSystem.AnyActive;
				}
			}
			return false;
		}
	}

	public void Start()
	{
		for (int i = 0; i < this.rooms.Length; i++)
		{
			this.rooms[i].Parent = this;
		}
		this.SabSystem = (SabotageSystemType)ShipStatus.Instance.Systems[SystemTypes.Sabotage];
		ISystemType systemType;
		if (ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Doors, out systemType))
		{
			this.doors = (IActivatable)systemType;
		}
	}

	private void FixedUpdate()
	{
		float specialActive = (this.doors != null && this.doors.IsActive) ? 1f : this.SabSystem.PercentCool;
		for (int i = 0; i < this.rooms.Length; i++)
		{
			this.rooms[i].SetSpecialActive(specialActive);
			this.rooms[i].OOBUpdate();
		}
	}

	private void OnEnable()
	{
		this.allButtons = base.GetComponentsInChildren<ButtonBehavior>(true);
		if (!this.selectedButton)
		{
			this.SelectClosestButton(base.transform.position);
		}
	}

	private void DeselectCurrent()
	{
		if (this.selectedButton && this.selectedButton.spriteRenderer)
		{
			this.selectedButton.spriteRenderer.color = Color.white;
			this.selectedButton = null;
		}
	}

	private void Select(ButtonBehavior newSelected)
	{
		if (ActiveInputManager.currentControlType != ActiveInputManager.InputType.Joystick)
		{
			return;
		}
		if (this.selectedButton)
		{
			this.DeselectCurrent();
		}
		this.selectedButton = newSelected;
		if (this.selectedButton && this.selectedButton.spriteRenderer)
		{
			this.selectedButton.spriteRenderer.color = Color.green;
		}
	}

	private void SelectClosestButton(Vector2 anchorSpot)
	{
		ButtonBehavior buttonBehavior = null;
		float num = float.PositiveInfinity;
		foreach (ButtonBehavior buttonBehavior2 in this.allButtons)
		{
			Vector2 vector = buttonBehavior2.transform.position;
			float sqrMagnitude = (anchorSpot - vector).sqrMagnitude;
			if (sqrMagnitude < num || buttonBehavior == null)
			{
				num = sqrMagnitude;
				buttonBehavior = buttonBehavior2;
			}
		}
		if (buttonBehavior != null)
		{
			if (!buttonBehavior.spriteRenderer)
			{
				buttonBehavior.spriteRenderer = buttonBehavior.GetComponent<SpriteRenderer>();
			}
			this.Select(buttonBehavior);
		}
	}

	private void Update()
	{
		if (ActiveInputManager.currentControlType != ActiveInputManager.InputType.Joystick)
		{
			this.DeselectCurrent();
		}
		Player player = ReInput.players.GetPlayer(0);
		Vector2 vector = new Vector2(player.GetAxis(16), player.GetAxis(17));
		float magnitude = vector.magnitude;
		if (magnitude > 0.9f)
		{
			if (this.currentCooldown > 0f)
			{
				this.currentCooldown -= Time.deltaTime;
			}
			else
			{
				ButtonBehavior buttonBehavior = null;
				float num = float.PositiveInfinity;
				Vector2 vector2 = this.selectedButton.transform.position;
				vector /= magnitude;
				Debug.DrawRay(vector2, vector, Color.green, 5f);
				foreach (ButtonBehavior buttonBehavior2 in this.allButtons)
				{
					if (buttonBehavior2 != this.selectedButton)
					{
						Vector2 vector3 = buttonBehavior2.transform.position;
						Vector2 vector4 = vector3 - vector2;
						float magnitude2 = vector4.magnitude;
						vector4 /= magnitude2;
						float num2 = Vector2.Dot(vector, vector4);
						float num3 = num2 / magnitude2;
						if (num2 > 0.7f && (buttonBehavior == null || num3 > num))
						{
							num = num3;
							buttonBehavior = buttonBehavior2;
							Debug.DrawLine(vector2, vector3, Color.cyan, 5f);
						}
						else
						{
							Debug.DrawLine(vector2, vector3, Color.red, 5f);
						}
					}
				}
				if (buttonBehavior != null)
				{
					this.DeselectCurrent();
					this.Select(buttonBehavior);
					this.currentCooldown = 0.5f;
				}
			}
		}
		else
		{
			this.currentCooldown = 0f;
		}
		if (this.selectedButton && player.GetButtonDown(11))
		{
			this.selectedButton.OnClick.Invoke();
		}
	}
}
