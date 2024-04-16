using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class RoomTracker : MonoBehaviour
{
	public TextMeshPro text;

	public float SourceY = -2.5f;

	public float TargetY = -3.25f;

	private Collider2D playerCollider;

	private ContactFilter2D filter;

	private Collider2D[] buffer = new Collider2D[10];

	public PlainShipRoom LastRoom;

	private Coroutine slideInRoutine;

	public void Awake()
	{
		this.filter = default(ContactFilter2D);
		this.filter.layerMask = Constants.PlayersOnlyMask;
		this.filter.useLayerMask = true;
		this.filter.useTriggers = false;
	}

	public void OnDisable()
	{
		this.LastRoom = null;
		Vector3 localPosition = this.text.transform.localPosition;
		localPosition.y = this.TargetY;
		this.text.transform.localPosition = localPosition;
	}

	public void FixedUpdate()
	{
		PlainShipRoom[] array = null;
		if (LobbyBehaviour.Instance)
		{
			PlainShipRoom[] allRooms = LobbyBehaviour.Instance.AllRooms;
			array = allRooms;
		}
		if (ShipStatus.Instance)
		{
			array = ShipStatus.Instance.AllRooms;
		}
		if (array == null)
		{
			return;
		}
		PlainShipRoom plainShipRoom = null;
		if (this.LastRoom)
		{
			int hitCount = this.LastRoom.roomArea.OverlapCollider(this.filter, this.buffer);
			if (RoomTracker.CheckHitsForPlayer(this.buffer, hitCount))
			{
				plainShipRoom = this.LastRoom;
			}
		}
		if (!plainShipRoom)
		{
			foreach (PlainShipRoom plainShipRoom2 in array)
			{
				if (plainShipRoom2.roomArea)
				{
					int hitCount2 = plainShipRoom2.roomArea.OverlapCollider(this.filter, this.buffer);
					if (RoomTracker.CheckHitsForPlayer(this.buffer, hitCount2))
					{
						plainShipRoom = plainShipRoom2;
					}
				}
			}
		}
		if (plainShipRoom)
		{
			if (this.LastRoom != plainShipRoom)
			{
				this.LastRoom = plainShipRoom;
				if (this.slideInRoutine != null)
				{
					base.StopCoroutine(this.slideInRoutine);
				}
				if (plainShipRoom.RoomId != SystemTypes.Hallway)
				{
					this.slideInRoutine = base.StartCoroutine(this.CoSlideIn(plainShipRoom.RoomId));
					return;
				}
				this.slideInRoutine = base.StartCoroutine(this.SlideOut());
				return;
			}
		}
		else if (this.LastRoom)
		{
			this.LastRoom = null;
			if (this.slideInRoutine != null)
			{
				base.StopCoroutine(this.slideInRoutine);
			}
			this.slideInRoutine = base.StartCoroutine(this.SlideOut());
		}
	}

	private IEnumerator CoSlideIn(SystemTypes newRoom)
	{
		yield return this.SlideOut();
		Vector3 tempPos = this.text.transform.localPosition;
		Color tempColor = Color.white;
		this.text.text = DestroyableSingleton<TranslationController>.Instance.GetString(newRoom);
		float timer = 0f;
		while (timer < 0.25f)
		{
			timer = Mathf.Min(0.25f, timer + Time.deltaTime);
			float num = timer / 0.25f;
			tempPos.y = Mathf.SmoothStep(this.TargetY, this.SourceY, num);
			tempColor.a = Mathf.Lerp(0f, 1f, num);
			this.text.transform.localPosition = tempPos;
			this.text.color = tempColor;
			yield return null;
		}
		yield break;
	}

	private IEnumerator SlideOut()
	{
		Vector3 tempPos = this.text.transform.localPosition;
		Color tempColor = Color.white;
		float timer = FloatRange.ReverseLerp(tempPos.y, this.SourceY, this.TargetY) * 0.1f;
		while (timer < 0.1f)
		{
			timer = Mathf.Min(0.1f, timer + Time.deltaTime);
			float num = timer / 0.1f;
			tempPos.y = Mathf.SmoothStep(this.SourceY, this.TargetY, num);
			tempColor.a = Mathf.Lerp(1f, 0f, num);
			this.text.transform.localPosition = tempPos;
			this.text.color = tempColor;
			yield return null;
		}
		yield break;
	}

	private static bool CheckHitsForPlayer(Collider2D[] buffer, int hitCount)
	{
		if (!PlayerControl.LocalPlayer)
		{
			return false;
		}
		for (int i = 0; i < hitCount; i++)
		{
			if (buffer[i].gameObject == PlayerControl.LocalPlayer.gameObject)
			{
				return true;
			}
		}
		return false;
	}
}
