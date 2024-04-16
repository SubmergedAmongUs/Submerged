using System;
using System.Collections;
using UnityEngine;

public abstract class OverlayAnimation : MonoBehaviour
{
	protected const float TwoFramesDelay = 0.083333336f;

	public abstract IEnumerator CoShow(KillOverlay parent);
}
