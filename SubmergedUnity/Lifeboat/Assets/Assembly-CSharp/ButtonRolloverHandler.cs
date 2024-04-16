using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ButtonRolloverHandler : MonoBehaviour
{
	public SpriteRenderer Target;

	public TextMeshPro TargetText;

	public MeshRenderer TargetMesh;

	public Color OverColor = Color.green;

	public Color OutColor = Color.white;

	public bool UseObjectsOutColor;

	public AudioClip HoverSound;

	public void Awake()
	{
		PassiveButton component = base.GetComponent<PassiveButton>();
		if (component != null)
		{
			component.OnMouseOver.AddListener(new UnityAction(this.DoMouseOver));
			component.OnMouseOut.AddListener(new UnityAction(this.DoMouseOut));
			return;
		}
		ButtonBehavior component2 = base.GetComponent<ButtonBehavior>();
		if (component2 != null)
		{
			component2.OnMouseOver.AddListener(new UnityAction(this.DoMouseOver));
			component2.OnMouseOut.AddListener(new UnityAction(this.DoMouseOut));
			return;
		}
		SlideBar component3 = base.GetComponent<SlideBar>();
		if (component3 != null)
		{
			component3.OnMouseOver.AddListener(new UnityAction(this.DoMouseOver));
			component3.OnMouseOut.AddListener(new UnityAction(this.DoMouseOut));
			return;
		}
		if (this.UseObjectsOutColor)
		{
			if (this.Target != null)
			{
				this.OutColor = this.Target.color;
			}
			if (this.TargetText != null)
			{
				this.OutColor = this.TargetText.color;
			}
			if (this.TargetMesh != null)
			{
				this.OutColor = this.TargetMesh.material.color;
			}
		}
	}

	public void DoMouseOver()
	{
		if (this.Target != null)
		{
			this.Target.color = this.OverColor;
		}
		if (this.TargetText != null)
		{
			this.TargetText.color = this.OverColor;
		}
		if (this.TargetMesh != null)
		{
			this.TargetMesh.material.SetColor("_Color", this.OverColor);
		}
		if (this.HoverSound)
		{
			SoundManager.Instance.PlaySound(this.HoverSound, false, 1f);
		}
	}

	public void DoMouseOut()
	{
		if (this.Target != null)
		{
			this.Target.color = this.OutColor;
		}
		if (this.TargetText != null)
		{
			this.TargetText.color = this.OutColor;
		}
		if (this.TargetMesh != null)
		{
			this.TargetMesh.material.SetColor("_Color", this.OutColor);
		}
	}

	public void SetDisabledColors()
	{
		this.ChangeOutColor(Color.gray);
		this.OverColor = Color.gray;
		this.Target.color = Color.grey;
	}

	public void SetEnabledColors()
	{
		this.ChangeOutColor(Color.white);
		this.OverColor = Color.green;
		this.Target.color = Color.white;
	}

	public void ChangeOutColor(Color color)
	{
		this.OutColor = color;
		this.DoMouseOut();
	}
}
