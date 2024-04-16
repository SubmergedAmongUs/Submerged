using System;

public class SubStringReader
{
	private readonly string Source;

	private int Position;

	public SubStringReader(string source)
	{
		this.Source = source;
	}

	public SubString ReadLine()
	{
		int position = this.Position;
		if (position >= this.Source.Length)
		{
			return default(SubString);
		}
		int num = this.Position;
		int i = position;
		while (i < this.Source.Length)
		{
			char c = this.Source[i];
			if (c == '\r')
			{
				num = i - 1;
				this.Position = i + 1;
				if (i + 1 < this.Source.Length && this.Source[i + 1] == '\n')
				{
					this.Position = i + 2;
					break;
				}
				break;
			}
			else
			{
				if (c == '\n')
				{
					num = i - 1;
					this.Position = i + 1;
					break;
				}
				this.Position++;
				i++;
			}
		}
		return new SubString(this.Source, position, num - position);
	}
}
