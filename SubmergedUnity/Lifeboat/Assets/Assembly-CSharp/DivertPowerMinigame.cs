using System;
using Rewired;
using UnityEngine;

public class DivertPowerMinigame : Minigame
{
	public SystemTypes[] SliderOrder = new SystemTypes[]
	{
		SystemTypes.LowerEngine,
		SystemTypes.UpperEngine,
		SystemTypes.Weapons,
		SystemTypes.Shields,
		SystemTypes.Nav,
		SystemTypes.Comms,
		SystemTypes.LifeSupp,
		SystemTypes.Security
	};

	public Collider2D[] Sliders;

	public LineRenderer[] Wires;

	public VerticalGauge[] Gauges;

	private int sliderId;

	public FloatRange SliderY = new FloatRange(-1f, 1f);

	private Controller myController = new Controller();

	public ActionMapGlyphDisplay glyphDisplay;

	private int inputJoystick;

	private bool prevHadInput;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		DivertPowerTask powerTask = (DivertPowerTask)task;
		this.sliderId = this.SliderOrder.IndexOf((SystemTypes t) => t == powerTask.TargetSystem);
		for (int i = 0; i < this.Sliders.Length; i++)
		{
			if (i != this.sliderId)
			{
				this.Sliders[i].GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f);
			}
		}
		base.SetupInput(true);
		if (this.sliderId < 4)
		{
			this.inputJoystick = 14;
			this.glyphDisplay.actionToDisplayMappedGlyphFor = RewiredConstsEnum.Action.TaskLVertical;
		}
		else
		{
			this.inputJoystick = 17;
			this.glyphDisplay.actionToDisplayMappedGlyphFor = RewiredConstsEnum.Action.TaskRVertical;
		}
		this.glyphDisplay.UpdateGlyphDisplay();
		this.glyphDisplay.transform.SetParent(this.Sliders[this.sliderId].transform, false);
	}

	public void FixedUpdate()
	{
		this.myController.Update();
		if (this.sliderId >= 0)
		{
			float axisRaw = ReInput.players.GetPlayer(0).GetAxisRaw(this.inputJoystick);
			Collider2D collider2D = this.Sliders[this.sliderId];
			Vector2 vector = collider2D.transform.localPosition;
			if (Mathf.Abs(axisRaw) > 0.01f)
			{
				this.prevHadInput = true;
				vector.y = this.SliderY.Clamp(vector.y + axisRaw * Time.deltaTime * 2f);
				collider2D.transform.localPosition = vector;
			}
			else
			{
				if (this.prevHadInput && this.SliderY.max - vector.y < 0.05f)
				{
					this.MyNormTask.NextStep();
					base.StartCoroutine(base.CoStartClose(0.75f));
					this.sliderId = -1;
					collider2D.GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f);
				}
				this.prevHadInput = false;
			}
		}
		float num = 0f;
		for (int i = 0; i < this.Sliders.Length; i++)
		{
			num += this.SliderY.ReverseLerp(this.Sliders[i].transform.localPosition.y) / (float)this.Sliders.Length;
		}
		for (int j = 0; j < this.Sliders.Length; j++)
		{
			float num2 = this.SliderY.ReverseLerp(this.Sliders[j].transform.localPosition.y);
			float num3 = num2 / num / 1.6f;
			this.Gauges[j].value = num3 + (Mathf.PerlinNoise((float)j, Time.time * 51f) - 0.5f) * 0.04f;
			Color color = Color.Lerp(Color.gray, Color.yellow, num2 * num2);
			color.a = (float)((num3 < 0.1f) ? 0 : 1);
			Vector2 textureOffset = this.Wires[j].material.GetTextureOffset("_MainTex");
			textureOffset.x -= Time.fixedDeltaTime * 3f * Mathf.Lerp(0.1f, 2f, num3);
			this.Wires[j].material.SetTextureOffset("_MainTex", textureOffset);
			this.Wires[j].material.SetColor("_Color", color);
		}
		if (this.sliderId < 0)
		{
			return;
		}
		Collider2D collider2D2 = this.Sliders[this.sliderId];
		Vector2 vector2 = collider2D2.transform.localPosition;
		DragState dragState = this.myController.CheckDrag(collider2D2);
		if (dragState == DragState.Dragging)
		{
			Vector2 vector3 = this.myController.DragPosition - (Vector2) collider2D2.transform.parent.position;
			vector3.y = this.SliderY.Clamp(vector3.y);
			vector2.y = vector3.y;
			collider2D2.transform.localPosition = vector2;
			return;
		}
		if (dragState != DragState.Released)
		{
			return;
		}
		if (this.SliderY.max - vector2.y < 0.05f)
		{
			this.MyNormTask.NextStep();
			base.StartCoroutine(base.CoStartClose(0.75f));
			this.sliderId = -1;
			collider2D2.GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f);
		}
	}
}
