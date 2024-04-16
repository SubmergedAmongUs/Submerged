using System;
using System.Collections;
using Rewired;
using UnityEngine;

public class AlignGame : Minigame
{
	private Controller myController = new Controller();

	public FloatRange YRange = new FloatRange(-0.425f, 0.425f);

	public AnimationCurve curve;

	public LineRenderer centerline;

	public LineRenderer[] guidelines;

	public SpriteRenderer engine;

	public Collider2D col;

	private float pulseTimer;

	private bool wasPushingJoystick;

	private float initialY;

	private TouchpadBehavior touchpad;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		float v = AlignGame.FromByte(this.MyNormTask.Data[base.ConsoleId]);
		Vector3 localPosition = this.col.transform.localPosition;
		localPosition.y = this.YRange.Lerp(v);
		float num = this.YRange.ReverseLerp(localPosition.y);
		localPosition.x = this.curve.Evaluate(num);
		this.col.transform.eulerAngles = new Vector3(0f, 0f, Mathf.Lerp(20f, -20f, num));
		this.engine.transform.eulerAngles = new Vector3(0f, 0f, Mathf.Lerp(45f, -45f, num));
		this.col.transform.localPosition = localPosition;
		this.centerline.material.SetColor("_Color", Color.red);
		this.engine.color = Color.red;
		this.guidelines[0].enabled = false;
		this.guidelines[1].enabled = false;
		this.touchpad = base.GetComponent<TouchpadBehavior>();
		base.SetupInput(true);
	}

	public void Update()
	{
		this.centerline.material.SetTextureOffset("_MainTex", new Vector2(Time.time, 0f));
		this.guidelines[0].material.SetTextureOffset("_MainTex", new Vector2(Time.time, 0f));
		this.guidelines[1].material.SetTextureOffset("_MainTex", new Vector2(Time.time, 0f));
		if (this.amClosing != Minigame.CloseState.None)
		{
			return;
		}
		if (this.MyTask && this.MyNormTask.IsComplete)
		{
			return;
		}
		Vector3 localPosition = this.col.transform.localPosition;
		Player player = ReInput.players.GetPlayer(0);
		Vector2 vector = new Vector2(player.GetAxis(13), player.GetAxis(14));
		if (vector.magnitude > 0.01f)
		{
			this.wasPushingJoystick = true;
			float num = this.YRange.ReverseLerp(localPosition.y);
			float num2 = Mathf.Clamp01(num + vector.y * Time.deltaTime);
			localPosition.y = this.YRange.Lerp(num2);
			localPosition.x = this.curve.Evaluate(num2);
			this.col.transform.eulerAngles = new Vector3(0f, 0f, Mathf.Lerp(20f, -20f, num2));
			this.engine.transform.eulerAngles = new Vector3(0f, 0f, Mathf.Lerp(45f, -45f, num2));
			this.MyNormTask.Data[base.ConsoleId] = AlignGame.ToByte(num2);
			this.centerline.material.SetColor("_Color", AlignGame.ShouldComplete(this.MyNormTask.Data[base.ConsoleId]) ? Color.green : Color.red);
			if (Mathf.Abs(num2 - num) > 0.001f)
			{
				this.pulseTimer += Time.deltaTime * 25f;
				int num3 = (int)this.pulseTimer % 3;
				if (num3 > 1)
				{
					if (num3 == 2)
					{
						this.engine.color = Color.clear;
					}
				}
				else
				{
					this.engine.color = Color.red;
				}
			}
			else
			{
				this.engine.color = Color.red;
			}
		}
		else if (this.touchpad.IsTouching())
		{
			this.wasPushingJoystick = true;
			if (this.touchpad.IsFirstTouch())
			{
				this.initialY = localPosition.y;
			}
			float y = this.touchpad.GetTouchVector().y;
			float num4 = this.YRange.ReverseLerp(this.initialY);
			float num5 = Mathf.Clamp01(num4 + y);
			localPosition.y = this.YRange.Lerp(num5);
			localPosition.x = this.curve.Evaluate(num5);
			this.col.transform.eulerAngles = new Vector3(0f, 0f, Mathf.Lerp(20f, -20f, num5));
			this.engine.transform.eulerAngles = new Vector3(0f, 0f, Mathf.Lerp(45f, -45f, num5));
			this.MyNormTask.Data[base.ConsoleId] = AlignGame.ToByte(num5);
			this.centerline.material.SetColor("_Color", AlignGame.ShouldComplete(this.MyNormTask.Data[base.ConsoleId]) ? Color.green : Color.red);
			if (Mathf.Abs(num5 - num4) > 0.001f)
			{
				this.pulseTimer += Time.deltaTime * 25f;
				int num3 = (int)this.pulseTimer % 3;
				if (num3 > 1)
				{
					if (num3 == 2)
					{
						this.engine.color = Color.clear;
					}
				}
				else
				{
					this.engine.color = Color.red;
				}
			}
			else
			{
				this.engine.color = Color.red;
			}
		}
		else if (this.wasPushingJoystick)
		{
			this.wasPushingJoystick = false;
			if (AlignGame.ShouldComplete(this.MyNormTask.Data[base.ConsoleId]))
			{
				this.MyNormTask.NextStep();
				this.MyNormTask.Data[base.ConsoleId + 2] = 1;
				base.StartCoroutine(this.LockEngine());
				base.StartCoroutine(base.CoStartClose(0.75f));
			}
			else
			{
				this.engine.color = Color.red;
			}
		}
		this.myController.Update();
		switch (this.myController.CheckDrag(this.col))
		{
		case DragState.TouchStart:
			this.pulseTimer = 0f;
			break;
		case DragState.Dragging:
		{
			Vector2 vector2 = this.myController.DragPosition - (Vector2) base.transform.position;
			float num6 = this.YRange.ReverseLerp(localPosition.y);
			localPosition.y = this.YRange.Clamp(vector2.y);
			float num7 = this.YRange.ReverseLerp(localPosition.y);
			localPosition.x = this.curve.Evaluate(num7);
			this.col.transform.eulerAngles = new Vector3(0f, 0f, Mathf.Lerp(20f, -20f, num7));
			this.engine.transform.eulerAngles = new Vector3(0f, 0f, Mathf.Lerp(45f, -45f, num7));
			this.MyNormTask.Data[base.ConsoleId] = AlignGame.ToByte(num7);
			this.centerline.material.SetColor("_Color", AlignGame.ShouldComplete(this.MyNormTask.Data[base.ConsoleId]) ? Color.green : Color.red);
			if (Mathf.Abs(num7 - num6) > 0.001f)
			{
				this.pulseTimer += Time.deltaTime * 25f;
				int num3 = (int)this.pulseTimer % 3;
				if (num3 > 1)
				{
					if (num3 == 2)
					{
						this.engine.color = Color.clear;
					}
				}
				else
				{
					this.engine.color = Color.red;
				}
			}
			else
			{
				this.engine.color = Color.red;
			}
			break;
		}
		case DragState.Released:
			if (AlignGame.ShouldComplete(this.MyNormTask.Data[base.ConsoleId]))
			{
				this.MyNormTask.NextStep();
				this.MyNormTask.Data[base.ConsoleId + 2] = 1;
				base.StartCoroutine(this.LockEngine());
				base.StartCoroutine(base.CoStartClose(0.75f));
			}
			else
			{
				this.engine.color = Color.red;
			}
			break;
		}
		this.col.transform.localPosition = localPosition;
	}

	private IEnumerator LockEngine()
	{
		int num;
		for (int i = 0; i < 3; i = num)
		{
			this.guidelines[0].enabled = true;
			this.guidelines[1].enabled = true;
			yield return new WaitForSeconds(0.1f);
			this.guidelines[0].enabled = false;
			this.guidelines[1].enabled = false;
			yield return new WaitForSeconds(0.1f);
			num = i + 1;
		}
		Color green = new Color(0f, 0.7f, 0f);
		yield return new WaitForLerp(1f, delegate(float t)
		{
			this.engine.color = Color.Lerp(Color.white, green, t);
		});
		this.guidelines[0].enabled = true;
		this.guidelines[1].enabled = true;
		yield break;
	}

	public static float FromByte(byte b)
	{
		return (float)b / 256f;
	}

	public static byte ToByte(float y)
	{
		return (byte)(y * 255f);
	}

	public static bool IsSuccess(byte b)
	{
		return b > 0;
	}

	public static bool ShouldComplete(byte b)
	{
		return (float)Mathf.Abs((int)(b - 128)) < 12.75f;
	}
}
