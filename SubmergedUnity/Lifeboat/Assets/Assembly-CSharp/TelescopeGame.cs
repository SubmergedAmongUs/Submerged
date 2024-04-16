using System;
using System.Collections;
using Rewired;
using UnityEngine;

public class TelescopeGame : Minigame
{
	private bool grabbed;

	public Transform Background;

	public SpriteRenderer ItemDisplay;

	public BoxCollider2D[] Items;

	private BoxCollider2D TargetItem;

	public BoxCollider2D Reticle;

	public SpriteRenderer ReticleImage;

	private Coroutine blinky;

	public AudioClip BlipSound;

	public FloatRange BlipDelay = new FloatRange(0.01f, 1f);

	private TouchpadBehavior touchpad;

	private Vector3 initialPos;

	public void Start()
	{
		this.TargetItem = this.Items.Random<BoxCollider2D>();
		this.ItemDisplay.sprite = this.TargetItem.GetComponent<SpriteRenderer>().sprite;
		base.StartCoroutine(this.RunBlipSound());
		this.touchpad = base.GetComponent<TouchpadBehavior>();
		base.SetupInput(true);
	}

	private IEnumerator RunBlipSound()
	{
		for (;;)
		{
			for (float time = 0f; time < this.BlipDelay.Lerp(Vector2.Distance(this.TargetItem.transform.position, this.Reticle.transform.position) / 10f); time += Time.deltaTime)
			{
				yield return null;
			}
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySoundImmediate(this.BlipSound, false, 1f, 1f);
			}
			VibrationManager.Vibrate(0.3f, 0.3f, 0.01f, VibrationManager.VibrationFalloff.Linear, null, false);
		}
		yield break;
	}

	public void Update()
	{
		NormalPlayerTask myNormTask = this.MyNormTask;
		if (myNormTask != null && myNormTask.IsComplete)
		{
			return;
		}
		Vector3 vector = Vector3.zero;
		Player player = ReInput.players.GetPlayer(0);
		Vector2 vector2 = new Vector2(player.GetAxis(13), player.GetAxis(14));
		if (vector2.magnitude > 0.01f)
		{
			Vector2 vector3 = -vector2 * Time.deltaTime * 3.5f;
			vector = this.Background.transform.localPosition;
			vector.x = Mathf.Clamp(vector.x + vector3.x, -6f, 6f);
			vector.y = Mathf.Clamp(vector.y + vector3.y, -7f, 7f);
			this.Background.transform.localPosition = vector;
		}
		else if (this.touchpad.IsTouching())
		{
			if (this.touchpad.IsFirstTouch())
			{
				this.initialPos = this.Background.transform.localPosition;
			}
			Vector2 touchVector = this.touchpad.GetTouchVector();
			vector.x = Mathf.Clamp(this.initialPos.x + touchVector.x, -6f, 6f);
			vector.y = Mathf.Clamp(this.initialPos.y + touchVector.y, -7f, 7f);
			this.Background.transform.localPosition = vector;
		}
		if (this.grabbed)
		{
			Controller controller = DestroyableSingleton<PassiveButtonManager>.Instance.controller;
			Vector2 vector4 = controller.DragPosition - controller.DragStartPosition;
			vector = this.Background.transform.localPosition;
			vector.x = Mathf.Clamp(vector.x + vector4.x, -6f, 6f);
			vector.y = Mathf.Clamp(vector.y + vector4.y, -7f, 7f);
			this.Background.transform.localPosition = vector;
			controller.ResetDragPosition();
		}
		if (this.Reticle.IsTouching(this.TargetItem))
		{
			if (this.blinky == null)
			{
				this.blinky = base.StartCoroutine(this.CoBlinky());
				return;
			}
		}
		else if (this.blinky != null)
		{
			base.StopCoroutine(this.blinky);
			this.blinky = null;
			this.ReticleImage.color = Color.white;
		}
	}

	private IEnumerator CoBlinky()
	{
		int num;
		for (int i = 0; i < 3; i = num)
		{
			this.ReticleImage.color = Color.green;
			yield return Effects.Wait(0.1f);
			this.ReticleImage.color = Color.white;
			yield return Effects.Wait(0.2f);
			num = i + 1;
		}
		this.blinky = null;
		this.ReticleImage.color = Color.green;
		this.MyNormTask.NextStep();
		yield return base.CoStartClose(0.75f);
		yield break;
	}

	public void Grab()
	{
		this.grabbed = !this.grabbed;
	}
}
