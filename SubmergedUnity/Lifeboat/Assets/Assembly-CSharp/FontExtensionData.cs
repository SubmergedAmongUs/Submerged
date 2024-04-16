using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class FontExtensionData : ScriptableObject
{
	public string FontName;

	public List<KerningPair> kernings = new List<KerningPair>();

	public List<OffsetAdjustment> Offsets = new List<OffsetAdjustment>();

	public void AdjustKernings(FontData target)
	{
		for (int i = 0; i < this.kernings.Count; i++)
		{
			KerningPair kerningPair = this.kernings[i];
			Dictionary<int, float> dictionary;
			if (target.kernings.TryGetValue((int)kerningPair.First, out dictionary))
			{
				float num;
				if (dictionary.TryGetValue((int)kerningPair.Second, out num))
				{
					dictionary[(int)kerningPair.Second] = num + (float)kerningPair.Pixels;
				}
				else
				{
					dictionary[(int)kerningPair.Second] = (float)kerningPair.Pixels;
				}
			}
			else
			{
				Dictionary<int, float> dictionary2 = new Dictionary<int, float>();
				dictionary2[(int)kerningPair.Second] = (float)kerningPair.Pixels;
				target.kernings[(int)kerningPair.First] = dictionary2;
			}
		}
	}

	public void AdjustOffsets(FontData target)
	{
		for (int i = 0; i < this.Offsets.Count; i++)
		{
			OffsetAdjustment offsetAdjustment = this.Offsets[i];
			int index;
			if (target.charMap.TryGetValue((int)offsetAdjustment.Char, out index))
			{
				Vector3 value = target.offsets[index];
				value.x += (float)offsetAdjustment.OffsetX;
				value.y += (float)offsetAdjustment.OffsetY;
				target.offsets[index] = value;
			}
		}
	}
}
