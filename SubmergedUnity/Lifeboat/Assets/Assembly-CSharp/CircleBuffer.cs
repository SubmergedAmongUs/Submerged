using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class CircleBuffer<T> : IEnumerable<T>, IEnumerable where T : class
{
	private T[] data;

	private int idx;

	private int count;

	public CircleBuffer(int size)
	{
		this.data = new T[size];
	}

	public void Add(T item)
	{
		lock (this)
		{
			for (int i = 0; i < this.count; i++)
			{
				if (this.data[this.idx] == item)
				{
					return;
				}
			}
			this.data[this.idx] = item;
			this.idx++;
			if (this.count < this.idx)
			{
				this.count = this.idx;
			}
			if (this.idx >= this.data.Length)
			{
				this.idx = 0;
			}
		}
	}

	public void Clear()
	{
		this.idx = 0;
		this.count = 0;
	}

	public IEnumerator<T> GetEnumerator()
	{
		int num;
		for (int i = 0; i < this.count; i = num)
		{
			yield return this.data[i];
			num = i + 1;
		}
		yield break;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		int num;
		for (int i = 0; i < this.count; i = num)
		{
			yield return this.data[i];
			num = i + 1;
		}
		yield break;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (T t in this.data)
		{
			stringBuilder.Append(t).Append('\n');
		}
		return stringBuilder.ToString();
	}
}
