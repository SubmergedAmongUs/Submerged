using System;
using System.Collections.Generic;

public class ThreadSafeActionQueue
{
	private readonly Queue<Action> pendingCallbacks = new Queue<Action>();

	public void Enqueue(Action action)
	{
		Queue<Action> obj = this.pendingCallbacks;
		lock (obj)
		{
			this.pendingCallbacks.Enqueue(action);
		}
	}

	public void Drain()
	{
		Queue<Action> obj = this.pendingCallbacks;
		lock (obj)
		{
			while (this.pendingCallbacks.Count > 0)
			{
				this.pendingCallbacks.Dequeue()();
			}
		}
	}
}
