using System;
using UnityEngine;

public class MeshAnimator : MonoBehaviour
{
	private MeshFilter filter;

	public Mesh[] Frames;

	public float frameRate;

	private float timer;

	private int frameId;

	private void Start()
	{
		this.filter = base.GetComponent<MeshFilter>();
	}

	private void Update()
	{
		this.timer += Time.deltaTime;
		if (this.timer > 1f / this.frameRate)
		{
			this.timer = 0f;
			this.frameId++;
			if (this.frameId >= this.Frames.Length)
			{
				this.frameId = 0;
			}
			this.filter.mesh = this.Frames[this.frameId];
		}
	}
}
