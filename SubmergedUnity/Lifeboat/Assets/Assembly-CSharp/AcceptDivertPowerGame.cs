using System;
using System.Collections;
using Rewired;
using UnityEngine;

public class AcceptDivertPowerGame : Minigame
{
	private LineRenderer[] LeftWires;

	private LineRenderer[] RightWires;

	public GameObject RightWireParent;

	public GameObject LeftWireParent;

	public SpriteRenderer Switch;

	public AudioClip SwitchSound;

	private bool done;

	private bool prevHadInput;

	private float rotateAngle;

	private Vector2 prevStickDir;

	public void Start()
	{
		this.LeftWires = this.LeftWireParent.GetComponentsInChildren<LineRenderer>();
		this.RightWires = this.RightWireParent.GetComponentsInChildren<LineRenderer>();
		for (int i = 0; i < this.LeftWires.Length; i++)
		{
			this.LeftWires[i].material.SetColor("_Color", Color.yellow);
		}
		base.SetupInput(true);
	}

	public void DoSwitch()
	{
		if (this.done)
		{
			return;
		}
		this.done = true;
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.SwitchSound, false, 1f);
		}
		base.StartCoroutine(this.CoDoSwitch());
	}

	private IEnumerator CoDoSwitch()
	{
		yield return new WaitForLerp(0.25f, delegate(float t)
		{
			this.Switch.transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(0f, 90f, t));
		});
		this.LeftWires[0].SetPosition(1, new Vector3(1.265f, 0f, 0f));
		for (int i = 0; i < this.RightWires.Length; i++)
		{
			this.RightWires[i].enabled = true;
			this.RightWires[i].material.SetColor("_Color", Color.yellow);
		}
		for (int j = 0; j < this.LeftWires.Length; j++)
		{
			this.LeftWires[j].material.SetColor("_Color", Color.yellow);
		}
		if (this.MyNormTask)
		{
			this.MyNormTask.NextStep();
		}
		base.StartCoroutine(base.CoStartClose(0.75f));
		yield break;
	}

	public void Update()
	{
		if (!this.done)
		{
			Vector2 axis2DRaw = ReInput.players.GetPlayer(0).GetAxis2DRaw(13, 14);
			if (axis2DRaw.sqrMagnitude > 0.9f)
			{
				Vector2 normalized = axis2DRaw.normalized;
				if (this.prevHadInput)
				{
					float num = Vector2.SignedAngle(this.prevStickDir, normalized);
					this.rotateAngle += num;
					if (Mathf.Abs(this.rotateAngle) > 45f)
					{
						this.DoSwitch();
					}
				}
				this.prevStickDir = normalized;
				this.prevHadInput = true;
			}
			else
			{
				this.rotateAngle = 0f;
				this.prevHadInput = false;
			}
		}
		for (int i = 0; i < this.LeftWires.Length; i++)
		{
			Vector2 textureOffset = this.LeftWires[i].material.GetTextureOffset("_MainTex");
			textureOffset.x -= Time.deltaTime * 3f;
			this.LeftWires[i].material.SetTextureOffset("_MainTex", textureOffset);
		}
		for (int j = 0; j < this.RightWires.Length; j++)
		{
			Vector2 textureOffset2 = this.RightWires[j].material.GetTextureOffset("_MainTex");
			textureOffset2.x += Time.deltaTime * 3f;
			this.RightWires[j].material.SetTextureOffset("_MainTex", textureOffset2);
		}
	}
}
