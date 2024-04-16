using System;
using UnityEngine;

public class SpecialInputHandler : MonoBehaviour
{
	public static int count;

	public static int disableVirtualCursorCount;

	[SerializeField]
	private bool _disableVirtualCursor;

	public bool disableVirtualCursor
	{
		get
		{
			return this._disableVirtualCursor;
		}
		set
		{
			if (this._disableVirtualCursor != value)
			{
				this._disableVirtualCursor = value;
				if (base.isActiveAndEnabled)
				{
					if (this._disableVirtualCursor)
					{
						SpecialInputHandler.disableVirtualCursorCount++;
						return;
					}
					SpecialInputHandler.disableVirtualCursorCount--;
				}
			}
		}
	}

	private void OnEnable()
	{
		SpecialInputHandler.count++;
		if (this.disableVirtualCursor)
		{
			SpecialInputHandler.disableVirtualCursorCount++;
		}
	}

	private void OnDisable()
	{
		SpecialInputHandler.count--;
		if (this.disableVirtualCursor)
		{
			SpecialInputHandler.disableVirtualCursorCount--;
		}
	}
}
