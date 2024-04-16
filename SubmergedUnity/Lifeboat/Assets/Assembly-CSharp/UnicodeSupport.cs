using System;
using System.Text;
using TMPro;

public static class UnicodeSupport
{
	private static StringBuilder sb = new StringBuilder();

	public static string FilterUnsupportedCharacters(TextMeshPro tr, FontData data)
	{
		UnicodeSupport.sb.Clear();
		for (int i = 0; i < tr.text.Length; i++)
		{
			if (tr.text[i] > ' ' && !data.charMap.ContainsKey((int)tr.text[i]))
			{
				UnicodeSupport.sb.Append('□');
			}
			else
			{
				UnicodeSupport.sb.Append(tr.text[i]);
			}
		}
		return UnicodeSupport.sb.ToString();
	}
}
