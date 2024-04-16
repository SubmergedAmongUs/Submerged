using System;
using System.Collections.Generic;

public static class MemSafeStringExtensions
{
	public static void SafeSplit(this SubString subString, List<SubString> output, char delim)
	{
		subString.Source.SafeSplit(output, delim, subString.Start, subString.Length);
	}

	public static void SafeSplit(this string source, List<SubString> output, char delim)
	{
		source.SafeSplit(output, delim, 0, source.Length);
	}

	public static void SafeSplit(this string source, List<SubString> output, char delim, int start, int length)
	{
		output.Clear();
		int num = start;
		int num2 = start + length;
		for (int i = start; i < num2; i++)
		{
			if (source[i] == delim)
			{
				if (num != i)
				{
					output.Add(new SubString(source, num, i - num));
				}
				num = i + 1;
			}
		}
		if (num != num2)
		{
			output.Add(new SubString(source, num, num2 - num));
		}
	}
}
