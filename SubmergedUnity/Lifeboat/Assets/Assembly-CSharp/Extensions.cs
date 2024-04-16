using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = System.Random;

public static class Extensions
{
	private static string[] ByteHex = (from x in Enumerable.Range(0, 256)
	select x.ToString("X2")).ToArray<string>();

	public static string RemoveAll(this string self, params char[] chars)
	{
		StringBuilder stringBuilder = new StringBuilder(self.Length);
		foreach (char value in self)
		{
			if (!chars.Contains(value))
			{
				stringBuilder.Append(value);
			}
		}
		return stringBuilder.ToString();
	}

	public static void TrimEnd(this StringBuilder self)
	{
		for (int i = self.Length - 1; i >= 0; i--)
		{
			char c = self[i];
			if (c != ' ' && c != '\t' && c != '\n' && c != '\r')
			{
				break;
			}
			int length = self.Length;
			self.Length = length - 1;
		}
	}

	public static void DestroyAll<T>(this IList<T> self) where T : MonoBehaviour
	{
		for (int i = 0; i < self.Count; i++)
		{
			 UnityEngine.Object.Destroy(self[i].gameObject);
		}
		self.Clear();
	}

	public static void AddUnique<T>(this IList<T> self, T item)
	{
		if (!self.Contains(item))
		{
			self.Add(item);
		}
	}

	public static string ToTextColor(this Color c)
	{
		return string.Concat(new string[]
		{
			"<color=#",
			Extensions.ByteHex[(int)((byte)(c.r * 255f))],
			Extensions.ByteHex[(int)((byte)(c.g * 255f))],
			Extensions.ByteHex[(int)((byte)(c.b * 255f))],
			Extensions.ByteHex[(int)((byte)(c.a * 255f))],
			">"
		});
	}

	public static Color SetAlpha(this Color c, float alpha)
	{
		return new Color(c.r, c.g, c.b, alpha);
	}

	public static int ToInteger(this Color c, bool alpha)
	{
		if (alpha)
		{
			return (int)((byte)(c.r * 256f)) << 24 | (int)((byte)(c.g * 256f)) << 16 | (int)((byte)(c.b * 256f)) << 8 | (int)((byte)(c.a * 256f));
		}
		return (int)((byte)(c.r * 256f)) << 16 | (int)((byte)(c.g * 256f)) << 8 | (int)((byte)(c.b * 256f));
	}

	public static bool HasAnyBit(this int self, int bit)
	{
		return (self & bit) != 0;
	}

	public static bool HasAnyBit(this byte self, byte bit)
	{
		return (self & bit) > 0;
	}

	public static bool HasAnyBit(this ushort self, byte bit)
	{
		return (self & (ushort)bit) > 0;
	}

	public static bool HasBit(this byte self, byte bit)
	{
		return (self & bit) == bit;
	}

	public static int BitCount(this byte self)
	{
		int num = 0;
		for (int i = 0; i < 8; i++)
		{
			if ((1 << i & (int)self) != 0)
			{
				num++;
			}
		}
		return num;
	}

	public static int IndexOf<T>(this T[] self, T item) where T : class
	{
		for (int i = 0; i < self.Length; i++)
		{
			if (self[i] == item)
			{
				return i;
			}
		}
		return -1;
	}

	public static int IndexOfMin<T>(this T[] self, Func<T, float> comparer)
	{
		float num = float.MaxValue;
		int result = -1;
		for (int i = 0; i < self.Length; i++)
		{
			float num2 = comparer(self[i]);
			if (num2 <= num)
			{
				result = i;
				num = num2;
			}
		}
		return result;
	}

	public static KeyValuePair<byte, int> MaxPair(this Dictionary<byte, int> self, out bool tie)
	{
		tie = true;
		KeyValuePair<byte, int> result = new KeyValuePair<byte, int>(byte.MaxValue, int.MinValue);
		foreach (KeyValuePair<byte, int> keyValuePair in self)
		{
			if (keyValuePair.Value > result.Value)
			{
				result = keyValuePair;
				tie = false;
			}
			else if (keyValuePair.Value == result.Value)
			{
				tie = true;
			}
		}
		return result;
	}

	public static TV GetValueOrSetDefault<TK, TV>(this Dictionary<TK, TV> self, TK key, Func<TV> defaultValueFunc)
	{
		TV tv;
		if (!self.TryGetValue(key, out tv))
		{
			tv = defaultValueFunc();
			self[key] = tv;
		}
		return tv;
	}

