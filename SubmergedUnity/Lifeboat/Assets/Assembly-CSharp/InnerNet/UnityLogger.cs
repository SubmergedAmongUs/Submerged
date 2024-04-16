using System;
using Hazel;
using UnityEngine;
using ILogger = Hazel.ILogger;

namespace InnerNet
{
	public class UnityLogger : ILogger
	{
		public void WriteVerbose(string msg)
		{
			Debug.Log(msg);
		}

		public void WriteError(string msg)
		{
			Debug.LogError(msg);
		}

		public void WriteInfo(string msg)
		{
			Debug.Log(msg);
		}
	}
}
