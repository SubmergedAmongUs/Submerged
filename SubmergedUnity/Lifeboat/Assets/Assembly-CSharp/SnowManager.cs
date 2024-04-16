using System;
using System.Collections;
using UnityEngine;

public class SnowManager : MonoBehaviour
{
	public ParticleSystem particles;

	private ParticleSystemRenderer rend;

	private float timer;

	private void Start()
	{
		this.rend = base.GetComponent<ParticleSystemRenderer>();
		base.StartCoroutine(this.Run());
	}

	private IEnumerator Run()
	{
		ContactFilter2D filter = default(ContactFilter2D);
		filter.layerMask = Constants.ShipOnlyMask;
		filter.useLayerMask = true;
		filter.useTriggers = true;
		Collider2D[] buffer = new Collider2D[10];
		WaitForSeconds wait = new WaitForSeconds(0.1f);
		for (;;)
		{
			yield return wait;
			if (PlayerControl.LocalPlayer)
			{
				bool flag = false;
				int num = PlayerControl.LocalPlayer.Collider.OverlapCollider(filter, buffer);
				for (int i = 0; i < num; i++)
				{
					if (buffer[i].tag == "NoSnow")
					{
						flag = true;
					}
				}
				if (!this.particles.isPlaying)
				{
					if (!flag)
					{
						this.particles.Play();
					}
				}
				else if (flag)
				{
					this.timer = Mathf.Max(0f, this.timer - 0.2f);
				}
				else
				{
					this.timer = Mathf.Min(1f, this.timer + 0.2f);
				}
				this.SetPartAlpha();
			}
		}
		yield break;
	}

	private void SetPartAlpha()
	{
		Color color = new Color(1f, 1f, 1f, this.timer);
		this.rend.material.SetColor("_Color", color);
	}
}
