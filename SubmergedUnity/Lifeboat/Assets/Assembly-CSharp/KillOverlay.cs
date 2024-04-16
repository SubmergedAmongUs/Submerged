using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillOverlay : MonoBehaviour
{
	public SpriteRenderer background;

	public GameObject flameParent;

	public OverlayKillAnimation[] KillAnims;

	private Queue<Func<IEnumerator>> queue = new Queue<Func<IEnumerator>>();

	private Coroutine showAll;

	private Coroutine showOne;

	public bool IsOpen
	{
		get
		{
			return this.showAll != null || this.queue.Count > 0;
		}
	}

	public IEnumerator WaitForFinish()
	{
		while (this.showAll != null || this.queue.Count > 0)
		{
			yield return null;
		}
		yield break;
	}

	public void ShowKillAnimation(GameData.PlayerInfo killer, GameData.PlayerInfo victim)
	{
		IEnumerable<OverlayKillAnimation> killAnims = this.KillAnims;
		if (killer.Object)
		{
			SkinLayer skin = killer.Object.MyPhysics.Skin;
			if (skin.skin && skin.skin.KillAnims.Length != 0)
			{
				killAnims = skin.skin.KillAnims;
			}
		}
		this.queue.Enqueue(delegate
		{
			OverlayKillAnimation overlayKillAnimation = UnityEngine.Object.Instantiate<OverlayKillAnimation>(killAnims.Random<OverlayKillAnimation>(), this.transform);
			overlayKillAnimation.Initialize(killer, victim);
			overlayKillAnimation.gameObject.SetActive(false);
			return this.CoShowOne(overlayKillAnimation);
		});
		if (this.showAll == null)
		{
			this.showAll = base.StartCoroutine(this.ShowAll());
		}
	}

	public void ShowMeeting(MeetingCalledAnimation prefab, GameData.PlayerInfo playerInfo)
	{
		this.queue.Enqueue(delegate
		{
			MeetingCalledAnimation meetingCalledAnimation = UnityEngine.Object.Instantiate<MeetingCalledAnimation>(prefab, this.transform);
			meetingCalledAnimation.Initialize(playerInfo);
			meetingCalledAnimation.gameObject.SetActive(false);
			return this.CoShowOne(meetingCalledAnimation);
		});
		if (this.showAll == null)
		{
			this.showAll = base.StartCoroutine(this.ShowAll());
		}
	}

	private IEnumerator ShowAll()
	{
		while (this.queue.Count > 0 || this.showOne != null)
		{
			if (this.showOne == null)
			{
				this.showOne = base.StartCoroutine(this.queue.Dequeue()());
			}
			yield return null;
		}
		this.showAll = null;
		yield break;
	}

	private IEnumerator CoShowOne(OverlayAnimation anim)
	{
		yield return anim.CoShow(this);
		 UnityEngine.Object.Destroy(anim.gameObject);
		this.showOne = null;
		yield break;
	}
}
