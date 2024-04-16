using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class SecurityLogger : MonoBehaviour
{
	private static Collider2D[] hits = new Collider2D[15];

	public SecurityLogBehaviour LogParent;

	public SecurityLogBehaviour.SecurityLogLocations MyLocation;

	public float Cooldown = 5f;

	public SpriteRenderer Image;

	public BoxCollider2D Sensor;

	private float[] Timers = new float[15];

	private ContactFilter2D filter;

	private void Awake()
	{
		this.filter = default(ContactFilter2D);
		this.filter.useLayerMask = true;
		this.filter.layerMask = Constants.PlayersOnlyMask;
	}

	public void FixedUpdate()
	{
		for (int j = 0; j < this.Timers.Length; j++)
		{
			this.Timers[j] -= Time.deltaTime;
		}
		if (PlayerControl.LocalPlayer && PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
		{
			return;
		}
		int num = this.Sensor.OverlapCollider(this.filter, SecurityLogger.hits);
		int i;
		int i2;
		for (i = 0; i < num; i = i2)
		{
			PlayerControl playerControl = PlayerControl.AllPlayerControls.FirstOrDefault((PlayerControl p) => p.Collider == SecurityLogger.hits[i]);
			if (playerControl && playerControl.Data != null && !playerControl.Data.IsDead && this.Timers[(int)playerControl.PlayerId] < 0f)
			{
				this.Timers[(int)playerControl.PlayerId] = this.Cooldown;
				this.LogParent.LogPlayer(playerControl, this.MyLocation);
				base.StopAllCoroutines();
				base.StartCoroutine(this.BlinkSensor());
			}
			i2 = i + 1;
		}
	}

	private IEnumerator BlinkSensor()
	{
		yield return Effects.Wait(0.1f);
		this.Image.color = this.LogParent.BarColors[(int)((byte)this.MyLocation)];
		yield return Effects.Wait(0.1f);
		this.Image.color = new Color(1f, 1f, 1f, 0.5f);
		yield break;
	}
}
