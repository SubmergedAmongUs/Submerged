using System;
using UnityEngine;

public class MapBehaviour : MonoBehaviour
{
	public static MapBehaviour Instance;

	public AlphaPulse ColorControl;

	public SpriteRenderer HerePoint;

	public MapCountOverlay countOverlay;

	public InfectedOverlay infectedOverlay;

	public MapTaskOverlay taskOverlay;

	private SpecialInputHandler specialInputHandler;

	public bool IsOpen
	{
		get
		{
			return base.isActiveAndEnabled;
		}
	}

	public bool IsOpenStopped
	{
		get
		{
			return this.IsOpen && this.countOverlay.isActiveAndEnabled;
		}
	}

	private void Awake()
	{
		MapBehaviour.Instance = this;
		this.specialInputHandler = base.GetComponent<SpecialInputHandler>();
	}

	private void GenericShow()
	{
		base.transform.localScale = Vector3.one;
		base.transform.localPosition = new Vector3(0f, 0f, -25f);
		Vector3 localScale = this.taskOverlay.transform.localScale;
		if (Mathf.Sign(localScale.x) != Mathf.Sign(ShipStatus.Instance.transform.localScale.x))
		{
			localScale.x *= -1f;
		}
		this.taskOverlay.transform.localScale = localScale;
		base.gameObject.SetActive(true);
	}

	public void ShowInfectedMap()
	{
		if (this.IsOpen)
		{
			this.Close();
			return;
		}
		if (!PlayerControl.LocalPlayer.CanMove)
		{
			return;
		}
		if (this.specialInputHandler != null)
		{
			this.specialInputHandler.disableVirtualCursor = true;
		}
		PlayerControl.LocalPlayer.SetPlayerMaterialColors(this.HerePoint);
		this.GenericShow();
		this.infectedOverlay.gameObject.SetActive(true);
		this.ColorControl.SetColor(Palette.ImpostorRed);
		this.taskOverlay.Hide();
		DestroyableSingleton<HudManager>.Instance.SetHudActive(false);
		ConsoleJoystick.SetMode_Sabotage();
	}

	public void ShowNormalMap()
	{
		if (this.IsOpen)
		{
			this.Close();
			return;
		}
		if (!PlayerControl.LocalPlayer.CanMove)
		{
			return;
		}
		PlayerControl.LocalPlayer.SetPlayerMaterialColors(this.HerePoint);
		this.GenericShow();
		this.taskOverlay.Show();
		this.ColorControl.SetColor(new Color(0.05f, 0.2f, 1f, 1f));
		DestroyableSingleton<HudManager>.Instance.SetHudActive(false);
	}

	public void ShowCountOverlay()
	{
		this.GenericShow();
		this.countOverlay.gameObject.SetActive(true);
		this.taskOverlay.Hide();
		this.HerePoint.enabled = false;
		DestroyableSingleton<HudManager>.Instance.SetHudActive(false);
	}

	public void FixedUpdate()
	{
		if (!ShipStatus.Instance)
		{
			return;
		}
		Vector3 vector = PlayerControl.LocalPlayer.transform.position;
		vector /= ShipStatus.Instance.MapScale;
		vector.x *= Mathf.Sign(ShipStatus.Instance.transform.localScale.x);
		vector.z = -1f;
		this.HerePoint.transform.localPosition = vector;
	}

	public void Close()
	{
		base.gameObject.SetActive(false);
		this.countOverlay.gameObject.SetActive(false);
		this.infectedOverlay.gameObject.SetActive(false);
		this.taskOverlay.Hide();
		this.HerePoint.enabled = true;
		DestroyableSingleton<HudManager>.Instance.SetHudActive(true);
	}
}