	public static void SetAll<T>(this IList<T> self, T value)
	{
		for (int i = 0; i < self.Count; i++)
		{
			self[i] = value;
		}
	}

	public static void AddAll<T>(this List<T> self, IList<T> other)
	{
		int num = self.Count + other.Count;
		if (self.Capacity < num)
		{
			self.Capacity = num;
		}
		for (int i = 0; i < other.Count; i++)
		{
			self.Add(other[i]);
		}
	}

	public static void RemoveDupes<T>(this IList<T> self) where T : class
	{
		for (int i = 0; i < self.Count; i++)
		{
			T t = self[i];
			for (int j = self.Count - 1; j > i; j--)
			{
				if (self[j] == t)
				{
					self.RemoveAt(j);
				}
			}
		}
	}

	public static void Shuffle<T>(this IList<T> self, int startAt = 0)
	{
		for (int i = startAt; i < self.Count - 1; i++)
		{
			T value = self[i];
			int index = UnityEngine.Random.Range(i, self.Count);
			self[i] = self[index];
			self[index] = value;
		}
	}

	public static void Shuffle<T>(this Random r, IList<T> self)
	{
		for (int i = 0; i < self.Count; i++)
		{
			T value = self[i];
			int index = r.Next(self.Count);
			self[i] = self[index];
			self[index] = value;
		}
	}

	public static T[] RandomSet<T>(this IList<T> self, int length)
	{
		T[] array = new T[length];
		self.RandomFill(array);
		return array;
	}

	public static void RandomFill<T>(this IList<T> self, T[] target)
	{
		HashSet<int> hashSet = new HashSet<int>();
		for (int i = 0; i < target.Length; i++)
		{
			int num;
			do
			{
				num = self.RandomIdx<T>();
			}
			while (hashSet.Contains(num));
			target[i] = self[num];
			hashSet.Add(num);
			if (hashSet.Count == self.Count)
			{
				return;
			}
		}
	}

	public static int RandomIdx<T>(this IList<T> self)
	{
		return UnityEngine.Random.Range(0, self.Count);
	}

	public static int RandomIdx<T>(this IEnumerable<T> self)
	{
		return UnityEngine.Random.Range(0, self.Count<T>());
	}

	public static T Random<T>(this IEnumerable<T> self)
	{
		return self.ToArray<T>().Random<T>();
	}

	public static T Random<T>(this IList<T> self)
	{
		if (self.Count > 0)
		{
			return self[UnityEngine.Random.Range(0, self.Count)];
		}
		return default(T);
	}

	public static Vector2 Div(this Vector2 a, Vector2 b)
	{
		return new Vector2(a.x / b.x, a.y / b.y);
	}

	public static Vector2 Mul(this Vector2 a, Vector2 b)
	{
		return new Vector2(a.x * b.x, a.y * b.y);
	}

	public static Vector3 Mul(this Vector3 a, Vector3 b)
	{
		return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
	}

	public static Vector3 Inv(this Vector3 a)
	{
		return new Vector3(1f / a.x, 1f / a.y, 1f / a.z);
	}

	public static Rect Lerp(this Rect source, Rect target, float t)
	{
		Rect result = default(Rect);
		result.position = Vector2.Lerp(source.position, target.position, t);
		result.size = Vector2.Lerp(source.size, target.size, t);
		return result;
	}

	public static void ForEach<T>(this IList<T> self, Action<T> todo)
	{
		for (int i = 0; i < self.Count; i++)
		{
			todo(self[i]);
		}
	}

	public static T Max<T>(this IList<T> self, Func<T, float> comparer)
	{
		T t = self.First<T>();
		float num = comparer(t);
		for (int i = 0; i < self.Count; i++)
		{
			T t2 = self[i];
			float num2 = comparer(t2);
			if (num < num2 || (num == num2 && UnityEngine.Random.value > 0.5f))
			{
				num = num2;
				t = t2;
			}
		}
		return t;
	}

	public static T Max<T>(this IList<T> self, Func<T, decimal> comparer)
	{
		T t = self.First<T>();
		decimal d = comparer(t);
		for (int i = 0; i < self.Count; i++)
		{
			T t2 = self[i];
			decimal num = comparer(t2);
			if (d < num || (d == num && UnityEngine.Random.value > 0.5f))
			{
				d = num;
				t = t2;
			}
		}
		return t;
	}

