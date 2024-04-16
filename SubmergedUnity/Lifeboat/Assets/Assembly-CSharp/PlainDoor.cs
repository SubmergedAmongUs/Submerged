using System;
using System.Collections;
using Hazel;
using PowerTools;
using UnityEngine;

public class PlainDoor : SomeKindaDoor
{
	public SystemTypes Room;

	public int Id;

	public bool Open;

	public BoxCollider2D myCollider;

	public SpriteAnim animator;

	public AnimationClip OpenDoorAnim;

	public AnimationClip CloseDoorAnim;

	public AudioClip OpenSound;

	public AudioClip CloseSound;

	private float size;

	private void Start()
	{
		Vector2 vector = this.myCollider.size;
		this.size = ((vector.x > vector.y) ? vector.y : vector.x);
		this.Open = this.myCollider.isTrigger;
		this.animator.Play(this.Open ? this.OpenDoorAnim : this.CloseDoorAnim, 1000f);
	}

	public override void SetDoorway(bool open)
	{
		if (this.Open == open)
		{
			return;
		}
		this.Open = open;
		this.myCollider.isTrigger = open;
		this.animator.Play(open ? this.OpenDoorAnim : this.CloseDoorAnim, 1f);
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

	public virtual void Serialize(MessageWriter writer)
	{
		writer.Write(this.Open);
	}

	public virtual void Deserialize(MessageReader reader)
	{
		this.SetDoorway(reader.ReadBoolean());
	}

	public virtual bool DoUpdate(float dt)
	{
		return false;
	}
}
