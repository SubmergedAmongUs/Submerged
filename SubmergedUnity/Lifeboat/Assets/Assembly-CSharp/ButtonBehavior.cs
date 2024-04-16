using System;
using UnityEngine;
using UnityEngine.UI;

public class ButtonBehavior : UiElement
{
	public bool OnUp = true;

	public bool OnDown;

	public bool Repeat;

	public Button.ButtonClickedEvent OnClick = new Button.ButtonClickedEvent();

	private Controller myController = new Controller();

	private Collider2D[] colliders;

	private float downTime;

	public SpriteRenderer spriteRenderer;

	private bool checkedClickEvent;

	public void OnEnable()
	{
		this.colliders = base.GetComponents<Collider2D>();
		this.myController.Reset();
		this.spriteRenderer = base.GetComponent<SpriteRenderer>();
		if (!this.checkedClickEvent)
		{
			this.checkedClickEvent = true;
			int persistentEventCount = this.OnClick.GetPersistentEventCount();
			for (int i = 0; i < persistentEventCount; i++)
			{
				string persistentMethodName = this.OnClick.GetPersistentMethodName(i);
				if (base.gameObject.name.ToLower().Contains("close") && persistentMethodName.ToLower().Contains("close"))
				{
					base.gameObject.AddComponent<CloseButtonConsoleBehaviour>();
				}
			}
		}
	}

	public void Update()
	{
		this.myController.Update();
		foreach (Collider2D coll in this.colliders)
		{
			switch (this.myController.CheckDrag(coll))
			{
			case DragState.TouchStart:
				if (this.OnDown)
				{
					this.OnClick.Invoke();
				}
				break;
			case DragState.Dragging:
				if (this.Repeat)
				{
					this.downTime += Time.fixedDeltaTime;
					if (this.downTime >= 0.3f)
					{
						this.downTime = 0f;
						this.OnClick.Invoke();
					}
				}
				else
				{
					this.downTime = 0f;
				}
				break;
			case DragState.Released:
				if (this.OnUp)
				{
					this.OnClick.Invoke();
				}
				break;
			}
		}
	}

	public void ReceiveClick()
	{
		this.OnClick.Invoke();
	}
}
