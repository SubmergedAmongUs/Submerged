using System;
using System.Runtime.InteropServices;
using UnityEngine;

public static class ClipboardHelper
{
	private const uint CF_TEXT = 1U;

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool IsClipboardFormatAvailable(uint format);

	[DllImport("user32.dll")]
	private static extern bool OpenClipboard(IntPtr hWndNewOwner);

	[DllImport("user32.dll")]
	private static extern bool CloseClipboard();

	[DllImport("user32.dll")]
	private static extern IntPtr GetClipboardData(uint format);

	[DllImport("kernel32.dll")]
	private static extern IntPtr GlobalLock(IntPtr hMem);

	[DllImport("kernel32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GlobalUnlock(IntPtr hMem);

	[DllImport("kernel32.dll")]
	private static extern int GlobalSize(IntPtr hMem);

	public static string GetClipboardString()
	{
		if (!ClipboardHelper.IsClipboardFormatAvailable(1U))
		{
			return null;
		}
		string result;
		try
		{
			if (!ClipboardHelper.OpenClipboard(IntPtr.Zero))
			{
				result = null;
			}
			else
			{
				IntPtr clipboardData = ClipboardHelper.GetClipboardData(1U);
				if (clipboardData == IntPtr.Zero)
				{
					result = null;
				}
				else
				{
					IntPtr intPtr = IntPtr.Zero;
					try
					{
						intPtr = ClipboardHelper.GlobalLock(clipboardData);
						int len = ClipboardHelper.GlobalSize(clipboardData);
						result = Marshal.PtrToStringAnsi(clipboardData, len);
					}
					finally
					{
						if (intPtr != IntPtr.Zero)
						{
							ClipboardHelper.GlobalUnlock(intPtr);
						}
					}
				}
			}
		}
		finally
		{
			ClipboardHelper.CloseClipboard();
		}
		return result;
	}

	public static void PutClipboardString(string str)
	{
		GUIUtility.systemCopyBuffer = str;
	}
}
