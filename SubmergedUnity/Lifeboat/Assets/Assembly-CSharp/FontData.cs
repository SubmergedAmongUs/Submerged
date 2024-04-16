using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FontData
{
	public Vector2 TextureSize = new Vector2(256f, 256f);

	public List<Vector4> bounds = new List<Vector4>();

	public List<Vector3> offsets = new List<Vector3>();

	public List<Vector4> Channels = new List<Vector4>();

	public Dictionary<int, int> charMap;

	public float LineHeight;

	public Dictionary<int, Dictionary<int, float>> kernings;

	public float GetKerning(int last, int cur)
	{
		Dictionary<int, float> dictionary;
		float result;
		if (this.kernings.TryGetValue(last, out dictionary) && dictionary.TryGetValue(cur, out result))
		{
			return result;
		}
		return 0f;
	}
}
