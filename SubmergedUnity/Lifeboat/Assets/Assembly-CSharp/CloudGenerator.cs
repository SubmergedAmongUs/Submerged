using System;
using UnityEngine;

public class CloudGenerator : MonoBehaviour
{
	public Sprite[] CloudImages;

	private Vector2[] ExtentCache;

	public int NumClouds = 500;

	public float Length = 25f;

	public float Width = 25f;

	public Vector2 Direction = new Vector2(1f, 0f);

	private Vector2 NormDir = new Vector2(1f, 0f);

	private Vector2 Tangent = new Vector2(0f, 1f);

	private float tanLen;

	public FloatRange Rates = new FloatRange(0.25f, 1f);

	public FloatRange Sizes = new FloatRange(0.75f, 1.25f);

	public bool Depth;

	public float MaxDepth = 4f;

	public float ParallaxOffset;

	public float ParallaxStrength = 1f;

	[HideInInspector]
	private CloudGenerator.Cloud[] clouds;

	[HideInInspector]
	private Vector3[] verts;

	[HideInInspector]
	private Mesh mesh;

	public void Start()
	{
		Vector2[][] array = new Vector2[this.CloudImages.Length][];
		this.ExtentCache = new Vector2[this.CloudImages.Length];
		for (int i = 0; i < this.CloudImages.Length; i++)
		{
			Sprite sprite = this.CloudImages[i];
			array[i] = sprite.uv;
			this.ExtentCache[i] = sprite.bounds.extents;
		}
		this.clouds = new CloudGenerator.Cloud[this.NumClouds];
		this.verts = new Vector3[this.NumClouds * 4];
		Vector2[] array2 = new Vector2[this.NumClouds * 4];
		int[] array3 = new int[this.NumClouds * 6];
		this.SetDirection(this.Direction);
		MeshFilter component = base.GetComponent<MeshFilter>();
		this.mesh = new Mesh();
		this.mesh.MarkDynamic();
		component.mesh = this.mesh;
		Vector3 vector = default(Vector3);
		for (int j = 0; j < this.clouds.Length; j++)
		{
			CloudGenerator.Cloud cloud = this.clouds[j];
			int num = cloud.CloudIdx = this.CloudImages.RandomIdx<Sprite>();
			Vector2 vector2 = this.ExtentCache[num];
			Vector2[] array4 = array[num];
			float num2 = FloatRange.Next(-1f, 1f) * this.Length;
			float num3 = FloatRange.Next(-1f, 1f) * this.Width;
			float num4 = cloud.PositionX = num2 * this.NormDir.x + num3 * this.Tangent.x;
			float num5 = cloud.PositionY = num2 * this.NormDir.y + num3 * this.Tangent.y;
			cloud.Rate = this.Rates.Next();
			cloud.Size = this.Sizes.Next();
			cloud.FlipX = (float)(BoolRange.Next(0.5f) ? -1 : 1);
			if (this.Depth)
			{
				cloud.PositionZ = FloatRange.Next(0f, this.MaxDepth);
			}
			vector2 *= cloud.Size;
			this.clouds[j] = cloud;
			int num6 = j * 4;
			vector.x = num4 - vector2.x * cloud.FlipX;
			vector.y = num5 + vector2.y;
			vector.z = cloud.PositionZ;
			this.verts[num6] = vector;
			vector.x = num4 + vector2.x * cloud.FlipX;
			this.verts[num6 + 1] = vector;
			vector.x = num4 - vector2.x * cloud.FlipX;
			vector.y = num5 - vector2.y;
			this.verts[num6 + 2] = vector;
			vector.x = num4 + vector2.x * cloud.FlipX;
			this.verts[num6 + 3] = vector;
			array2[num6] = array4[0];
			array2[num6 + 1] = array4[1];
			array2[num6 + 2] = array4[2];
			array2[num6 + 3] = array4[3];
			int num7 = j * 6;
			array3[num7] = num6;
			array3[num7 + 1] = num6 + 1;
			array3[num7 + 2] = num6 + 2;
			array3[num7 + 3] = num6 + 2;
			array3[num7 + 4] = num6 + 1;
			array3[num7 + 5] = num6 + 3;
		}
		this.mesh.vertices = this.verts;
		this.mesh.uv = array2;
		this.mesh.SetIndices(array3, 0, 0);
	}

	private void Update()
	{
		float num = -0.99f * this.Length;
		Vector2 vector = this.Direction * Time.deltaTime;
		Vector3 vector2 = default(Vector3);
		for (int i = 0; i < this.clouds.Length; i++)
		{
			int num2 = i * 4;
			CloudGenerator.Cloud cloud = this.clouds[i];
			float num3 = cloud.PositionX;
			float num4 = cloud.PositionY;
			Vector2 vector3 = this.ExtentCache[cloud.CloudIdx];
			vector3 *= cloud.Size;
			float rate = cloud.Rate;
			num3 += rate * vector.x;
			num4 += rate * vector.y;
			if (this.OrthoDistance(num3, num4) > this.Length)
			{
				float num5 = FloatRange.Next(-1f, 1f) * this.Width;
				num3 = num * this.NormDir.x + num5 * this.Tangent.x;
				num4 = num * this.NormDir.y + num5 * this.Tangent.y;
				cloud.Rate = this.Rates.Next();
				cloud.Size = this.Sizes.Next();
				cloud.FlipX = (float)(BoolRange.Next(0.5f) ? -1 : 1);
				if (this.Depth)
				{
					cloud.PositionZ = FloatRange.Next(0f, this.MaxDepth);
				}
			}
			cloud.PositionX = num3;
			cloud.PositionY = num4;
			if (this.Depth)
			{
				num4 += (base.transform.position.y + this.ParallaxOffset) / (cloud.PositionZ * this.ParallaxStrength + 0.0001f);
				vector2.z = cloud.PositionZ;
			}
			this.clouds[i] = cloud;
			vector2.x = num3 - vector3.x * cloud.FlipX;
			vector2.y = num4 + vector3.y;
			this.verts[num2] = vector2;
			vector2.x = num3 + vector3.x * cloud.FlipX;
			this.verts[num2 + 1] = vector2;
			vector2.x = num3 - vector3.x * cloud.FlipX;
			vector2.y = num4 - vector3.y;
			this.verts[num2 + 2] = vector2;
			vector2.x = num3 + vector3.x * cloud.FlipX;
			this.verts[num2 + 3] = vector2;
		}
		this.mesh.vertices = this.verts;
	}

	public void SetDirection(Vector2 dir)
	{
		this.Direction = dir;
		this.NormDir = this.Direction.normalized;
		this.Tangent = new Vector2(-this.NormDir.y, this.NormDir.x);
		this.tanLen = Mathf.Sqrt(this.Tangent.y * this.Tangent.y + this.Tangent.x * this.Tangent.x);
	}

	private float OrthoDistance(float pointx, float pointy)
	{
		return (this.Tangent.y * pointx - this.Tangent.x * pointy) / this.tanLen;
	}

	private struct Cloud
	{
		public int CloudIdx;

		public float Rate;

		public float Size;

		public float FlipX;

		public float PositionX;

		public float PositionY;

		public float PositionZ;
	}
}
