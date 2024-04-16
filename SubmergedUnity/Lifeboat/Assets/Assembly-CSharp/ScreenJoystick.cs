using System;
using UnityEngine;

public class ScreenJoystick : MonoBehaviour, IVirtualJoystick
{
	private Collider2D[] hitBuffer = new Collider2D[20];

	private Controller myController = new Controller();

	private int touchId = -1;

	public Vector2 Delta { get; private set; }

	private void FixedUpdate()
	{
		this.myController.Update();
		if (this.touchId <= -1)
		{
			for (int i = 0; i < this.myController.Touches.Length; i++)
			{
				Controller.TouchState touchState = this.myController.Touches[i];
				if (touchState.TouchStart)
				{
					bool flag = false;
					int num = Physics2D.OverlapPointNonAlloc(touchState.Position, this.hitBuffer, Constants.NotShipMask);
					for (int j = 0; j < num; j++)
					{
						Collider2D collider2D = this.hitBuffer[j];
						if (collider2D.GetComponent<ButtonBehavior>() || collider2D.GetComponent<PassiveButton>())
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						this.touchId = i;
						return;
					}
				}
			}
			return;
		}
		Controller.TouchState touchState2 = this.myController.Touches[this.touchId];
		if (touchState2.IsDown)
		{
			Vector2 vector = Camera.main.ViewportToWorldPoint(new Vector2(0.5f, 0.5f));
			this.Delta = (touchState2.Position - vector).normalized;
			return;
		}
		this.touchId = -1;
		this.Delta = Vector2.zero;
	}
}
