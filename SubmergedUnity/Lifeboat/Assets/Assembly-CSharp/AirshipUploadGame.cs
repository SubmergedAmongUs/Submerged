using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class AirshipUploadGame : Minigame
{
	public SpriteRenderer Phone;

	public Collider2D Hotspot;

	public Collider2D Perfect;

	public Collider2D Good;

	public Collider2D Poor;

	public GameObject PerfectIcon;

	public GameObject GoodIcon;

	public GameObject PoorIcon;

	public GameObject NoneIcon;

	public HorizontalGauge gauge;

	public float moveSpeed = 1f;

	private const float MaxTimer = 20f;

	private float timer;

	public AudioClip nearSound;

	public float BeepPeriod = 0.5f;

	private float beepTimer;

	public Controller cont = new Controller();

	public SpriteRenderer promptGlyph;

	private Color glyphColor;

	private float glyphDisappearDelay = 5f;

	private bool phoneGrabbed;

	public void Start()
	{
		this.Hotspot.transform.localPosition = UnityEngine.Random.insideUnitCircle.normalized * 2.5f;
		PlayerControl.LocalPlayer.SetPlayerMaterialColors(this.Phone);
		base.SetupInput(false);
		this.glyphColor = this.promptGlyph.color;
	}

	public void Update()
	{
		if (this.amClosing != Minigame.CloseState.None)
		{
			return;
		}
		float num = 0f;
		if (this.Hotspot.IsTouching(this.Perfect))
		{
			if (!this.PerfectIcon.activeSelf)
			{
				this.DeactivateIcons();
				this.PerfectIcon.SetActive(true);
			}
			num = Time.deltaTime * 4f;
		}
		else if (this.Hotspot.IsTouching(this.Good))
		{
			if (!this.GoodIcon.activeSelf)
			{
				this.DeactivateIcons();
				this.GoodIcon.SetActive(true);
			}
			num = Time.deltaTime * 2f;
		}
		else if (this.Hotspot.IsTouching(this.Poor))
		{
			if (!this.PoorIcon.activeSelf)
			{
				this.DeactivateIcons();
				this.PoorIcon.SetActive(true);
			}
			num = Time.deltaTime;
		}
		else if (!this.NoneIcon.activeSelf)
		{
			this.DeactivateIcons();
			this.NoneIcon.SetActive(true);
		}
		this.cont.Update();
		if (this.glyphColor.a > 0f)
		{
			if (this.glyphDisappearDelay > 0f)
			{
				this.glyphDisappearDelay -= Time.deltaTime;
			}
			else
			{
				this.promptGlyph.color = this.glyphColor;
				this.glyphColor.a = this.glyphColor.a - Time.deltaTime;
			}
		}
		else if (this.promptGlyph.gameObject.activeSelf)
		{
			this.promptGlyph.gameObject.SetActive(false);
		}
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			this.Phone.transform.position = VirtualCursor.currentPosition;
		}
		this.timer += num;
		if (Constants.ShouldPlaySfx())
		{
			this.beepTimer += (10f - Vector2.Distance(this.Hotspot.transform.localPosition, this.Phone.transform.localPosition)) * num / this.BeepPeriod;
			if (this.beepTimer >= 1f)
			{
				this.beepTimer = 0f;
				SoundManager.Instance.PlaySoundImmediate(this.nearSound, false, 1f, 1f);
			}
		}
		if (this.phoneGrabbed)
		{
			Vector3 vector = this.Phone.transform.position;
			float z = vector.z;
			vector = DestroyableSingleton<PassiveButtonManager>.Instance.controller.Touches[0].Position;
			vector.z = z;
			this.Phone.transform.position = vector;
		}
		this.gauge.Value = this.timer / 20f;
		if (this.timer >= 20f)
		{
			this.MyNormTask.NextStep();
			this.Close();
		}
	}

	public void ToggleGrab()
	{
		this.phoneGrabbed = !this.phoneGrabbed;
	}

	private void DeactivateIcons()
	{
		this.PerfectIcon.SetActive(false);
		this.GoodIcon.SetActive(false);
		this.PoorIcon.SetActive(false);
		this.NoneIcon.SetActive(false);
	}
}
