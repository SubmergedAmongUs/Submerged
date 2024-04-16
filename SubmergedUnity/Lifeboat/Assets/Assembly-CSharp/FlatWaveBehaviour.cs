using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class FlatWaveBehaviour : MonoBehaviour
{
	public int NumPoints = 128;

	public FloatRange Width;

	public FloatRange Delta;

	public float Center;

	private Mesh mesh;

	private Vector3[] vecs;

	public float TickRate = 0.1f;

	private float timer;

	public int Skip = 3;

	[Range(0f, 1f)]
	public float NoiseP = 0.5f;

	public void Start()
	{
		this.mesh = new Mesh();
		base.GetComponent<MeshFilter>().mesh = this.mesh;
		this.mesh.MarkDynamic();
		this.vecs = new Vector3[this.NumPoints];
		int[] array = new int[this.NumPoints];
		for (int i = 0; i < this.vecs.Length; i++)
		{
			Vector3 vector = this.vecs[i];
			vector.x = this.Width.Lerp((float)i / (float)this.vecs.Length);
			vector.y = this.Center;
			if (BoolRange.Next(this.NoiseP))
			{
				vector.y += this.Delta.Next();
			}
			this.vecs[i] = vector;
			array[i] = i;
		}
		this.mesh.vertices = this.vecs;
		this.mesh.SetIndices(array, MeshTopology.LineStrip, 0);
	}

	public void Update()
	{
		this.timer += Time.deltaTime;
		if (this.timer > this.TickRate)
		{
			this.timer = 0f;
			for (int i = 0; i < this.vecs.Length - this.Skip; i++)
			{
				this.vecs[i].y = this.vecs[i + this.Skip].y;
			}
			for (int j = 1; j <= this.Skip; j++)
			{
				this.vecs[this.vecs.Length - j].y = this.Center;
				if (BoolRange.Next(this.NoiseP))
				{
					Vector3[] array = this.vecs;
					int num = this.vecs.Length - j;
					array[num].y = array[num].y + this.Delta.Next();
				}
			}
			this.mesh.vertices = this.vecs;
		}
	}
}
