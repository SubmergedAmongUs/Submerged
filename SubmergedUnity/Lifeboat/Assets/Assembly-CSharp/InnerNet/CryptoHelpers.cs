using System;
using System.Collections.Generic;

namespace InnerNet
{
	public static class CryptoHelpers
	{
		public static byte[] DecodePEM(string pemData)
		{
			List<byte> list = new List<byte>();
			pemData = pemData.Replace("\r", "");
			foreach (string text in pemData.Split(new char[]
			{
				'\n'
			}))
			{
				if (!text.StartsWith("-----"))
				{
					byte[] collection = Convert.FromBase64String(text);
					list.AddRange(collection);
				}
			}
			return list.ToArray();
		}
	}
}
