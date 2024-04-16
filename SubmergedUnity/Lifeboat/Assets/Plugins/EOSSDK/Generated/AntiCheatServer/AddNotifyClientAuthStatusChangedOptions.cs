// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.AntiCheatServer
{
	public class AddNotifyClientAuthStatusChangedOptions
	{
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
	internal struct AddNotifyClientAuthStatusChangedOptionsInternal : ISettable, System.IDisposable
	{
		private int m_ApiVersion;

		public void Set(AddNotifyClientAuthStatusChangedOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = AntiCheatServerInterface.AddnotifyclientauthstatuschangedApiLatest;
			}
		}

		public void Set(object other)
		{
			Set(other as AddNotifyClientAuthStatusChangedOptions);
		}

		public void Dispose()
		{
		}
	}
}