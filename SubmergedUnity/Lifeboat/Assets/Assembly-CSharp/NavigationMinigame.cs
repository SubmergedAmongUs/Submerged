using System;
using System.Collections;
using Rewired;
using UnityEngine;

public class NavigationMinigame : Minigame
{
	public MeshRenderer TwoAxisImage;

	public SpriteRenderer CrossHairImage;

	public Collider2D hitbox;

	private Controller myController = new Controller();

	private Vector2 crossHair;

	private Vector2 half = new Vector2(0.5f, 0.5f);

	private Vector2 initialPos;

	private TouchpadBehavior touchpad;

	private bool prevHadInput;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		this.crossHair = UnityEngine.Random.insideUnitCircle.normalized / 2f * 0.6f;
		Vector3 localPosition = new Vector3(this.crossHair.x * this.TwoAxisImage.bounds.size.x, this.crossHair.y * this.TwoAxisImage.bounds.size.y, -2f);
		this.CrossHairImage.transform.localPosition = localPosition;
		this.TwoAxisImage.material.SetVector("_CrossHair", this.crossHair + this.half);
		this.touchpad = base.GetComponent<TouchpadBehavior>();
		base.SetupInput(true);
	}

	public void FixedUpdate()
	{
		if (this.MyNormTask && this.MyNormTask.IsComplete)
		{
			return;
		}
		this.myController.Update();
		if (Controller.currentTouchType != Controller.TouchType.Joystick)
		{
			switch (this.myController.CheckDrag(this.hitbox))
			{
			case DragState.TouchStart:
			case DragState.Dragging:
			{
				Vector2 dragPosition = this.myController.DragPosition;
				Vector2 a = dragPosition - (Vector2) (this.TwoAxisImage.transform.position - this.TwoAxisImage.bounds.size / 2f);
				this.crossHair = a.Div(this.TwoAxisImage.bounds.size);
				if ((this.crossHair - this.half).magnitude < 0.45f)
				{
					Vector3 localPosition = (Vector3) dragPosition - base.transform.position;
					localPosition.z = -2f;
					this.CrossHairImage.transform.localPosition = localPosition;
					this.TwoAxisImage.material.SetVector("_CrossHair", this.crossHair);
					return;
				}
				break;
			}
			case DragState.Holding:
				break;
			case DragState.Released:
				if ((this.crossHair - this.half).magnitude < 0.05f)
				{
					base.StartCoroutine(this.CompleteGame());
					this.MyNormTask.NextStep();
				}
				break;
			default:
				return;
			}
			return;
		}
		Player player = ReInput.players.GetPlayer(0);
		Vector2 vector = Vector2.zero;
		if (player.GetButton(11))
		{
			if (!this.prevHadInput)
			{
				this.inputHandler.disableVirtualCursor = false;
				VirtualCursor.instance.SetWorldPosition(this.CrossHairImage.transform.position);
			}
			vector = VirtualCursor.currentPosition;
			Vector2 a2 = vector - (Vector2) (this.TwoAxisImage.transform.position - this.TwoAxisImage.bounds.size / 2f);
			this.crossHair = a2.Div(this.TwoAxisImage.bounds.size);
			if ((this.crossHair - this.half).magnitude < 0.45f)
			{
				Vector3 localPosition2 = (Vector3) vector - base.transform.position;
				localPosition2.z = -2f;
				this.CrossHairImage.transform.localPosition = localPosition2;
				this.TwoAxisImage.material.SetVector("_CrossHair", this.crossHair);
			}
			this.prevHadInput = true;
			return;
		}
		if (this.touchpad.IsTouching())
		{
			if (this.touchpad.IsFirstTouch())
			{
				this.initialPos = this.TwoAxisImage.material.GetVector("_CrossHair");
			}
			this.crossHair = this.touchpad.GetTouchVector() + this.initialPos;
			if ((this.crossHair - this.half).magnitude < 0.45f)
			{
				this.CrossHairImage.transform.localPosition = (this.crossHair - this.half) * 5f;
				this.TwoAxisImage.material.SetVector("_CrossHair", this.crossHair);
			}
			this.prevHadInput = true;
			return;
		}
		if (this.prevHadInput)
		{
			this.inputHandler.disableVirtualCursor = true;
			if ((this.crossHair - this.half).magnitude < 0.05f)
			{
				base.StartCoroutine(this.CompleteGame());
				this.MyNormTask.NextStep();
			}
		}
		this.prevHadInput = false;
	}

	private IEnumerator CompleteGame()
	{
		WaitForSeconds wait = new WaitForSeconds(0.1f);
		Color green = new Color(0f, 0.8f, 0f, 1f);
		Color32 yellow = new Color32(byte.MaxValue, 202, 0, byte.MaxValue);
		this.CrossHairImage.transform.localPosition = new Vector3(0f, 0f, -2f);
		this.TwoAxisImage.material.SetVector("_CrossHair", this.half);
		this.CrossHairImage.color = yellow;
		this.TwoAxisImage.material.SetColor("_CrossColor", yellow);
		yield return wait;
		this.CrossHairImage.color = Color.white;
		this.TwoAxisImage.material.SetColor("_CrossColor", Color.white);
		yield return wait;
		this.CrossHairImage.color = yellow;
		this.TwoAxisImage.material.SetColor("_CrossColor", yellow);
		yield return wait;
		this.CrossHairImage.color = Color.white;
		this.TwoAxisImage.material.SetColor("_CrossColor", Color.white);
		yield return wait;
		this.CrossHairImage.color = green;
		this.TwoAxisImage.material.SetColor("_CrossColor", green);
		yield return base.CoStartClose(0.75f);
		yield break;
	}
}
