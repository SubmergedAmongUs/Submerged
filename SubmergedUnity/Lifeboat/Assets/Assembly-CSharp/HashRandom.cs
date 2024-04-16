using System;

public static class HashRandom
{
	private static XXHash src = new XXHash((int)DateTime.UtcNow.Ticks);

	private static int cnt = 0;

	public static uint Next()
	{
		return HashRandom.src.GetHash(HashRandom.cnt++);
	}

	public static int FastNext(int maxInt)
	{
		return (int)((ulong)HashRandom.Next() % (ulong)((long)maxInt));
	}

	public static int Next(int maxInt)
	{
		uint num = (uint)(-1 / maxInt);
		uint num2 = num * (uint)maxInt;
		uint num3;
		do
		{
			num3 = HashRandom.Next();
		}
		while (num3 > num2);
		return (int)(num3 / num);
	}

	public static int Next(int minInt, int maxInt)
	{
		return HashRandom.Next(maxInt - minInt) + minInt;
	}
}
