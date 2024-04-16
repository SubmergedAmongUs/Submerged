using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlatformIdentifierIcon : MonoBehaviour
{
	private SpriteRenderer sr;

	private TextMeshPro tr;

	public bool isOnHUD;

	public Sprite genericConsole;

	public Sprite mobile;

	public Sprite pc;

	public Sprite nSwitch;

	public Sprite ps4;

	public Sprite xbox;

	public void Awake()
	{
		this.sr = base.GetComponent<SpriteRenderer>();
		this.sr.enabled = false;
		this.tr = base.GetComponentInParent<TextMeshPro>();
	}

	public void SetIcon(RuntimePlatform platform)
	{
		this.sr.enabled = false;
	}

	[ContextMenu("Update pos")]
	public void UpdatePosition()
	{
		Vector3 localPosition = base.transform.localPosition;
		localPosition.x = -this.tr.bounds.size.x - base.transform.lossyScale.x * this.sr.sprite.rect.width / this.sr.sprite.pixelsPerUnit;
		base.transform.localPosition = localPosition;
	}

	private Sprite GetPrevalidationIcon(RuntimePlatform platform)
	{
		if (platform <= (RuntimePlatform) 11)
		{
			if (platform == (RuntimePlatform) 8 || platform == (RuntimePlatform) 11)
			{
				return this.mobile;
			}
		}
		else
		{
			if (platform == (RuntimePlatform) 25)
			{
				return this.ps4;
			}
			if (platform == (RuntimePlatform) 27)
			{
				return this.xbox;
			}
			if (platform == (RuntimePlatform) 32)
			{
				return this.nSwitch;
			}
		}
		return this.pc;
	}

	private Sprite ValidateIcon(Sprite icon)
	{
		if (icon == this.ps4 || icon == this.xbox || icon == this.nSwitch)
		{
			return this.genericConsole;
		}
		return icon;
	}
}
