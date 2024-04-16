using System;

public static class NetHelpers
{
	public static bool SidGreaterThan(ushort newSid, ushort prevSid)
	{
		int num = prevSid + 32767;
		if (prevSid < num)
		{
			return newSid > prevSid && newSid <= num;
		}
		return newSid > prevSid || newSid <= num;
	}

	public static bool SidGreaterThan(byte newSid, byte prevSid)
	{
		int b = prevSid + 127;
		if (prevSid < b)
		{
			return newSid > prevSid && newSid <= b;
		}
		return newSid > prevSid || newSid <= b;
	}
}
