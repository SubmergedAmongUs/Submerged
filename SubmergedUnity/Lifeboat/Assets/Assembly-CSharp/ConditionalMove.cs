using System;
using UnityEngine;

public class ConditionalMove : MonoBehaviour
{
	public ConditionalMove.MoveForPlatforms[] moveForPlatforms = new ConditionalMove.MoveForPlatforms[0];

	private void Awake()
	{
		ConditionalMove.MoveForPlatforms[] array = this.moveForPlatforms;
		for (int i = 0; i < array.Length; i++)
		{
		}
	}

	[Serializable]
	public struct MoveForPlatforms
	{
		public RuntimePlatform runtimePlatform;

		public Vector3 offset;
	}
}
