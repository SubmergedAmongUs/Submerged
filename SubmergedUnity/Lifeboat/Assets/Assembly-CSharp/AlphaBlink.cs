using System;
using TMPro;
using UnityEngine;

public class AlphaBlink : MonoBehaviour
{
	public float Period = 1f;

	public float Ratio = 0.5f;

	private SpriteRenderer rend;

	private MeshRenderer mesh;

	private TextMeshPro tmPro;

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
		this.tmPro = base.GetComponent<TextMeshPro>();
		if (this.tmPro)
		{
			this.baseColor = this.tmPro.color;
		}
	}

	public void Update()
	{
		float num = Time.time % this.Period / this.Period;
		num = (float)((num < this.Ratio) ? 1 : 0);
		if (this.rend)
		{
			this.rend.color = new Color(this.baseColor.r, this.baseColor.g, this.baseColor.b, this.AlphaRange.Lerp(num));
		}
		if (this.tmPro)
		{
			this.tmPro.color = new Color(this.baseColor.r, this.baseColor.g, this.baseColor.b, this.AlphaRange.Lerp(num));
			return;
		}
		if (this.mesh)
		{
			this.mesh.material.SetColor("_Color", new Color(this.baseColor.r, this.baseColor.g, this.baseColor.b, this.AlphaRange.Lerp(num)));
		}
	}
}
