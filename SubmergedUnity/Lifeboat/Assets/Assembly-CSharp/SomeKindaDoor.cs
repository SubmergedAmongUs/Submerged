using System;
using UnityEngine;

public abstract class SomeKindaDoor : MonoBehaviour
{
	public const float vibrationIntensity = 2.5f;

	public abstract void SetDoorway(bool open);
}
