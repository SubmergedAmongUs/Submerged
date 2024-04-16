using System;
using PowerTools;
using UnityEngine;

public class ReactorShipRoom : SkeldShipRoom
{
	public Sprite normalManifolds;

	public Sprite meltdownManifolds;

	public SpriteRenderer Manifolds;

	public AnimationClip normalReactor;

	public AnimationClip meltdownReactor;

	public SpriteAnim Reactor;

	public AnimationClip normalHighFloor;

	public AnimationClip meltdownHighFloor;

	public SpriteAnim HighFloor;

	public AnimationClip normalMidFloor;

	public AnimationClip meltdownMidFloor;

	public SpriteAnim MidFloor1;

	public SpriteAnim MidFloor2;

	public AnimationClip normalLowFloor;

	public AnimationClip meltdownLowFloor;

	public SpriteAnim LowFloor;

	public AnimationClip[] normalPipes;

	public AnimationClip[] meltdownPipes;

	public SpriteAnim[] Pipes;

	public SupressorBehaviour[] Supressors;

	public MeshRenderer Orb;

	public Rotater OrbGlass;

	public ChainBehaviour[] Arms;

	public void StartMeltdown()
	{
		if (this.Manifolds)
		{
			this.Manifolds.sprite = this.meltdownManifolds;
		}
		if (this.Reactor)
		{
			this.Reactor.Play(this.meltdownReactor, 1f);
		}
		if (this.HighFloor)
		{
			this.HighFloor.Play(this.meltdownHighFloor, 1f);
		}
		if (this.MidFloor1)
		{
			this.MidFloor1.Play(this.meltdownMidFloor, 1f);
		}
		if (this.MidFloor2)
		{
			this.MidFloor2.Play(this.meltdownMidFloor, 1f);
		}
		if (this.LowFloor)
		{
			this.LowFloor.Play(this.meltdownLowFloor, 1f);
		}
		for (int i = 0; i < this.Pipes.Length; i++)
		{
			this.Pipes[i].Play(this.meltdownPipes[i], 1f);
		}
		for (int j = 0; j < this.Supressors.Length; j++)
		{
			this.Supressors[j].Deactivate();
		}
		if (this.Orb)
		{
			this.Orb.material.SetColor("_Color", Color.red);
			this.Orb.material.SetFloat("_Speed", 3f);
		}
		if (this.OrbGlass)
		{
			this.OrbGlass.DegreesPerSecond = 1440f;
		}
		for (int k = 0; k < this.Arms.Length; k++)
		{
			this.Arms[k].SwingRange.min = -0.75f;
			this.Arms[k].SwingRange.max = 0.75f;
		}
	}

	public void StopMeltdown()
	{
		if (this.Manifolds)
		{
			this.Manifolds.sprite = this.normalManifolds;
		}
		if (this.Reactor)
		{
			this.Reactor.Play(this.normalReactor, 1f);
		}
		if (this.HighFloor)
		{
			this.HighFloor.Play(this.normalHighFloor, 1f);
		}
		if (this.MidFloor1)
		{
			this.MidFloor1.Play(this.normalMidFloor, 1f);
		}
		if (this.MidFloor2)
		{
			this.MidFloor2.Play(this.normalMidFloor, 1f);
		}
		if (this.LowFloor)
		{
			this.LowFloor.Play(this.normalLowFloor, 1f);
		}
		for (int i = 0; i < this.Pipes.Length; i++)
		{
			this.Pipes[i].Play(this.normalPipes[i], 1f);
		}
		for (int j = 0; j < this.Supressors.Length; j++)
		{
			this.Supressors[j].Activate();
		}
		if (this.Orb)
		{
			this.Orb.material.SetColor("_Color", Color.white);
			this.Orb.material.SetFloat("_Speed", 1f);
		}
		if (this.OrbGlass)
		{
			this.OrbGlass.DegreesPerSecond = 720f;
		}
		for (int k = 0; k < this.Arms.Length; k++)
		{
			this.Arms[k].SwingRange.min = -0.15f;
			this.Arms[k].SwingRange.max = 0.15f;
		}
	}
}
