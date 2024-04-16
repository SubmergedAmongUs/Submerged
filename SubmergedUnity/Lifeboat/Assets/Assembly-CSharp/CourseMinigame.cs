using System;
using System.Runtime.InteropServices;
using Rewired;
using UnityEngine;
using Object = UnityEngine.Object;

public class CourseMinigame : Minigame
{
	public CourseStarBehaviour StarPrefab;

	public CourseStarBehaviour[] Stars;

	public SpriteRenderer DotPrefab;

	public Sprite DotLight;

	public SpriteRenderer[] Dots;

	public Collider2D Ship;

	public CourseStarBehaviour Destination;

	public Vector3[] PathPoints;

	public int NumPoints;

	public FloatRange XRange;

	public FloatRange YRange;

	public LineRenderer Path;

	public Controller myController = new Controller();

	public float lineTimer;

	private CourseMinigame.UIntFloat Converter;

	public AudioClip SetCourseSound;

	public AudioClip SetCourseLastSound;

	private TouchpadBehavior touchpad;

	private float initialCurVec;

	private float targetCurVec;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		this.PathPoints = new Vector3[this.NumPoints];
		this.Stars = new CourseStarBehaviour[this.NumPoints];
		this.Dots = new SpriteRenderer[this.NumPoints];
		for (int i = 0; i < this.PathPoints.Length; i++)
		{
			this.PathPoints[i].x = this.XRange.Lerp((float)i / ((float)this.PathPoints.Length - 1f));
			do
			{
				this.PathPoints[i].y = this.YRange.Next();
			}
			while (i > 0 && Mathf.Abs(this.PathPoints[i - 1].y - this.PathPoints[i].y) < this.YRange.Width / 4f);
			this.Dots[i] = UnityEngine.Object.Instantiate<SpriteRenderer>(this.DotPrefab, base.transform);
			this.Dots[i].transform.localPosition = this.PathPoints[i];
			if (i == 0)
			{
				this.Dots[i].sprite = this.DotLight;
			}
			else
			{
				if (i == 1)
				{
					this.Ship.transform.localPosition = this.PathPoints[0];
					this.Ship.transform.eulerAngles = new Vector3(0f, 0f, Vector2.up.AngleSigned(this.PathPoints[1] - this.PathPoints[0]));
				}
				this.Stars[i] = UnityEngine.Object.Instantiate<CourseStarBehaviour>(this.StarPrefab, base.transform);
				this.Stars[i].transform.localPosition = this.PathPoints[i];
				if (i == this.PathPoints.Length - 1)
				{
					this.Destination.transform.localPosition = this.PathPoints[i];
				}
			}
		}
		this.Path.positionCount = this.PathPoints.Length;
		this.Path.SetPositions(this.PathPoints);
		this.touchpad = base.GetComponent<TouchpadBehavior>();
		base.SetupInput(true);
	}

	public void FixedUpdate()
	{
		float num = this.Converter.GetFloat(this.MyNormTask.Data);
		int num2 = (int)num;
		Vector2 vector = this.PathPoints[num2];
		this.myController.Update();
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			Player player = ReInput.players.GetPlayer(0);
			Vector2 vector2 = new Vector2(player.GetAxis(13), player.GetAxis(14));
			float magnitude = vector2.magnitude;
			if (magnitude > 0.1f)
			{
				if (num < (float)(this.PathPoints.Length - 1))
				{
					Vector2 vector3 = (Vector2) this.PathPoints[num2 + 1] - vector;
					Vector2 normalized = vector3.normalized;
					Vector2 normalized2 = vector2.normalized;
					float num3 = Vector2.Dot(normalized, normalized2);
					if (num3 > 0.7f)
					{
						num += magnitude * num3 * Time.deltaTime * 1.75f;
					}
					else
					{
						num = Mathf.Max((float)num2, Mathf.Lerp(num, (float)num2, Time.deltaTime * 5f));
					}
					this.Ship.transform.eulerAngles = new Vector3(0f, 0f, Vector2.up.AngleSigned(vector3));
				}
			}
			else if (this.touchpad.IsTouching())
			{
				if (num < (float)(this.PathPoints.Length - 1))
				{
					if (this.touchpad.IsFirstTouch())
					{
						this.initialCurVec = num;
						this.targetCurVec = Mathf.Floor(num) + 1f;
					}
					Vector2 vector4 = (Vector2) this.PathPoints[num2 + 1] - vector;
					Vector2 touchVector = this.touchpad.GetTouchVector();
					float num4 = Vector2.Dot(vector4.normalized, touchVector.normalized);
					if (num4 > 0.7f)
					{
						num = (touchVector.x + this.initialCurVec) * num4;
						if (num > this.targetCurVec)
						{
							num = this.targetCurVec + 0.01f;
							this.initialCurVec = num;
							this.targetCurVec = Mathf.Floor(num) + 1f;
							this.touchpad.ResetTouch();
						}
					}
					else
					{
						num = Mathf.Max((float)num2, Mathf.Lerp(num, (float)num2, Time.deltaTime * 5f));
					}
					if (num < this.initialCurVec)
					{
						num = this.initialCurVec;
					}
					this.Ship.transform.eulerAngles = new Vector3(0f, 0f, Vector2.up.AngleSigned(vector4));
				}
			}
			else
			{
				num = Mathf.Max((float)num2, Mathf.Lerp(num, (float)num2, Time.deltaTime * 5f));
			}
			if (num < (float)(this.PathPoints.Length - 1))
			{
				float num5 = num - Mathf.Floor(num);
				int num6 = (int)num;
				Vector3 localPosition = Vector2.Lerp(this.PathPoints[num6], this.PathPoints[num6 + 1], num5);
				localPosition.z = -1f;
				this.Ship.transform.localPosition = localPosition;
			}
			else
			{
				Vector3 localPosition2 = this.PathPoints[this.PathPoints.Length - 1];
				localPosition2.z = -1f;
				this.Ship.transform.localPosition = localPosition2;
			}
		}
		else
		{
			DragState dragState = this.myController.CheckDrag(this.Ship);
			if (dragState != DragState.NoTouch)
			{
				if (dragState == DragState.Dragging)
				{
					if (num < (float)(this.PathPoints.Length - 1))
					{
						Vector2 vector5 = (Vector2) this.PathPoints[num2 + 1] - vector;
						Vector2 vector6 = new Vector2(1f, vector5.y / vector5.x);
						Vector2 vector7 = (Vector2) base.transform.InverseTransformPoint(this.myController.DragPosition) - vector;
						if (vector7.x > 0f)
						{
							Vector2 vector8 = vector6 * vector7.x;
							if (Mathf.Abs(vector8.y - vector7.y) < 0.5f)
							{
								num = (float)num2 + Mathf.Min(1f, vector7.x / vector5.x);
								Vector3 localPosition3 = vector8 + vector;
								localPosition3.z = -1f;
								this.Ship.transform.localPosition = localPosition3;
								this.Ship.transform.localPosition = localPosition3;
								this.Ship.transform.eulerAngles = new Vector3(0f, 0f, Vector2.up.AngleSigned(vector5));
							}
							else
							{
								this.myController.Reset();
							}
						}
					}
					else
					{
						Vector3 localPosition4 = this.PathPoints[this.PathPoints.Length - 1];
						localPosition4.z = -1f;
						this.Ship.transform.localPosition = localPosition4;
					}
				}
			}
			else if (num < (float)(this.PathPoints.Length - 1))
			{
				Vector2 vector9 = (Vector2) this.PathPoints[num2 + 1] - vector;
				Vector2 vector10 = new Vector2(1f, vector9.y / vector9.x);
				num = Mathf.Max((float)num2, Mathf.Lerp(num, (float)num2, Time.deltaTime * 5f));
				Vector3 localPosition5 = vector10 * (num - (float)num2) + vector;
				localPosition5.z = -1f;
				this.Ship.transform.localPosition = localPosition5;
			}
			else
			{
				Vector3 localPosition6 = this.PathPoints[this.PathPoints.Length - 1];
				localPosition6.z = -1f;
				this.Ship.transform.localPosition = localPosition6;
			}
		}
		if ((int)num > num2 && this.Stars[num2 + 1])
		{
			 UnityEngine.Object.Destroy(this.Stars[num2 + 1].gameObject);
			this.Dots[num2 + 1].sprite = this.DotLight;
			if (num2 == this.PathPoints.Length - 2)
			{
				if (Constants.ShouldPlaySfx())
				{
					SoundManager.Instance.PlaySound(this.SetCourseLastSound, false, 1f).volume = 0.7f;
				}
				this.Destination.Speed *= 5f;
				this.MyNormTask.NextStep();
				base.StartCoroutine(base.CoStartClose(0.75f));
			}
			else if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(this.SetCourseSound, false, 1f).volume = 0.7f;
			}
		}
		this.Converter.GetBytes(num, this.MyNormTask.Data);
		this.SetLineDivision(num);
	}

	private void SetLineDivision(float curVec)
	{
		int num = (int)curVec;
		float num2 = 0f;
		int num3 = 0;
		while ((float)num3 <= curVec && num3 < this.PathPoints.Length - 1)
		{
			float num4 = Vector2.Distance(this.PathPoints[num3], this.PathPoints[num3 + 1]);
			if (num3 == num)
			{
				num4 *= curVec - (float)num3;
			}
			num2 += num4;
			num3++;
		}
		this.lineTimer -= Time.fixedDeltaTime;
		Vector2 vector = new Vector2(this.lineTimer, 0f);
		this.Path.material.SetTextureOffset("_MainTex", vector);
		this.Path.material.SetTextureOffset("_AltTex", vector);
		this.Path.material.SetFloat("_Perc", num2 + this.lineTimer / 8f);
	}

	[StructLayout(LayoutKind.Explicit)]
	private struct UIntFloat
	{
		[FieldOffset(0)]
		public float FloatValue;

		[FieldOffset(0)]
		public int IntValue;

		public float GetFloat(byte[] bytes)
		{
			this.IntValue = ((int)bytes[0] | (int)bytes[1] << 8 | (int)bytes[2] << 16 | (int)bytes[3] << 24);
			return this.FloatValue;
		}

		public void GetBytes(float value, byte[] bytes)
		{
			this.FloatValue = value;
			bytes[0] = (byte)(this.IntValue & 255);
			bytes[1] = (byte)(this.IntValue >> 8 & 255);
			bytes[2] = (byte)(this.IntValue >> 16 & 255);
			bytes[3] = (byte)(this.IntValue >> 24 & 255);
		}
	}
}
