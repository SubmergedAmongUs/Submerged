using System;
using System.IO;
using System.Text;
using UnityEngine;

public static class FileIO
{
	public static string GetPlayerName()
	{
		return null;
	}

	public static string FilterText(string input, string inputCompo = "")
	{
		char c = ' ';
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Clear();
		foreach (char c2 in input)
		{
			if (c != ' ' || c2 != ' ')
			{
				if (c2 != '\r')
				{
				}
				if (c2 == '\b')
				{
					stringBuilder.Length = Mathf.Max(stringBuilder.Length - 1, 0);
				}
				if (FileIO.IsCharAllowed(c2))
				{
					stringBuilder.Append(c2);
					c = c2;
				}
			}
		}
		stringBuilder.Length = Mathf.Min(stringBuilder.Length, 10);
		input = stringBuilder.ToString();
		return input;
	}

	public static bool IsCharAllowed(char i)
	{
		return i == ' ' || (i >= 'A' && i <= 'Z') || (i >= 'a' && i <= 'z') || (i >= '0' && i <= '9') || (i >= 'À' && i <= 'ÿ') || (i >= 'Ѐ' && i <= 'џ') || (i >= '぀' && i <= '㆟') || (i >= 'ⱡ' && i <= '힣');
	}

	public static bool Exists(string path)
	{
		return File.Exists(path);
	}

	public static string ReadAllText(string path)
	{
		return File.ReadAllText(path);
	}

	public static byte[] ReadAllBytes(string path)
	{
		return File.ReadAllBytes(path);
	}

	public static void WriteAllText(string path, string contents)
	{
		File.WriteAllText(path, contents);
	}

	public static void WriteAllBytes(string path, byte[] bytes)
	{
		File.WriteAllBytes(path, bytes);
	}

	public static void Delete(string path)
	{
		File.Delete(path);
	}
}
