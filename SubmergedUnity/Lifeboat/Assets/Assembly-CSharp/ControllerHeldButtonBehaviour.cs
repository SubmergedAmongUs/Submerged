using System;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

public class ControllerHeldButtonBehaviour : MonoBehaviour
{
	public RewiredConstsEnum.Action Action;

	private Player player;

	public SpriteRenderer targetCooldownSprite;

	public float holdDuration;

	private float holdTimer;

	private bool alreadyPressed;

	private void Start()
	{
		this.player = ReInput.players.GetPlayer(0);
	}

	private void Update()
	{
		if (this.player.GetButton((int)this.Action))
		{
			if (!this.alreadyPressed)
			{
				this.holdTimer += Time.unscaledDeltaTime;
				if (this.targetCooldownSprite)
				{
					float num = Mathf.Clamp01(this.holdTimer / this.holdDuration);
					this.targetCooldownSprite.SetCooldownNormalizedUvs();
					this.targetCooldownSprite.material.SetFloat("_Percent", 1f - num);
				}
				if (this.holdTimer >= this.holdDuration)
				{
					this.alreadyPressed = true;
					ButtonBehavior component = base.GetComponent<ButtonBehavior>();
					if (component)
					{
						Button.ButtonClickedEvent onClick = component.OnClick;
						if (onClick == null)
						{
							return;
						}
						onClick.Invoke();
						return;
					}
					else
					{
						PassiveButton component2 = base.GetComponent<PassiveButton>();
						if (component2)
						{
							Button.ButtonClickedEvent onClick2 = component2.OnClick;
							if (onClick2 == null)
							{
								return;
							}
							onClick2.Invoke();
							return;
						}
					}
				}
			}
		}
		else
		{
			if (this.holdTimer > 0f)
			{
				this.targetCooldownSprite.material.SetFloat("_Percent", 0f);
			}
			this.alreadyPressed = false;
			this.holdTimer = 0f;
		}
	}
}
