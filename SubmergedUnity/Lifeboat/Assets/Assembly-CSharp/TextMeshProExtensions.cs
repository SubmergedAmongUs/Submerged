using System;
using System.Linq;
using TMPro;
using UnityEngine;

public static class TextMeshProExtensions
{
	public static float GetNotDumbRenderedHeight(this TextMeshPro self)
	{
		if ((double)self.renderedHeight > 100000000.0 || (double)self.renderedHeight < -100000000.0)
		{
			return 0f;
		}
		return self.renderedHeight;
	}

	public static Vector2 CursorPos(this TextMeshPro self)
	{
		if (self.textInfo == null || self.textInfo.lineCount == 0 || self.textInfo.lineInfo[0].characterCount <= 0)
		{
			return Vector2.zero;
		}
		return self.textInfo.lineInfo.Last((TMP_LineInfo l) => l.characterCount > 0).lineExtents.max;
	}

	public static bool GetWordPosition(this TextMeshPro self, string str, out Vector3 bottomLeft, out Vector3 topRight)
	{
		int num = self.text.IndexOf(str);
		if (num != -1)
		{
			TMP_CharacterInfo tmp_CharacterInfo = self.textInfo.characterInfo[num];
			TMP_CharacterInfo tmp_CharacterInfo2 = self.textInfo.characterInfo[num + str.Length - 1];
			bottomLeft = tmp_CharacterInfo.bottomLeft;
			topRight = tmp_CharacterInfo2.topRight;
			bottomLeft.z = (topRight.z = self.transform.localPosition.z);
			return true;
		}
		bottomLeft = Vector3.zero;
		topRight = Vector3.zero;
		return false;
	}
}
