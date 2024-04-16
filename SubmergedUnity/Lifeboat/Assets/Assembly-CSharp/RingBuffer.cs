using System;

public class RingBuffer<T>
{
	private readonly T[] Data;

	private int startIdx;

	public int Count { get; private set; }

	public int Capacity
	{
		get
		{
			return this.Data.Length;
		}
	}

	public RingBuffer(int size)
	{
		this.Data = new T[size];
	}

	public T this[int i]
	{
		get
		{
			int num = (this.startIdx + i) % this.Data.Length;
			return this.Data[num];
		}
	}

	public T First()
	{
		return this.Data[this.startIdx];
	}

	public T Last()
	{
		int num = (this.startIdx + this.Count - 1) % this.Data.Length;
		return this.Data[num];
	}

	public void Add(T item)
	{
		int num = (this.startIdx + this.Count) % this.Data.Length;
		this.Data[num] = item;
		if (this.Count < this.Data.Length)
		{
			int count = this.Count;
			this.Count = count + 1;
			return;
		}
		this.startIdx = (this.startIdx + 1) % this.Data.Length;
	}

	public T RemoveFirst()
	{
		if (this.Count == 0)
		{
			throw new InvalidOperationException("Data is empty");
		}
		T result = this.Data[this.startIdx];
		this.startIdx = (this.startIdx + 1) % this.Data.Length;
		int count = this.Count;
		this.Count = count - 1;
		return result;
	}

	public void Clear()
	{
		this.startIdx = 0;
		this.Count = 0;
	}
}
