using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class FontLoader
{
	public static FontData FromBinary(TextAsset dataSrc, FontExtensionData eData)
	{
		FontData fontData = new FontData();
		using (MemoryStream memoryStream = new MemoryStream(dataSrc.bytes))
		{
			using (BinaryReader binaryReader = new BinaryReader(memoryStream))
			{
				if (memoryStream.ReadByte() != 66 || memoryStream.ReadByte() != 77 || memoryStream.ReadByte() != 70 || memoryStream.ReadByte() != 3)
				{
					throw new InvalidDataException("Wrong format font.");
				}
				long num = -1L;
				while (memoryStream.Position < memoryStream.Length)
				{
					if (num == memoryStream.Position)
					{
						throw new InvalidDataException("Bad font.");
					}
					num = memoryStream.Position;
					byte b = binaryReader.ReadByte();
					int num2 = binaryReader.ReadInt32();
					long position = memoryStream.Position;
					switch (b)
					{
					default:
						memoryStream.Position += (long)num2;
						break;
					case 2:
						fontData.LineHeight = (float)binaryReader.ReadUInt16();
						memoryStream.Position += 2L;
						fontData.TextureSize = new Vector2((float)binaryReader.ReadUInt16(), (float)binaryReader.ReadUInt16());
						memoryStream.Position = position + (long)num2;
						break;
					case 4:
					{
						int num3 = num2 / 20;
						fontData.charMap = new Dictionary<int, int>(num3);
						fontData.bounds.Capacity = num3;
						fontData.offsets.Capacity = num3;
						fontData.Channels.Capacity = num3;
						fontData.kernings = new Dictionary<int, Dictionary<int, float>>(256);
						for (int i = 0; i < num3; i++)
						{
							int key = binaryReader.ReadInt32();
							int num4 = (int)binaryReader.ReadUInt16();
							int num5 = (int)binaryReader.ReadUInt16();
							int num6 = (int)binaryReader.ReadUInt16();
							int num7 = (int)binaryReader.ReadUInt16();
							int num8 = (int)binaryReader.ReadInt16();
							int num9 = (int)binaryReader.ReadInt16();
							int num10 = (int)binaryReader.ReadInt16();
							binaryReader.ReadByte();
							int input = (int)binaryReader.ReadByte();
							fontData.charMap.Add(key, fontData.bounds.Count);
							fontData.bounds.Add(new Vector4((float)num4, (float)num5, (float)num6, (float)num7));
							fontData.offsets.Add(new Vector3((float)num8, (float)num9, (float)num10));
							fontData.Channels.Add(FontLoader.IntToChannels(input));
						}
						break;
					}
					case 5:
						while (memoryStream.Position < position + (long)num2)
						{
							int key2 = binaryReader.ReadInt32();
							int key3 = binaryReader.ReadInt32();
							int num11 = (int)binaryReader.ReadInt16();
							Dictionary<int, float> dictionary;
							if (!fontData.kernings.TryGetValue(key2, out dictionary))
							{
								fontData.kernings.Add(key2, dictionary = new Dictionary<int, float>(256));
							}
							dictionary.Add(key3, (float)num11);
						}
						break;
					}
				}
			}
		}
		if (eData != null)
		{
			eData.AdjustKernings(fontData);
			eData.AdjustOffsets(fontData);
		}
		return fontData;
	}

	private static Vector4 IntToChannels(int input)
	{
		Vector4 result = default(Vector4);
		for (int i = 0; i < 4; i++)
		{
			if ((input >> i & 1) == 1)
			{
				result[i] = 1f;
			}
		}
		return result;
	}
}
