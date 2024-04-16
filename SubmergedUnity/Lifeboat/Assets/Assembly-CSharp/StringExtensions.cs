using System;
using UnityEngine;

public static class StringExtensions
{
	private static char[] buffer = new char[256];

	public static string Lerp(string a, string b, float t)
	{
		int num = Mathf.Max(a.Length, b.Length);
		int num2 = (int)Mathf.Lerp(0f, (float)num, t);
		for (int i = 0; i < num; i++)
		{
			if (i < num2)
			{
				if (i < b.Length)
				{
					StringExtensions.buffer[i] = b[i];
				}
				else
				{
					StringExtensions.buffer[i] = ' ';
				}
			}
			else if (i < a.Length)
			{
				StringExtensions.buffer[i] = a[i];
			}
		}
		return new string(StringExtensions.buffer, 0, num);
	}

	public static string Color(this string toColor, string color)
	{
		return string.Concat(new string[]
		{
			"<color=",
			color,
			">",
			toColor,
			"</color>"
		});
	}
}
