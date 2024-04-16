// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.Sessions
{
	/// <summary>
	/// Input parameters for the <see cref="SessionsInterface.AddNotifySessionInviteReceived" /> function.
	/// </summary>
	public class AddNotifySessionInviteReceivedOptions
	{
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
	internal struct AddNotifySessionInviteReceivedOptionsInternal : ISettable, System.IDisposable
	{
		private int m_ApiVersion;

		public void Set(AddNotifySessionInviteReceivedOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = SessionsInterface.AddnotifysessioninvitereceivedApiLatest;
			}
		}

		public void Set(object other)
		{
			Set(other as AddNotifySessionInviteReceivedOptions);
		}

		public void Dispose()
		{
		}
	}
}