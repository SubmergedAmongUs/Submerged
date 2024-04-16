using System;

public class XXHash
{
	private uint seed;

	private const uint PRIME32_1 = 2654435761U;

	private const uint PRIME32_2 = 2246822519U;

	private const uint PRIME32_3 = 3266489917U;

	private const uint PRIME32_4 = 668265263U;

	private const uint PRIME32_5 = 374761393U;

	public XXHash(int seed)
	{
		this.seed = (uint)seed;
	}

	public uint GetHash(byte[] buf)
	{
		int i = 0;
		int num = buf.Length;
		uint num3;
		if (num >= 16)
		{
			int num2 = num - 16;
			uint value = this.seed + 2654435761U + 2246822519U;
			uint value2 = this.seed + 2246822519U;
			uint value3 = this.seed;
			uint value4 = this.seed - 2654435761U;
			do
			{
				value = XXHash.CalcSubHash(value, buf, i);
				i += 4;
				value2 = XXHash.CalcSubHash(value2, buf, i);
				i += 4;
				value3 = XXHash.CalcSubHash(value3, buf, i);
				i += 4;
				value4 = XXHash.CalcSubHash(value4, buf, i);
				i += 4;
			}
			while (i <= num2);
			num3 = XXHash.RotateLeft(value, 1) + XXHash.RotateLeft(value2, 7) + XXHash.RotateLeft(value3, 12) + XXHash.RotateLeft(value4, 18);
		}
		else
		{
			num3 = this.seed + 374761393U;
		}
		num3 += (uint)num;
		while (i <= num - 4)
		{
			num3 += BitConverter.ToUInt32(buf, i) * 3266489917U;
			num3 = XXHash.RotateLeft(num3, 17) * 668265263U;
			i += 4;
		}
		while (i < num)
		{
			num3 += (uint)buf[i] * 374761393U;
			num3 = XXHash.RotateLeft(num3, 11) * 2654435761U;
			i++;
		}
		num3 ^= num3 >> 15;
		num3 *= 2246822519U;
		num3 ^= num3 >> 13;
		num3 *= 3266489917U;
		return num3 ^ num3 >> 16;
	}

	public uint GetHash(params uint[] buf)
	{
		int i = 0;
		int num = buf.Length;
		uint num3;
		if (num >= 4)
		{
			int num2 = num - 4;
			uint value = this.seed + 2654435761U + 2246822519U;
			uint value2 = this.seed + 2246822519U;
			uint value3 = this.seed;
			uint value4 = this.seed - 2654435761U;
			do
			{
				value = XXHash.CalcSubHash(value, buf[i]);
				i++;
				value2 = XXHash.CalcSubHash(value2, buf[i]);
				i++;
				value3 = XXHash.CalcSubHash(value3, buf[i]);
				i++;
				value4 = XXHash.CalcSubHash(value4, buf[i]);
				i++;
			}
			while (i <= num2);
			num3 = XXHash.RotateLeft(value, 1) + XXHash.RotateLeft(value2, 7) + XXHash.RotateLeft(value3, 12) + XXHash.RotateLeft(value4, 18);
		}
		else
		{
			num3 = this.seed + 374761393U;
		}
		num3 += (uint)(num * 4);
		while (i < num)
		{
			num3 += buf[i] * 3266489917U;
			num3 = XXHash.RotateLeft(num3, 17) * 668265263U;
			i++;
		}
		num3 ^= num3 >> 15;
		num3 *= 2246822519U;
		num3 ^= num3 >> 13;
		num3 *= 3266489917U;
		return num3 ^ num3 >> 16;
	}

	public uint GetHash(params int[] buf)
	{
		int i = 0;
		int num = buf.Length;
		uint num3;
		if (num >= 4)
		{
			int num2 = num - 4;
			uint value = this.seed + 2654435761U + 2246822519U;
			uint value2 = this.seed + 2246822519U;
			uint value3 = this.seed;
			uint value4 = this.seed - 2654435761U;
			do
			{
				value = XXHash.CalcSubHash(value, (uint)buf[i]);
				i++;
				value2 = XXHash.CalcSubHash(value2, (uint)buf[i]);
				i++;
				value3 = XXHash.CalcSubHash(value3, (uint)buf[i]);
				i++;
				value4 = XXHash.CalcSubHash(value4, (uint)buf[i]);
				i++;
			}
			while (i <= num2);
			num3 = XXHash.RotateLeft(value, 1) + XXHash.RotateLeft(value2, 7) + XXHash.RotateLeft(value3, 12) + XXHash.RotateLeft(value4, 18);
		}
		else
		{
			num3 = this.seed + 374761393U;
		}
		num3 += (uint)(num * 4);
		while (i < num)
		{
			num3 += (uint)(buf[i] * -1028477379);
			num3 = XXHash.RotateLeft(num3, 17) * 668265263U;
			i++;
		}
		num3 ^= num3 >> 15;
		num3 *= 2246822519U;
		num3 ^= num3 >> 13;
		num3 *= 3266489917U;
		return num3 ^ num3 >> 16;
	}

	public uint GetHash(int buf)
	{
		uint num = XXHash.RotateLeft(this.seed + 374761393U + 4U + (uint)(buf * -1028477379), 17) * 668265263U;
		uint num2 = (num ^ num >> 15) * 2246822519U;
		uint num3 = (num2 ^ num2 >> 13) * 3266489917U;
		return num3 ^ num3 >> 16;
	}

	private static uint CalcSubHash(uint value, byte[] buf, int index)
	{
		uint num = BitConverter.ToUInt32(buf, index);
		value += num * 2246822519U;
		value = XXHash.RotateLeft(value, 13);
		value *= 2654435761U;
		return value;
	}

	private static uint CalcSubHash(uint value, uint read_value)
	{
		value += read_value * 2246822519U;
		value = XXHash.RotateLeft(value, 13);
		value *= 2654435761U;
		return value;
	}

	private static uint RotateLeft(uint value, int count)
	{
		return value << count | value >> 32 - count;
	}
}
