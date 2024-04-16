using System;
using System.Collections;
using Hazel;
using UnityEngine;

public class ManualDoor : SomeKindaDoor
{
	public bool Opening;

	public BoxCollider2D myCollider;

	public SpriteRenderer image;

	private float size;

	public float OpenDuration = 0.3f;

	private float openTimer;

	public AudioClip OpenSound;

	public AudioClip CloseSound;

	private void Awake()
	{
		Vector2 vector = this.myCollider.size;
		this.size = ((vector.x > vector.y) ? vector.y : vector.x);
		this.image.SetCooldownNormalizedUvs();
		this.Opening = this.myCollider.isTrigger;
	}

	private void Update()
	{
		if (this.Opening && this.openTimer < this.OpenDuration)
		{
			this.openTimer += Time.deltaTime;
			float num = Mathf.SmoothStep(0f, 1f, this.openTimer / this.OpenDuration);
			this.image.material.SetFloat("_PercentY", num);
			return;
		}
		if (!this.Opening && this.openTimer > 0f)
		{
			this.openTimer -= Time.deltaTime;
			float num2 = Mathf.SmoothStep(0f, 1f, this.openTimer / this.OpenDuration);
			this.image.material.SetFloat("_PercentY", num2);
		}
	}

	public override void SetDoorway(bool open)
	{
		if (this.Opening == open)
		{
			return;
		}
		this.Opening = open;
		this.myCollider.isTrigger = open;
		base.StopAllCoroutines();
		if (!open)
		{
			Vector2 vector = this.myCollider.size;
			base.StartCoroutine(this.CoCloseDoorway(vector.x > vector.y));
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlayDynamicSound(base.name, this.CloseSound, false, new DynamicSound.GetDynamicsFunction(this.DoorDynamics), true);
			}
			VibrationManager.Vibrate(2.5f, base.transform.position, 3f, 0f, VibrationManager.VibrationFalloff.None, this.CloseSound, false);
			return;
		}
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlayDynamicSound(base.name, this.OpenSound, false, new DynamicSound.GetDynamicsFunction(this.DoorDynamics), true);
		}
		VibrationManager.Vibrate(2.5f, base.transform.position, 3f, 0f, VibrationManager.VibrationFalloff.None, this.OpenSound, false);
	}

	private void DoorDynamics(AudioSource source, float dt)
	{
		if (!PlayerControl.LocalPlayer)
		{
			source.volume = 0f;
			return;
		}
		Vector2 vector = base.transform.position;
		Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
		float num = Vector2.Distance(vector, truePosition);
		if (num > 4f)
		{
			source.volume = 0f;
			return;
		}
		float num2 = 1f - num / 4f;
		source.volume = Mathf.Lerp(source.volume, num2, dt);
	}

	private IEnumerator CoCloseDoorway(bool isHort)
	{
		Vector2 s = this.myCollider.size;
		float i = 0f;
		if (isHort)
		{
			while (i < 0.1f)
			{
				i += Time.deltaTime;
				s.y = Mathf.Lerp(0.0001f, this.size, i / 0.1f);
				this.myCollider.size = s;
				yield return null;
			}
		}
		else
		{
			while (i < 0.1f)
			{
				i += Time.deltaTime;
				s.x = Mathf.Lerp(0.0001f, this.size, i / 0.1f);
				this.myCollider.size = s;
				yield return null;
			}
		}
		yield break;
	}

	public virtual void Serialize(MessageWriter writer)
	{
		writer.Write(this.Opening);
	}

	public virtual void Deserialize(MessageReader reader)
	{
		this.SetDoorway(reader.ReadBoolean());
	}
}
