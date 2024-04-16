using System;
using System.Collections;
using UnityEngine;

public class TagAmbientSoundPlayer : MonoBehaviour
{
	public AudioClip AmbientSound;

	public float MaxVolume = 1f;

	public string TargetTag = "NoSnow";

	private float targetVolume;

	public void Start()
	{
		SoundManager.Instance.PlayDynamicSound(base.name + base.GetInstanceID().ToString(), this.AmbientSound, true, new DynamicSound.GetDynamicsFunction(this.Dynamics), false);
		base.StartCoroutine(this.Run());
	}

	private void Dynamics(AudioSource source, float dt)
	{
		source.volume = Mathf.Lerp(source.volume, this.targetVolume * this.MaxVolume, dt);
	}

	public void OnDestroy()
	{
		SoundManager.Instance.StopNamedSound(base.name + base.GetInstanceID().ToString());
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
					if (buffer[i].tag == this.TargetTag)
					{
						flag = true;
						break;
					}
				}
				this.targetVolume = (float)(flag ? 0 : 1);
			}
		}
		yield break;
	}
}
