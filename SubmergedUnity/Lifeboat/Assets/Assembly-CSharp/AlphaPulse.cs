using System;
using UnityEngine;

public class AlphaPulse : MonoBehaviour
{
	public float Offset = 1f;

	public float Duration = 2.5f;

	private SpriteRenderer rend;

	private MeshRenderer mesh;

	public FloatRange AlphaRange = new FloatRange(0.2f, 0.5f);

	public Color baseColor = Color.white;

	public void SetColor(Color c)
	{
		this.Start();
		this.baseColor = c;
		this.Update();
	}

	private void Start()
	{
		this.mesh = base.GetComponent<MeshRenderer>();
		this.rend = base.GetComponent<SpriteRenderer>();
	}

	public void Update()
	{
		float v = Mathf.Abs(Mathf.Cos((this.Offset + Time.time) * 3.1415927f / this.Duration));
		if (this.rend)
		{
			this.rend.color = new Color(this.baseColor.r, this.baseColor.g, this.baseColor.b, this.AlphaRange.Lerp(v));
		}
		if (this.mesh)
		{
			this.mesh.material.SetColor("_Color", new Color(this.baseColor.r, this.baseColor.g, this.baseColor.b, this.AlphaRange.Lerp(v)));
		}
	}
}
