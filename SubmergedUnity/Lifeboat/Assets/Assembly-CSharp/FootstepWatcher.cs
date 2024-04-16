using System;
using UnityEngine;

internal class FootstepWatcher : MonoBehaviour, IStepWatcher
{
	public int priority;

	public Collider2D Area;

	public SoundGroup Sounds;

	public int Priority
	{
		get
		{
			return this.priority;
		}
	}

	public SoundGroup MakeFootstep(PlayerControl player)
	{
		if (this.Area.OverlapPoint(player.GetTruePosition()))
		{
			return this.Sounds;
		}
		return null;
	}
}
