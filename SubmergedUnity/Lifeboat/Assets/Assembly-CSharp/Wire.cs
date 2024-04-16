using System;
using UnityEngine;

public class Wire : MonoBehaviour
{
	private const int WireDepth = -14;

	public SpriteRenderer Liner;

	public SpriteRenderer ColorBase;

	public SpriteRenderer SymbolBase;

	public SpriteRenderer ColorEnd;

	public Collider2D hitbox;

	public SpriteRenderer WireTip;

	public sbyte WireId;

	public Vector2 BaseWorldPos { get; internal set; }

	public void Start()
	{
		this.BaseWorldPos = base.transform.position;
	}

	public void ResetLine(Vector3 targetWorldPos, bool reset = false)
	{
		if (reset)
		{
			this.Liner.transform.localScale = new Vector3(0f, 0f, 0f);
			this.WireTip.transform.eulerAngles = Vector3.zero;
			this.WireTip.transform.position = base.transform.position;
			return;
		}
		Vector2 vector = targetWorldPos - base.transform.position;
		Vector2 normalized = vector.normalized;
		Vector3 localPosition = default(Vector3);
		localPosition = vector - normalized * 0.075f;
		localPosition.z = -0.01f;
		this.WireTip.transform.localPosition = localPosition;
		float magnitude = vector.magnitude;
		this.Liner.transform.localScale = new Vector3(magnitude, 1f, 1f);
		this.Liner.transform.localPosition = vector / 2f;
		this.WireTip.transform.LookAt2d(targetWorldPos);
		this.Liner.transform.localEulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, vector));
	}

	public void ConnectRight(WireNode node)
	{
		Vector3 position = node.transform.position;
		this.ResetLine(position, false);
	}

	public void SetColor(Color color, Sprite symbol)
	{
		this.SymbolBase.sprite = symbol;
		this.Liner.material.SetColor("_Color", color);
		this.ColorBase.color = color;
		this.ColorEnd.color = color;
	}
}
