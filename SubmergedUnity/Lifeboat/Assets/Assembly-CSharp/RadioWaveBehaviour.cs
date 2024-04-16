using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RadioWaveBehaviour : MonoBehaviour
{
	public int NumPoints = 128;

	public FloatRange Width;

	public FloatRange Height;

	private Mesh mesh;

	private Vector3[] vecs;

	public float TickRate = 0.1f;

	private float timer;

	public int Skip = 2;

	public float Frequency = 5f;

	private int Tick;

	public bool Random;

	[Range(0f, 1f)]
	public float NoiseLevel;

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
			vector.y = this.Height.Next();
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
			this.Tick++;
			for (int i = 0; i < this.vecs.Length - this.Skip; i++)
			{
				this.vecs[i].y = this.vecs[i + this.Skip].y;
			}
			if (this.Random)
			{
				for (int j = 1; j <= this.Skip; j++)
				{
					this.vecs[this.vecs.Length - j].y = this.Height.Next();
				}
			}
			else
			{
				float num = 1f - this.NoiseLevel;
				for (int k = 0; k < this.Skip; k++)
				{
					this.vecs[this.vecs.Length - 1 - this.Skip + k].y = Mathf.Sin(((float)this.Tick + (float)k / (float)this.Skip) * this.Frequency) * this.Height.max * num + this.Height.Next() * this.NoiseLevel;
				}
			}
			this.mesh.vertices = this.vecs;
		}
	}
}
