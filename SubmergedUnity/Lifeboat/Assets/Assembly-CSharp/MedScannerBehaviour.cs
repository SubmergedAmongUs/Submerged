using System;
using UnityEngine;

public class MedScannerBehaviour : MonoBehaviour
{
	public Vector3 Offset;

	public Vector3 Position
	{
		get
		{
			return base.transform.position + this.Offset;
		}
	}
}
