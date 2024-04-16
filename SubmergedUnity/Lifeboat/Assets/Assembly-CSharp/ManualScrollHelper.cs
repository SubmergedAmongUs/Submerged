using System;
using Rewired;
using UnityEngine;

[RequireComponent(typeof(Scroller))]
public class ManualScrollHelper : MonoBehaviour
{
	public bool doVertical = true;

	public RewiredConstsEnum.Action verticalAxis;

	public bool doHorizontal;

	public RewiredConstsEnum.Action horizontalAxis;

	public float scrollSpeed = 3f;

	private Scroller scroller;

	private void Start()
	{
		this.scroller = base.GetComponent<Scroller>();
		if (!this.scroller)
		{
			base.enabled = false;
		}
	}

	private void Update()
	{
		Vector3 localPosition = this.scroller.Inner.transform.localPosition;
		Player player = ReInput.players.GetPlayer(0);
		if (this.doVertical)
		{
			float axisRaw = player.GetAxisRaw((int)this.verticalAxis);
			localPosition.y -= axisRaw * Time.deltaTime * this.scrollSpeed;
		}
		if (this.doHorizontal)
		{
			float axisRaw2 = player.GetAxisRaw((int)this.horizontalAxis);
			localPosition.x -= axisRaw2 * Time.deltaTime * this.scrollSpeed;
		}
		localPosition.x = this.scroller.XBounds.Clamp(localPosition.x);
		localPosition.y = this.scroller.YBounds.Clamp(localPosition.y);
		this.scroller.Inner.transform.localPosition = localPosition;
	}
}
