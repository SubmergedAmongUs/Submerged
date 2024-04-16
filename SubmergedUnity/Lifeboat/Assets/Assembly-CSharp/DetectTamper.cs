using System;
using UnityEngine;

public class DetectTamper
{
	public static bool Detect()
	{
		return !Application.genuineCheckAvailable || Application.genuine;
	}
}
