using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SlideBar : PassiveUiElement
{
	[Space(20f)]
	public TextMeshPro Title;

	public SpriteRenderer Bar;

	public SpriteRenderer Dot;

	public FloatRange Range;

	public bool Vertical;

	public float Value;

	public UnityEvent OnValueChange;

	private float sliderSegmentIncrement = 0.1f;

	public override bool HandleDrag
	{
		get
		{
			return true;
		}
	}

	public void OnEnable()
	{
		if (this.Title)
		{
			this.Title.color = Color.white;
		}
		if (this.Bar)
		{
			this.Bar.color = Color.white;
		}
		this.Dot.color = Color.white;
		this.UpdateValue();
	}

	public void OnDisable()
	{
		if (this.Title)
		{
			this.Title.color = Color.gray;
		}
		if (this.Bar)
		{
			this.Bar.color = Color.gray;
		}
		this.Dot.color = Color.gray;
	}

	public override void ReceiveClickDrag(Vector2 dragDelta)
	{
		Vector3 localPosition = this.Dot.transform.localPosition;
		Vector2 vector = DestroyableSingleton<PassiveButtonManager>.Instance.controller.DragPosition - (Vector2) this.Bar.transform.position;
		if (this.Vertical)
		{
			localPosition.y = this.Range.Clamp(vector.y);
			this.Value = this.Range.ReverseLerp(localPosition.y);
		}
		else
		{
			localPosition.x = this.Range.Clamp(vector.x);
			this.Value = this.Range.ReverseLerp(localPosition.x);
		}
		this.UpdateValue();
		this.OnValueChange.Invoke();
	}

	public void UpdateValue()
	{
		Vector3 localPosition = this.Dot.transform.localPosition;
		if (this.Vertical)
		{
			localPosition.y = this.Range.Lerp(this.Value);
		}
		else
		{
			localPosition.x = this.Range.Lerp(this.Value);
		}
		this.Dot.transform.localPosition = localPosition;
	}

	public void SetValue(float newValue)
	{
		this.Value = Mathf.Clamp01(newValue);
		this.UpdateValue();
	}

	public void ControllerIncrease()
	{
		float num = this.Value + this.sliderSegmentIncrement;
		this.Value = Mathf.Clamp(num, 0f, 1f);
		Debug.Log("ControllerIncrease: " + this.Value.ToString());
		this.UpdateValue();
		this.OnValueChange.Invoke();
	}

	public void ControllerDecrease()
	{
		float num = this.Value - this.sliderSegmentIncrement;
		this.Value = Mathf.Clamp(num, 0f, 1f);
		Debug.Log("ControllerDecrease: " + this.Value.ToString());
		this.UpdateValue();
		this.OnValueChange.Invoke();
	}

	private void OnValidate()
	{
		this.UpdateValue();
	}
}
