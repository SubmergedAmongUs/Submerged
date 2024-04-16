using System;
using System.Collections;
using System.Linq;
using PowerTools;
using UnityEngine;

public class DripBehaviour : MonoBehaviour
{
	public Vector2Range SpawnArea;

	public FloatRange FirstWait = new FloatRange(0f, 3f);

	public FloatRange Frequency = new FloatRange(0.75f, 3f);

	public SpriteAnim myAnim;

	public Collider2D[] IgnoreAreas;

	public bool FixDepth = true;

	public void Start()
	{
		base.StartCoroutine(this.Run());
	}

	private IEnumerator Run()
	{
		yield return Effects.Wait(this.FirstWait.Next());
		for (;;)
		{
			Vector3 pos = this.SpawnArea.Next();
			base.transform.localPosition = pos;
			if (this.FixDepth)
			{
				pos = base.transform.position;
				pos.z = pos.y / 1000f;
				base.transform.position = pos;
			}
			if (!this.IgnoreAreas.Any((Collider2D i) => i.OverlapPoint(pos)))
			{
				this.myAnim.Play(null, 1f);
				while (this.myAnim.IsPlaying())
				{
					yield return null;
				}
				yield return Effects.Wait(this.Frequency.Next());
			}
		}
		yield break;
	}
}
