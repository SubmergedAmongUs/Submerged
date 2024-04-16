using System;
using System.Collections;
using UnityEngine;

public class SpinAnimator : MonoBehaviour
{
	public float Speed = 60f;

	public GameObject inputGlyph;

	private SpinAnimator.States curState;

	private void Update()
	{
		if (this.curState == SpinAnimator.States.Spinning)
		{
			base.transform.Rotate(0f, 0f, this.Speed * Time.deltaTime);
		}
	}

	public void Appear()
	{
		if (this.curState != SpinAnimator.States.Invisible)
		{
			return;
		}
		this.curState = SpinAnimator.States.Visible;
		base.gameObject.SetActive(true);
		this.inputGlyph.SetActive(true);
		base.StopAllCoroutines();
		base.StartCoroutine(Effects.ScaleIn(base.transform, 0f, 1f, 0.125f));
	}

	public void Disappear()
	{
		if (this.curState == SpinAnimator.States.Invisible)
		{
			return;
		}
		this.curState = SpinAnimator.States.Invisible;
		base.StopAllCoroutines();
		base.StartCoroutine(this.CoDisappear());
	}

	private IEnumerator CoDisappear()
	{
		yield return Effects.ScaleIn(base.transform, 1f, 0f, 0.125f);
		base.gameObject.SetActive(false);
		this.inputGlyph.SetActive(false);
		yield break;
	}

	public void StartPulse()
	{
		if (this.curState == SpinAnimator.States.Pulsing)
		{
			return;
		}
		this.curState = SpinAnimator.States.Pulsing;
		SpriteRenderer component = base.GetComponent<SpriteRenderer>();
		base.StartCoroutine(Effects.CycleColors(component, Color.white, Color.green, 1f, float.MaxValue));
	}

	internal void Play()
	{
		this.curState = SpinAnimator.States.Spinning;
		this.inputGlyph.SetActive(false);
	}

	private enum States
	{
		Visible,
		Invisible,
		Spinning,
		Pulsing
	}
}
