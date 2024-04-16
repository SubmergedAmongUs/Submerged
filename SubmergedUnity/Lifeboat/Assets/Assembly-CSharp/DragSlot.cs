using System;
using UnityEngine;

public class DragSlot : MonoBehaviour
{
	public Vector3 Offset;

	public Behaviour Occupant;

	public Vector3 TargetPosition
	{
		get
		{
			return base.transform.position + this.Offset;
		}
	}
}
