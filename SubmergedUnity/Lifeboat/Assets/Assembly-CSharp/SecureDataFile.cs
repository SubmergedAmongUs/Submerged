using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SecureDataFile
{
	private string filePath;

	public bool Loaded { get; private set; }

	public SecureDataFile(string filePath)
	{
		this.filePath = filePath;
	}

	public void LoadData(Action<BinaryReader> performRead)
	{
		this.Loaded = true;
		Debug.Log("Loading secure: " + this.filePath);
		if (FileIO.Exists(this.filePath))
		{
			byte[] array;
			try
			{
				array = FileIO.ReadAllBytes(this.filePath);
				for (int i = 0; i < array.Length; i++)
				{
					byte[] array2 = array;
					int num = i;
					array2[num] ^= (byte)(i % 212);
				}
			}
			catch
			{
				Debug.Log("Couldn't read secure file");
				this.Delete();
				return;
			}
			try
			{
				using (MemoryStream memoryStream = new MemoryStream(array))
				{
					using (BinaryReader binaryReader = new BinaryReader(memoryStream))
					{
						if (binaryReader.ReadString() != SystemInfo.deviceUniqueIdentifier)
						{
							Debug.Log("Invalid secure file");
							this.Delete();
						}
						else
						{
							performRead(binaryReader);
						}
					}
				}
			}
			catch
			{
				Debug.Log("Deleted corrupt secure file inner");
				this.Delete();
			}
		}
	}

	public void SaveData(params object[] items)
	{
		byte[] array;
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
			{
				binaryWriter.Write(SystemInfo.deviceUniqueIdentifier);
				foreach (object obj in items)
				{
					if (obj is long)
					{
						binaryWriter.Write((long)obj);
					}
					else
					{
						if (obj is HashSet<string>)
						{
							using (HashSet<string>.Enumerator enumerator = ((HashSet<string>)obj).GetEnumerator())
							{
								while (enumerator.MoveNext())
								{
									string value = enumerator.Current;
									binaryWriter.Write(value);
								}
								goto IL_96;
							}
						}
						if (obj is string)
						{
							binaryWriter.Write((string)obj);
						}
					}
					IL_96:;
				}
				binaryWriter.Flush();
				memoryStream.Position = 0L;
				array = memoryStream.ToArray();
			}
		}
		for (int j = 0; j < array.Length; j++)
		{
			byte[] array2 = array;
			int num = j;
			array2[num] ^= (byte)(j % 212);
		}
		FileIO.WriteAllBytes(this.filePath, array);
	}

	public void Delete()
	{
		try
		{
			FileIO.Delete(this.filePath);
		}
		catch
		{
		}
	}
}
