using System;
using System.Collections.Generic;

public class SequenceBuffer<T>
{
	private readonly List<SequenceBuffer<T>.SequencedData<T>> buffer = new List<SequenceBuffer<T>.SequencedData<T>>();

	public ushort LastSid { get; set; }

	public SequenceBuffer(ushort sidStart = 0)
	{
		this.LastSid = sidStart;
	}

	public void Add(ushort sid, T info)
	{
		this.buffer.Add(new SequenceBuffer<T>.SequencedData<T>(sid, info));
	}

	public void BumpSid()
	{
		ushort lastSid = this.LastSid;
		this.LastSid = (ushort) (lastSid + 1);
	}

	public bool IsInvalidSid(ushort sid)
	{
		return !NetHelpers.SidGreaterThan(sid, this.LastSid);
	}

	public bool IsNextSid(ushort sid)
	{
		return sid == this.LastSid + 1;
	}

	public IEnumerable<T> SubsequentObjs()
	{
		this.Sort();
		while (this.HasElements() && this.IsNextSid(this.Peek().Sid))
		{
			yield return this.Pop().Data;
		}
		yield break;
	}

	private void Sort()
	{
		this.buffer.Sort();
	}

	private bool HasElements()
	{
		return this.buffer.Count > 0;
	}

	private SequenceBuffer<T>.SequencedData<T> Pop()
	{
		if (!this.HasElements())
		{
			throw new InvalidOperationException("No elements to pop");
		}
		SequenceBuffer<T>.SequencedData<T> result = this.Peek();
		this.buffer.RemoveAt(0);
		return result;
	}

	private SequenceBuffer<T>.SequencedData<T> Peek()
	{
		if (!this.HasElements())
		{
			throw new InvalidOperationException("No elements to peek");
		}
		return this.buffer[0];
	}

	private struct SequencedData<T> : IComparable<SequenceBuffer<T>.SequencedData<T>>
	{
		public readonly ushort Sid;

		public readonly T Data;

		public SequencedData(ushort sid, T data)
		{
			this.Sid = sid;
			this.Data = data;
		}

		public int CompareTo(SequenceBuffer<T>.SequencedData<T> other)
		{
			return this.Sid.CompareTo(other.Sid);
		}
	}
}
