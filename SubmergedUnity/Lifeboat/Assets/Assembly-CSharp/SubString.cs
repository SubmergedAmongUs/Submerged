using System;

public struct SubString
{
	public readonly int Start;

	public readonly int Length;

	public readonly string Source;

	public SubString(string source, int start, int length)
	{
		this.Source = source;
		this.Start = start;
		this.Length = length;
	}

	public override string ToString()
	{
		return this.Source.Substring(this.Start, this.Length);
	}

	public int GetKvpValue()
	{
		int num = this.Start + this.Length;
		for (int i = this.Start; i < num; i++)
		{
			if (this.Source[i] == '=')
			{
				i++;
				return new SubString(this.Source, i, num - i).ToInt();
			}
		}
		throw new InvalidCastException();
	}

	public int ToInt()
	{
		int num = 0;
		int num2 = this.Start + this.Length;
		bool flag = false;
		for (int i = this.Start; i < num2; i++)
		{
			char c = this.Source[i];
			if (c == '-')
			{
				flag = true;
			}
			else if (c >= '0' && c <= '9')
			{
				int num3 = (int)(c - '0');
				num = 10 * num + num3;
			}
		}
		if (!flag)
		{
			return num;
		}
		return -num;
	}

	public bool StartsWith(string v)
	{
		if (v.Length > this.Length)
		{
			return false;
		}
		for (int i = 0; i < v.Length; i++)
		{
			if (this.Source[i + this.Start] != v[i])
			{
				return false;
			}
		}
		return true;
	}
}
