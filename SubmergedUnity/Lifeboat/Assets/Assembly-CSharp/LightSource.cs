using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class LightSource : MonoBehaviour
{
	public static Dictionary<GameObject, NoShadowBehaviour> NoShadows = new Dictionary<GameObject, NoShadowBehaviour>();

	public static Dictionary<GameObject, OneWayShadows> OneWayShadows = new Dictionary<GameObject, OneWayShadows>();

	[HideInInspector]
	private GameObject child;

	[HideInInspector]
	private Vector2[] requiredDels;

	[HideInInspector]
	private Mesh myMesh;

	public int MinRays = 24;

	public float LightRadius = 3f;

	public Material Material;

	private RaycastHit2D[] buffer = new RaycastHit2D[50];

	private Collider2D[] hits = new Collider2D[100];

	private ContactFilter2D filter;

	public bool useGPUShadow = true;

	private bool oldUseGPUShadow;

	private Vector2 del;

	private Vector2 tan;

	private Vector2 side;

	private const float twopi = 6.2831855f;

	public Mesh occluderMesh;

	private List<Vector3> occVerts = new List<Vector3>();

	private List<Vector3> occNorms = new List<Vector3>();

	private List<Vector2> occUVs = new List<Vector2>();

	private List<int> occTris = new List<int>();

	public Mesh lightChildMesh;

	private MeshFilter lightChildMeshFilter;

	public Material shadowCasterMaterialPrefab;

	private Material shadowCasterMaterial;

	private CommandBuffer cb;

	private LightSource.AddEdgeDelegate AddEdge;

	private bool noGeomSupport;

	[Header("RenderTexture config")]
	public int shadowmapResolution = 2048;

	public RenderTextureFormat preferredRTFormat = RenderTextureFormat.RHalf;

	private RenderTexture shadowTexture;

	[HideInInspector]
	private List<LightSource.VertInfo> verts = new List<LightSource.VertInfo>(256);

	[HideInInspector]
	private int vertCount;

	private Vector3[] vec;

	private Vector2[] uvs;

	private int[] triangles = new int[1800];

	public float tol = 0.05f;

	private List<RaycastHit2D> lightHits = new List<RaycastHit2D>();

	private void Start()
	{
		this.filter.useTriggers = true;
		this.filter.layerMask = Constants.ShadowMask;
		this.filter.useLayerMask = true;
		this.requiredDels = new Vector2[this.MinRays];
		for (int i = 0; i < this.requiredDels.Length; i++)
		{
			this.requiredDels[i] = Vector2.left.Rotate((float)i / (float)this.requiredDels.Length * 360f);
		}
		this.myMesh = new Mesh();
		this.myMesh.MarkDynamic();
		this.myMesh.name = "ShadowMesh";
		GameObject gameObject = new GameObject("LightChild");
		gameObject.layer = 10;
		this.lightChildMeshFilter = gameObject.AddComponent<MeshFilter>();
		Renderer renderer = gameObject.AddComponent<MeshRenderer>();
		this.Material = new Material(this.Material);
		renderer.sharedMaterial = this.Material;
		this.child = gameObject;
		this.occluderMesh = new Mesh();
		this.occluderMesh.MarkDynamic();
		this.occluderMesh.name = "Occluder Mesh";
		this.UpdateShadowType();
		GraphicsDeviceType graphicsDeviceType = SystemInfo.graphicsDeviceType;
		if (graphicsDeviceType == GraphicsDeviceType.OpenGLES2 || graphicsDeviceType == GraphicsDeviceType.Metal)
		{
			this.noGeomSupport = true;
			this.AddEdge = new LightSource.AddEdgeDelegate(this.AddEdgeNoGeom);
			Debug.Log("Graphics API has no geometry shader support, switching to CPU workaround");
		}
		else
		{
			this.noGeomSupport = false;
			this.AddEdge = new LightSource.AddEdgeDelegate(this.AddEdgeDefault);
			Debug.Log("Graphics API has geometry shader support");
		}
		if (!this.noGeomSupport && !SystemInfo.supportsGeometryShaders)
		{
			this.noGeomSupport = true;
			this.AddEdge = new LightSource.AddEdgeDelegate(this.AddEdgeNoGeom);
			Debug.Log("Graphics hardware has no geometry shader support, switching to CPU workaround");
		}
	}

	private void UpdateShadowType()
	{
		if (this.useGPUShadow)
		{
			this.lightChildMeshFilter.sharedMesh = this.lightChildMesh;
			this.Material.EnableKeyword("GPU_SHADOW");
			this.GenerateShadowmap();
			return;
		}
		this.lightChildMeshFilter.sharedMesh = this.myMesh;
		this.Material.DisableKeyword("GPU_SHADOW");
		if (this.shadowTexture)
		{
			this.shadowTexture.Release();
			this.shadowTexture = null;
		}
	}

	private void Update()
	{
		this.vertCount = 0;
		Vector3 position = base.transform.position;
		position.z -= 7f;
		this.child.transform.position = position;
		Vector2 myPos = position;
		this.Material.SetFloat("_LightRadius", this.LightRadius);
		if (this.useGPUShadow)
		{
			this.GPUShadows(myPos);
			return;
		}
		this.RaycastShadows(myPos);
	}

	private void GenerateShadowmap()
	{
		if (SystemInfo.SupportsRenderTextureFormat(this.preferredRTFormat))
		{
			this.shadowTexture = new RenderTexture(3, this.shadowmapResolution, 16, this.preferredRTFormat);
		}
		else
		{
			this.shadowTexture = new RenderTexture(3, this.shadowmapResolution, 16, RenderTextureFormat.R8);
		}
		this.shadowTexture.wrapModeU = (TextureWrapMode) 1;
		this.shadowTexture.wrapModeV = 0;
		this.shadowTexture.filterMode = (FilterMode) 1;
		this.shadowTexture.Create();
		this.shadowCasterMaterial = new Material(this.shadowCasterMaterialPrefab);
	}

	private void ClearEdges()
	{
		this.occluderMesh.Clear();
		this.occVerts.Clear();
		this.occNorms.Clear();
		if (this.noGeomSupport)
		{
			this.occUVs.Clear();
		}
		this.occTris.Clear();
	}

	private void UpdateOccMesh()
	{
		this.occluderMesh.SetVertices(this.occVerts);
		this.occluderMesh.SetNormals(this.occNorms);
		if (this.noGeomSupport)
		{
			this.occluderMesh.SetUVs(0, this.occUVs);
		}
		this.occluderMesh.SetTriangles(this.occTris, 0, this.occTris.Count, 0, true, 0);
	}

	private void DrawOcclusion(float effectiveRadius)
	{
		if (this.cb == null)
		{
			this.cb = new CommandBuffer();
			this.cb.name = "Draw occlusion";
		}
		if (this.shadowTexture && this.shadowCasterMaterial)
		{
			float num = (float)this.shadowTexture.width;
			this.shadowCasterMaterial.SetFloat("_DepthCompressionValue", effectiveRadius);
			this.cb.Clear();
			this.cb.SetRenderTarget(this.shadowTexture);
			this.cb.ClearRenderTarget(true, true, new Color(1f, 1f, 1f, 1f));
			this.cb.SetGlobalTexture("_ShmapTexture", this.shadowTexture);
			this.cb.SetGlobalFloat("_Radius", this.LightRadius);
			this.cb.SetGlobalFloat("_Column", 0f);
			this.cb.SetGlobalVector("_lightPosition", base.transform.position);
			this.cb.SetGlobalVector("_TexelSize", new Vector4(1f / num, 1f / num, num, num));
			this.cb.SetGlobalFloat("_DepthCompressionValue", effectiveRadius);
			this.cb.DrawMesh(this.occluderMesh, Matrix4x4.identity, this.shadowCasterMaterial);
			Graphics.ExecuteCommandBuffer(this.cb);
		}
	}

	private void DebugDrawRadius(Vector2 center, float radius)
	{
		int num = Mathf.Max(20, (int)(20f * radius));
		Vector2 vector = center + Vector2.right * radius;
		for (int i = 1; i <= num; i++)
		{
			float num2 = (float)i / (float)num * 3.1415927f * 2f;
			Vector2 vector2 = center + new Vector2(Mathf.Cos(num2), Mathf.Sin(num2)) * radius;
			Debug.DrawLine(vector, vector2, Color.yellow);
			vector = vector2;
		}
	}

	private void GPUShadows(Vector2 myPos)
	{
		this.ClearEdges();
		this.child.transform.localScale = new Vector3(this.LightRadius * 3f, this.LightRadius * 3f, 1f);
		Camera main = Camera.main;
		Vector2 vector = main.transform.position - base.transform.position;
		Vector2 vector2 = new Vector2(main.orthographicSize * main.aspect, main.orthographicSize);
		float num = vector2.magnitude + vector.magnitude;
		float num2 = Mathf.Min(this.LightRadius, num);
		foreach (NoShadowBehaviour noShadowBehaviour in LightSource.NoShadows.Values)
		{
			noShadowBehaviour.CheckHit(num2, myPos);
		}
		int num3 = Physics2D.OverlapCircleNonAlloc(myPos, num2, this.hits, Constants.ShadowMask);
		for (int i = 0; i < num3; i++)
		{
			Collider2D collider2D = this.hits[i];
			NoShadowBehaviour noShadowBehaviour2;
			OneWayShadows oneWayShadows;
			if (!collider2D.isTrigger && (!LightSource.NoShadows.TryGetValue(collider2D.gameObject, out noShadowBehaviour2) || !(noShadowBehaviour2.hitOverride == collider2D)) && (!LightSource.OneWayShadows.TryGetValue(collider2D.gameObject, out oneWayShadows) || !oneWayShadows.IsIgnored(this)))
			{
				EdgeCollider2D edgeCollider2D = collider2D as EdgeCollider2D;
				if (edgeCollider2D)
				{
					Vector2[] points = edgeCollider2D.points;
					for (int j = 0; j < points.Length - 1; j++)
					{
						Vector3 a = edgeCollider2D.transform.TransformPoint(points[j]);
						Vector3 b = edgeCollider2D.transform.TransformPoint(points[j + 1]);
						this.AddEdge(a, b);
					}
				}
				else
				{
					PolygonCollider2D polygonCollider2D = collider2D as PolygonCollider2D;
					if (polygonCollider2D)
					{
						Vector2[] points2 = polygonCollider2D.points;
						for (int k = 0; k < points2.Length; k++)
						{
							int num4 = k + 1;
							if (num4 == points2.Length)
							{
								num4 = 0;
							}
							Vector3 a2 = polygonCollider2D.transform.TransformPoint(points2[k]);
							Vector3 b2 = polygonCollider2D.transform.TransformPoint(points2[num4]);
							this.AddEdge(a2, b2);
						}
					}
					else
					{
						BoxCollider2D boxCollider2D = collider2D as BoxCollider2D;
						if (boxCollider2D)
						{
							Vector2 vector3 = boxCollider2D.size / 2f;
							Vector2 vector4 = boxCollider2D.transform.TransformPoint(boxCollider2D.offset - vector3);
							Vector2 vector5 = boxCollider2D.transform.TransformPoint(boxCollider2D.offset + vector3);
							Vector3 a3 = vector4;
							Vector3 b3 = vector4;
							b3.y = vector5.y;
							this.AddEdge(a3, b3);
							a3.y = vector5.y;
							b3.x = vector5.x;
							this.AddEdge(a3, b3);
							a3 = vector5;
							b3.y = vector4.y;
							this.AddEdge(a3, b3);
							a3.y = vector4.y;
							b3 = vector4;
							this.AddEdge(a3, b3);
						}
					}
				}
			}
		}
		this.UpdateOccMesh();
		this.DrawOcclusion(num2);
	}

	private void AddEdgeNoGeom(Vector3 a, Vector3 b)
	{
		Vector2 vector = a - base.transform.position;
		Vector2 vector2 = b - base.transform.position;
		float num = Mathf.Atan2(vector.y, vector.x);
		float num2 = Mathf.Atan2(vector2.y, vector2.x);
		float num3 = num + ((num < 0f) ? 6.2831855f : 0f);
		num2 += ((num2 < 0f) ? 6.2831855f : 0f);
		if (Mathf.Abs(num3 - num2) > 3.1415927f)
		{
			this.AddEdgeInternal(a, b, new Vector2(0f, 0f));
			this.AddEdgeInternal(a, b, new Vector2(0f, 6.2831855f));
			return;
		}
		this.AddEdgeInternal(a, b, new Vector2(1f, 0f));
	}

	private void AddEdgeInternal(Vector3 a, Vector3 b, Vector2 uv)
	{
		a.z = 0f;
		b.z = 0f;
		Vector3 vector = b - a;
		vector = new Vector3(vector.y, -vector.x, 0f);
		Vector3 item = a + Vector3.forward * 0.5f;
		Vector3 item2 = a - Vector3.forward * 0.5f;
		Vector3 item3 = b + Vector3.forward * 0.5f;
		Vector3 item4 = b - Vector3.forward * 0.5f;
		int count;
		int num = count = this.occVerts.Count;
		int item5 = num + 1;
		int item6 = num + 2;
		int item7 = num + 3;
		this.occVerts.Add(item);
		this.occVerts.Add(item2);
		this.occVerts.Add(item3);
		this.occVerts.Add(item4);
		this.occNorms.Add(vector);
		this.occNorms.Add(vector);
		this.occNorms.Add(vector);
		this.occNorms.Add(vector);
		this.occUVs.Add(uv);
		this.occUVs.Add(uv);
		this.occUVs.Add(uv);
		this.occUVs.Add(uv);
		this.occTris.Add(count);
		this.occTris.Add(item5);
		this.occTris.Add(item6);
		this.occTris.Add(item5);
		this.occTris.Add(item7);
		this.occTris.Add(item6);
	}

	private void AddEdgeDefault(Vector3 a, Vector3 b)
	{
		a.z = 0f;
		b.z = 0f;
		Vector3 vector = b - a;
		vector = new Vector3(vector.y, -vector.x, 0f);
		Vector3 item = a + Vector3.forward * 0.5f;
		Vector3 item2 = a - Vector3.forward * 0.5f;
		Vector3 item3 = b + Vector3.forward * 0.5f;
		Vector3 item4 = b - Vector3.forward * 0.5f;
		int count;
		int num = count = this.occVerts.Count;
		int item5 = num + 1;
		int item6 = num + 2;
		int item7 = num + 3;
		this.occVerts.Add(item);
		this.occVerts.Add(item2);
		this.occVerts.Add(item3);
		this.occVerts.Add(item4);
		this.occNorms.Add(vector);
		this.occNorms.Add(vector);
		this.occNorms.Add(vector);
		this.occNorms.Add(vector);
		this.occTris.Add(count);
		this.occTris.Add(item5);
		this.occTris.Add(item6);
		this.occTris.Add(item5);
		this.occTris.Add(item7);
		this.occTris.Add(item6);
	}

	private void RaycastShadows(Vector2 myPos)
	{
		this.child.transform.localScale = Vector3.one;
		int num = Physics2D.OverlapCircleNonAlloc(myPos, this.LightRadius, this.hits, Constants.ShadowMask);
		for (int i = 0; i < num; i++)
		{
			Collider2D collider2D = this.hits[i];
			if (!collider2D.isTrigger)
			{
				EdgeCollider2D edgeCollider2D = collider2D as EdgeCollider2D;
				if (edgeCollider2D)
				{
					Vector2[] points = edgeCollider2D.points;
					for (int j = 0; j < points.Length; j++)
					{
						Vector2 vector = edgeCollider2D.transform.TransformPoint(points[j]);
						this.del.x = vector.x - myPos.x;
						this.del.y = vector.y - myPos.y;
						this.TestBothSides(myPos);
					}
				}
				else
				{
					PolygonCollider2D polygonCollider2D = collider2D as PolygonCollider2D;
					if (polygonCollider2D)
					{
						Vector2[] points2 = polygonCollider2D.points;
						for (int k = 0; k < points2.Length; k++)
						{
							Vector2 vector2 = polygonCollider2D.transform.TransformPoint(points2[k]);
							this.del.x = vector2.x - myPos.x;
							this.del.y = vector2.y - myPos.y;
							this.TestBothSides(myPos);
						}
					}
					else
					{
						BoxCollider2D boxCollider2D = collider2D as BoxCollider2D;
						if (boxCollider2D)
						{
							Vector2 vector3 = boxCollider2D.size / 2f;
							Vector2 vector4 = (Vector2) boxCollider2D.transform.TransformPoint(boxCollider2D.offset - vector3) - myPos;
							Vector2 vector5 = (Vector2) boxCollider2D.transform.TransformPoint(boxCollider2D.offset + vector3) - myPos;
							this.del.x = vector4.x;
							this.del.y = vector4.y;
							this.TestBothSides(myPos);
							this.del.x = vector5.x;
							this.TestBothSides(myPos);
							this.del.y = vector5.y;
							this.TestBothSides(myPos);
							this.del.x = vector4.x;
							this.TestBothSides(myPos);
						}
					}
				}
			}
		}
		float num2 = this.LightRadius * 1.05f;
		for (int l = 0; l < this.requiredDels.Length; l++)
		{
			Vector2 vector6 = num2 * this.requiredDels[l];
			this.CreateVert(myPos, ref vector6);
		}
		this.verts.Sort(0, this.vertCount, LightSource.AngleComparer.Instance);
		this.myMesh.Clear();
		if (this.vec == null || this.vec.Length < this.vertCount + 1)
		{
			this.vec = new Vector3[this.vertCount + 1];
			this.uvs = new Vector2[this.vec.Length];
		}
		this.vec[0] = Vector3.zero;
		this.uvs[0] = new Vector2(this.vec[0].x, this.vec[0].y);
		for (int m = 0; m < this.vertCount; m++)
		{
			int num3 = m + 1;
			this.vec[num3] = this.verts[m].Position;
			this.uvs[num3] = new Vector2(this.vec[num3].x, this.vec[num3].y);
		}
		int num4 = this.vertCount * 3;
		if (num4 > this.triangles.Length)
		{
			this.triangles = new int[num4];
			Debug.LogWarning("Resized triangles to: " + num4.ToString());
		}
		int num5 = 0;
		for (int n = 0; n < this.triangles.Length; n += 3)
		{
			if (n < num4)
			{
				this.triangles[n] = 0;
				this.triangles[n + 1] = num5 + 1;
				if (n == num4 - 3)
				{
					this.triangles[n + 2] = 1;
				}
				else
				{
					this.triangles[n + 2] = num5 + 2;
				}
				num5++;
			}
			else
			{
				this.triangles[n] = 0;
				this.triangles[n + 1] = 0;
				this.triangles[n + 2] = 0;
			}
		}
		this.myMesh.vertices = this.vec;
		this.myMesh.uv = this.uvs;
		this.myMesh.SetIndices(this.triangles, 0, 0);
	}

	private void TestBothSides(Vector2 myPos)
	{
		float num = LightSource.length(this.del.x, this.del.x);
		this.tan.x = -this.del.y / num * this.tol;
		this.tan.y = this.del.x / num * this.tol;
		this.side.x = this.del.x + this.tan.x;
		this.side.y = this.del.y + this.tan.y;
		this.CreateVert(myPos, ref this.side);
		this.side.x = this.del.x - this.tan.x;
		this.side.y = this.del.y - this.tan.y;
		this.CreateVert(myPos, ref this.side);
	}

	private void CreateVert(Vector2 myPos, ref Vector2 del)
	{
		float num = this.LightRadius * 1.5f;
		int num2 = Physics2D.Raycast(myPos, del, this.filter, this.buffer, num);
		if (num2 > 0)
		{
			this.lightHits.Clear();
			RaycastHit2D raycastHit2D = default(RaycastHit2D);
			Collider2D collider2D = null;
			for (int i = 0; i < num2; i++)
			{
				RaycastHit2D raycastHit2D2 = this.buffer[i];
				Collider2D collider = raycastHit2D2.collider;
				OneWayShadows oneWayShadows;
				if (!LightSource.OneWayShadows.TryGetValue(collider.gameObject, out oneWayShadows) || !oneWayShadows.IsIgnored(this))
				{
					this.lightHits.Add(raycastHit2D2);
					if (!collider.isTrigger)
					{
						raycastHit2D = raycastHit2D2;
						collider2D = collider;
						break;
					}
				}
			}
			for (int j = 0; j < this.lightHits.Count; j++)
			{
				RaycastHit2D raycastHit2D3 = this.lightHits[j];
				NoShadowBehaviour noShadowBehaviour;
				if (raycastHit2D3.distance <= this.LightRadius && LightSource.NoShadows.TryGetValue(raycastHit2D3.collider.gameObject, out noShadowBehaviour))
				{
					noShadowBehaviour.didHit = true;
				}
			}
			if (collider2D && !collider2D.isTrigger)
			{
				Vector2 point = raycastHit2D.point;
				this.GetEmptyVert().Complete(point.x - myPos.x, point.y - myPos.y);
				return;
			}
		}
		Vector2 normalized = del.normalized;
		this.GetEmptyVert().Complete(normalized.x * num, normalized.y * num);
	}

	private LightSource.VertInfo GetEmptyVert()
	{
		if (this.vertCount < this.verts.Count)
		{
			List<LightSource.VertInfo> list = this.verts;
			int num = this.vertCount;
			this.vertCount = num + 1;
			return list[num];
		}
		LightSource.VertInfo vertInfo = new LightSource.VertInfo();
		this.verts.Add(vertInfo);
		this.vertCount = this.verts.Count;
		return vertInfo;
	}

	private static float length(float x, float y)
	{
		return Mathf.Sqrt(x * x + y * y);
	}

	public static float pseudoAngle(float dx, float dy)
	{
		if (dx < 0f)
		{
			float num = -dx;
			float num2 = (dy > 0f) ? dy : (-dy);
			return 2f - dy / (num + num2);
		}
		float num3 = (dy > 0f) ? dy : (-dy);
		return dy / (dx + num3);
	}

	public delegate void AddEdgeDelegate(Vector3 a, Vector3 b);

	private class VertInfo
	{
		public float Angle;

		public Vector3 Position;

		internal void Complete(float x, float y)
		{
			this.Position.x = x;
			this.Position.y = y;
			this.Angle = LightSource.pseudoAngle(y, x);
		}

		internal void Complete(Vector2 point)
		{
			this.Position.x = point.x;
			this.Position.y = point.y;
			this.Angle = LightSource.pseudoAngle(point.y, point.x);
		}
	}

	private class AngleComparer : IComparer<LightSource.VertInfo>
	{
		public static readonly LightSource.AngleComparer Instance = new LightSource.AngleComparer();

		public int Compare(LightSource.VertInfo x, LightSource.VertInfo y)
		{
			if (x.Angle > y.Angle)
			{
				return 1;
			}
			if (x.Angle >= y.Angle)
			{
				return 0;
			}
			return -1;
		}
	}

	private class HitDepthComparer : IComparer<RaycastHit2D>
	{
		public static readonly LightSource.HitDepthComparer Instance = new LightSource.HitDepthComparer();

		public int Compare(RaycastHit2D x, RaycastHit2D y)
		{
			if (x.fraction <= y.fraction)
			{
				return -1;
			}
			return 1;
		}
	}
}