	public static int Wrap(this int self, int max)
	{
		if (self >= 0)
		{
			return self % max;
		}
		return (self + -(self / max) * max + max) % max;
	}

	public static int LastIndexOf<T>(this T[] self, Predicate<T> pred)
	{
		for (int i = self.Length - 1; i > -1; i--)
		{
			if (pred(self[i]))
			{
				return i;
			}
		}
		return -1;
	}

	public static int IndexOf<T>(this T[] self, Predicate<T> pred)
	{
		for (int i = 0; i < self.Length; i++)
		{
			if (pred(self[i]))
			{
				return i;
			}
		}
		return -1;
	}

	public static Vector2 MapToRectangle(this Vector2 del, Vector2 widthAndHeight)
	{
		del = del.normalized;
		if (Mathf.Abs(del.x) > Mathf.Abs(del.y))
		{
			return new Vector2(Mathf.Sign(del.x) * widthAndHeight.x, del.y * widthAndHeight.y / 0.70710677f);
		}
		return new Vector2(del.x * widthAndHeight.x / 0.70710677f, Mathf.Sign(del.y) * widthAndHeight.y);
	}

	public static float AngleSignedRad(this Vector2 vector1, Vector2 vector2)
	{
		return Mathf.Atan2(vector2.y, vector2.x) - Mathf.Atan2(vector1.y, vector1.x);
	}

	public static float AngleSigned(this Vector2 vector1, Vector2 vector2)
	{
		return vector1.AngleSignedRad(vector2) * 57.29578f;
	}

	public static float AngleSigned(this Vector2 vector1)
	{
		return Mathf.Atan2(vector1.y, vector1.x);
	}

	public static float WheelAngle(this Vector2 vector1, Vector2 vector2)
	{
		float num = vector1.AngleSignedRad(vector2) * 57.29578f;
		if (num > 180f)
		{
			num -= 360f;
		}
		if (num < -180f)
		{
			num += 360f;
		}
		return num;
	}

	public static Vector2 Rotate(this Vector2 self, float degrees)
	{
		float num = 0.017453292f * degrees;
		float num2 = Mathf.Cos(num);
		float num3 = Mathf.Sin(num);
		return new Vector2(self.x * num2 - num3 * self.y, self.x * num3 + num2 * self.y);
	}

	public static Vector3 RotateZ(this Vector3 self, float degrees)
	{
		float num = 0.017453292f * degrees;
		float num2 = Mathf.Cos(num);
		float num3 = Mathf.Sin(num);
		return new Vector3(self.x * num2 - num3 * self.y, self.x * num3 + num2 * self.y, self.z);
	}

	public static Vector3 RotateY(this Vector3 self, float degrees)
	{
		float num = 0.017453292f * degrees;
		float num2 = Mathf.Cos(num);
		float num3 = Mathf.Sin(num);
		return new Vector3(self.x * num2 - num3 * self.z, self.y, self.x * num3 + num2 * self.z);
	}

	public static bool TryToEnum<TEnum>(this string strEnumValue, out TEnum enumValue)
	{
		enumValue = default(TEnum);
		if (!Enum.IsDefined(typeof(TEnum), strEnumValue))
		{
			return false;
		}
		enumValue = (TEnum)((object)Enum.Parse(typeof(TEnum), strEnumValue));
		return true;
	}

	public static TEnum ToEnum<TEnum>(this string strEnumValue)
	{
		if (!Enum.IsDefined(typeof(TEnum), strEnumValue))
		{
			return default(TEnum);
		}
		return (TEnum)((object)Enum.Parse(typeof(TEnum), strEnumValue));
	}

	public static TEnum ToEnum<TEnum>(this string strEnumValue, TEnum defaultValue)
	{
		if (!Enum.IsDefined(typeof(TEnum), strEnumValue))
		{
			return defaultValue;
		}
		return (TEnum)((object)Enum.Parse(typeof(TEnum), strEnumValue));
	}

	public static bool IsNullOrWhiteSpace(this string s)
	{
		if (s == null)
		{
			return true;
		}
		return !s.Any((char c) => !char.IsWhiteSpace(c));
	}
}
