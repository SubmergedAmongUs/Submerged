using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextMarquee : MonoBehaviour
{
	public TextMeshPro Target;

	private MeshRenderer[] allRenderers;

	private int lastChildCount;

	private string targetText;

	public float ScrollSpeed = 1f;

	public float PauseTime = 1f;

	public float AreaWidth = 3f;

	public bool IgnoreTextChanges;

	public void Start()
	{
		base.StartCoroutine(this.Run());
	}

	private void UpdateRendererList()
	{
		this.lastChildCount = this.Target.transform.childCount;
		this.allRenderers = this.Target.gameObject.GetComponentsInChildren<MeshRenderer>();
		this.allRenderers.ForEach(delegate(MeshRenderer render)
		{
			render.material.SetInt("_Mask", 4);
		});
	}

	private void Update()
	{
		if (this.Target.transform.childCount != this.lastChildCount)
		{
			this.UpdateRendererList();
		}
	}

	private IEnumerator Run()
	{
		yield return null;
		this.UpdateRendererList();
		int num;
		for (int i = 0; i < 1000; i = num)
		{
			var temp = default(Vector4);
			targetText = Target.text;
			this.allRenderers.ForEach(delegate(MeshRenderer render)
			{
				render.material.SetFloat("_VertexOffsetX", temp.x);
				render.material.SetFloat("_VertexOffsetY", temp.y);
			});
			float timer = 0f;
			while (timer < this.PauseTime && (this.IgnoreTextChanges || !(this.targetText != this.Target.text)))
			{
				yield return null;
				timer += Time.deltaTime;
			}
			timer = 0f;
			while (timer < 100f && (this.IgnoreTextChanges || !(this.targetText != this.Target.text)))
			{
				temp.x = temp.x - this.ScrollSpeed * Time.deltaTime;
				IList<MeshRenderer> self = this.allRenderers;
				Action<MeshRenderer> todo = delegate(MeshRenderer render)
				{
					render.material.SetFloat("_VertexOffsetX", temp.x);
					render.material.SetFloat("_VertexOffsetY", temp.y);
				};
				
				self.ForEach(todo);
				if (this.Target.bounds.size.x + temp.x < this.AreaWidth)
				{
					break;
				}
				yield return null;
				timer += Time.deltaTime;
			}
			timer = 0f;
			while (timer < this.PauseTime && (this.IgnoreTextChanges || !(this.targetText != this.Target.text)))
			{
				yield return null;
				timer += Time.deltaTime;
			}
			yield return null;
			num = i + 1;
		}
	}
}
