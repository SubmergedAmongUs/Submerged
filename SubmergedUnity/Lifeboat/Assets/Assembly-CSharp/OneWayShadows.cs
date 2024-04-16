using System;
using UnityEngine;

public class OneWayShadows : MonoBehaviour
{
	public Collider2D RoomCollider;

	public bool IgnoreImpostor;

	public void OnEnable()
	{
		LightSource.OneWayShadows.Add(base.gameObject, this);
	}

	public void OnDisable()
	{
		LightSource.OneWayShadows.Remove(base.gameObject);
	}

	public bool IsIgnored(LightSource lightSource)
	{
		return (this.IgnoreImpostor && PlayerControl.LocalPlayer.Data.IsImpostor) || this.RoomCollider.OverlapPoint(lightSource.transform.position);
	}
}
