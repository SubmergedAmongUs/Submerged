using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class VertLineBehaviour : MonoBehaviour
{
	public int NumPoints = 128;

	public FloatRange Width;

	public FloatRange Height;

	private Mesh mesh;

	private MeshRenderer rend;

	private Vector3[] vecs;

	public float Duration = 4f;

	private float timer;

	public int Offset = 25;

	public int beats = 4;

	public int beatPadding = 5;

	public Color color
	{
		set
		{
			this.rend.material.SetColor("_Color", value);
		}
	}

	public void Awake()
	{
		this.rend = base.GetComponent<MeshRenderer>();
		this.mesh = new Mesh();
		base.GetComponent<MeshFilter>().mesh = this.mesh;
		this.mesh.MarkDynamic();
		this.vecs = new Vector3[this.NumPoints];
		Vector2[] array = new Vector2[this.NumPoints];
		int[] array2 = new int[this.NumPoints];
		float num = (float)(this.vecs.Length - 1);
		for (int i = 0; i < this.vecs.Length; i++)
		{
			this.vecs[i].y = this.Height.Lerp((float)i / num);
			array2[i] = i;
			array[i] = new Vector2(0.5f, (float)i / num);
		}
		this.mesh.vertices = this.vecs;
		this.mesh.uv = array;
		this.mesh.SetIndices(array2, MeshTopology.LineStrip, 0);
	}

	public void Update()
	{
		this.timer += Time.deltaTime;
		if (this.timer >= this.Duration)
		{
			this.timer = 0f;
		}
		float num = this.timer / this.Duration;
		this.mesh.vertices = this.vecs;
		this.rend.material.SetFloat("_Fade", num);
	}

	public void SetAlive()
	{
		this.color = Color.white;
		int num = this.vecs.Length;
		int num2 = this.vecs.Length / (this.beats + this.beatPadding);
		for (int i = 0; i < this.beats; i++)
		{
			int num3 = (int)((float)i * ((float)this.vecs.Length / (float)this.beats)) + this.Offset;
			for (int j = 0; j < num2; j++)
			{
				int num4 = (j + num3) % this.vecs.Length;
				Vector3 vector = this.vecs[num4];
				float num5 = (float)j / (float)num2;
				if ((double)num5 < 0.3)
				{
					float num6 = num5 / 0.3f;
					vector.x = this.Width.Lerp(Mathf.Lerp(0.5f, 0f, num6));
				}
				else if (num5 < 0.6f)
				{
					float num7 = (num5 - 0.3f) / 0.3f;
					vector.x = this.Width.Lerp(Mathf.Lerp(0f, 1f, num7));
				}
				else
				{
					float num8 = (num5 - 0.6f) / 0.3f;
					vector.x = this.Width.Lerp(Mathf.Lerp(1f, 0.5f, num8));
				}
				this.vecs[num4] = vector;
			}
		}
		this.mesh.vertices = this.vecs;
	}

	public void SetDead()
	{
		this.color = Color.red;
		float num = (float)(this.vecs.Length - 1);
		for (int i = 0; i < this.vecs.Length; i++)
		{
			Vector3 vector = this.vecs[i];
			vector.x = this.Width.Lerp(0.5f);
			vector.y = this.Height.Lerp((float)i / num);
			this.vecs[i] = vector;
		}
		this.mesh.vertices = this.vecs;
	}
}
