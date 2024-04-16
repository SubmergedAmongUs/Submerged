using System;
using Rewired;
using UnityEngine;

public class WireMinigame : Minigame
{
	private static readonly Color[] colors = new Color[]
	{
		Color.red,
		new Color(0.15f, 0.15f, 1f, 1f),
		Color.yellow,
		Color.magenta
	};

	public Sprite[] Symbols;

	public Wire[] LeftNodes;

	public WireNode[] RightNodes;

	public SpriteRenderer[] LeftLights;

	public SpriteRenderer[] RightLights;

	private Controller myController = new Controller();

	private sbyte[] ExpectedWires = new sbyte[4];

	private sbyte[] ActualWires = new sbyte[4];

	public AudioClip[] WireSounds;

	private int prevSelectedWireIndex = -1;

	private int selectedWireIndex;

	private bool prevButtonDown;

	private float inputCooldown;

	public Vector2 controllerWirePos = Vector2.zero;

	private const float controllerWireSpeed = 7f;

	public GameObject[] selectingWireGlyphs;

	public GameObject[] movingWireGlyphs;

	public Transform selectedWireUI;

	private bool TaskIsForThisPanel()
	{
		return this.MyNormTask.taskStep < this.MyNormTask.Data.Length && !this.MyNormTask.IsComplete && (int)this.MyNormTask.Data[this.MyNormTask.taskStep] == base.ConsoleId;
	}

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		IntRange.FillRandomRange(this.ExpectedWires);
		for (int i = 0; i < this.LeftNodes.Length; i++)
		{
			this.ActualWires[i] = -1;
			int num = (int)this.ExpectedWires[i];
			Wire wire = this.LeftNodes[i];
			wire.SetColor(WireMinigame.colors[num], this.Symbols[num]);
			wire.WireId = (sbyte)i;
			this.RightNodes[i].SetColor(WireMinigame.colors[i], this.Symbols[i]);
			this.RightNodes[i].WireId = (sbyte)i;
			int num2 = (int)this.ActualWires[i];
			if (num2 > -1)
			{
				wire.ConnectRight(this.RightNodes[num2]);
			}
			else
			{
				wire.ResetLine(Vector3.zero, true);
			}
		}
		this.UpdateLights();
		base.SetupInput(true);
	}

	public void Update()
	{
		if (!this.TaskIsForThisPanel())
		{
			return;
		}
		this.myController.Update();
		this.selectedWireUI.gameObject.SetActive(Controller.currentTouchType == Controller.TouchType.Joystick);
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			if (!this.amOpening)
			{
				Player player = ReInput.players.GetPlayer(0);
				Vector2 vector = new Vector2(player.GetAxis(13), player.GetAxis(14));
				bool button = player.GetButton(11);
				if (button != this.prevButtonDown)
				{
					GameObject[] array = this.movingWireGlyphs;
					for (int i = 0; i < array.Length; i++)
					{
						array[i].SetActive(button);
					}
					array = this.selectingWireGlyphs;
					for (int i = 0; i < array.Length; i++)
					{
						array[i].SetActive(!button);
					}
					if (button)
					{
						this.controllerWirePos = this.LeftNodes[this.selectedWireIndex].transform.position + Vector3.right * 0.2f;
						VibrationManager.Vibrate(0.3f, 0f, 0.05f, VibrationManager.VibrationFalloff.None, null, false);
					}
				}
				if (button)
				{
					this.prevSelectedWireIndex = this.selectedWireIndex;
					Wire wire = this.LeftNodes[this.selectedWireIndex];
					this.controllerWirePos += vector * Time.deltaTime * 7f;
					this.controllerWirePos.x = Mathf.Min(this.controllerWirePos.x, this.RightNodes[0].transform.position.x);
					WireNode wireNode = this.CheckRightSide(this.controllerWirePos);
					wireNode = this.CheckRightSide(this.controllerWirePos);
					if (wireNode)
					{
						Vector2 vector2 = wireNode.transform.position;
						this.ActualWires[(int)wire.WireId] = wireNode.WireId;
						wire.ResetLine(vector2, false);
					}
					else
					{
						this.ActualWires[(int)wire.WireId] = -1;
						wire.ResetLine(this.controllerWirePos, false);
					}
				}
				else
				{
					this.prevSelectedWireIndex = -1;
					if (this.prevButtonDown)
					{
						Wire wire2 = this.LeftNodes[this.selectedWireIndex];
						if (this.ActualWires[(int)wire2.WireId] == -1)
						{
							wire2.ResetLine(wire2.BaseWorldPos, true);
						}
						else
						{
							if (Constants.ShouldPlaySfx())
							{
								SoundManager.Instance.PlaySound(this.WireSounds.Random<AudioClip>(), false, 1f);
							}
							VibrationManager.Vibrate(0f, 0.3f, 0.05f, VibrationManager.VibrationFalloff.None, null, false);
						}
						this.CheckTask();
					}
					else if (Math.Abs(vector.y) > 0.5f)
					{
						if (this.inputCooldown > 0f)
						{
							this.inputCooldown -= Time.deltaTime;
						}
						else
						{
							int num = this.selectedWireIndex;
							int num2 = (int)Mathf.Sign(-vector.y);
							this.selectedWireIndex = Mathf.Clamp(this.selectedWireIndex + num2, 0, 3);
							if (this.selectedWireIndex != num)
							{
								this.inputCooldown = 0.25f;
								Vector3 localPosition = this.selectedWireUI.localPosition;
								localPosition.y = this.LeftNodes[this.selectedWireIndex].transform.localPosition.y;
								this.selectedWireUI.localPosition = localPosition;
							}
						}
					}
					else
					{
						this.inputCooldown = 0f;
					}
				}
				this.prevButtonDown = button;
			}
		}
		else
		{
			this.prevButtonDown = false;
			if (this.prevSelectedWireIndex != -1)
			{
				Wire wire3 = this.LeftNodes[this.prevSelectedWireIndex];
				if (this.ActualWires[(int)wire3.WireId] == -1)
				{
					wire3.ResetLine(wire3.BaseWorldPos, true);
				}
				else if (Constants.ShouldPlaySfx())
				{
					SoundManager.Instance.PlaySound(this.WireSounds.Random<AudioClip>(), false, 1f);
				}
				this.CheckTask();
				this.prevSelectedWireIndex = -1;
			}
			for (int j = 0; j < this.LeftNodes.Length; j++)
			{
				Wire wire4 = this.LeftNodes[j];
				DragState dragState = this.myController.CheckDrag(wire4.hitbox);
				if (dragState != DragState.Dragging)
				{
					if (dragState == DragState.Released)
					{
						if (this.ActualWires[(int)wire4.WireId] == -1)
						{
							wire4.ResetLine(wire4.BaseWorldPos, true);
						}
						else if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySound(this.WireSounds.Random<AudioClip>(), false, 1f);
						}
						this.CheckTask();
					}
				}
				else
				{
					Vector2 vector3 = this.myController.DragPosition;
					WireNode wireNode2 = this.CheckRightSide(vector3);
					if (wireNode2)
					{
						vector3 = wireNode2.transform.position;
						this.ActualWires[(int)wire4.WireId] = wireNode2.WireId;
					}
					else
					{
						vector3 -= wire4.BaseWorldPos.normalized * 0.05f;
						this.ActualWires[(int)wire4.WireId] = -1;
					}
					wire4.ResetLine(vector3, false);
				}
			}
		}
		this.UpdateLights();
	}

	private void UpdateLights()
	{
		for (int i = 0; i < this.ActualWires.Length; i++)
		{
			Color color = Color.yellow;
			color *= 1f - Mathf.PerlinNoise((float)i, Time.time * 35f) * 0.3f;
			color.a = 1f;
			if (this.ActualWires[i] != this.ExpectedWires[i])
			{
				this.RightLights[(int)this.ExpectedWires[i]].color = new Color(0.2f, 0.2f, 0.2f);
			}
			else
			{
				this.RightLights[(int)this.ExpectedWires[i]].color = color;
			}
			this.LeftLights[i].color = color;
		}
	}

	private WireNode CheckRightSide(Vector2 pos)
	{
		for (int i = 0; i < this.RightNodes.Length; i++)
		{
			WireNode wireNode = this.RightNodes[i];
			if (wireNode.hitbox.OverlapPoint(pos))
			{
				return wireNode;
			}
		}
		return null;
	}

	private void CheckTask()
	{
		bool flag = true;
		for (int i = 0; i < this.ActualWires.Length; i++)
		{
			if (this.ActualWires[i] != this.ExpectedWires[i])
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			this.MyNormTask.NextStep();
			this.Close();
		}
	}
}
