using System;
using System.Collections.Generic;
using System.Linq;

public class RandomFill<T>
{
	private T[] values;

	private int idx;

	public RandomFill()
	{
	}

	public RandomFill(IEnumerable<T> set)
	{
		this.Set(set);
	}

	public void Set(IEnumerable<T> values)
	{
		if (this.values == null)
		{
			this.values = values.ToArray<T>();
			this.values.Shuffle(0);
			this.idx = this.values.Length - 1;
		}
	}

	public T Get()
	{
		if (this.idx < 0)
		{
			this.values.Shuffle(0);
			this.idx = this.values.Length - 1;
		}
		T[] array = this.values;
		int num = this.idx;
		this.idx = num - 1;
		return array[num];
	}
}
