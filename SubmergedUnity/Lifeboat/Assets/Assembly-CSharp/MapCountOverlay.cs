using System;
using TMPro;
using UnityEngine;

public class MapCountOverlay : MonoBehaviour
{
	public AlphaPulse BackgroundColor;

	public TextMeshPro SabotageText;

	public CounterArea[] CountAreas;

	private Collider2D[] buffer = new Collider2D[20];

	private ContactFilter2D filter;

	private float timer;

	private bool isSab;

	public void Awake()
	{
		this.filter.useLayerMask = true;
		this.filter.layerMask = Constants.PlayersOnlyMask;
		this.filter.useTriggers = true;
	}

	public void OnEnable()
	{
		this.BackgroundColor.SetColor(PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer) ? Palette.DisabledGrey : Color.green);
		this.timer = 1f;
	}

	public void OnDisable()
	{
		for (int i = 0; i < this.CountAreas.Length; i++)
		{
			this.CountAreas[i].UpdateCount(0);
		}
	}

	public void Update()
	{
		this.timer += Time.deltaTime;
		if (this.timer < 0.1f)
		{
			return;
		}
		this.timer = 0f;
		if (!this.isSab && PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
		{
			this.isSab = true;
			this.BackgroundColor.SetColor(Palette.DisabledGrey);
			this.SabotageText.gameObject.SetActive(true);
			return;
		}
		if (this.isSab && !PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
		{
			this.isSab = false;
			this.BackgroundColor.SetColor(Color.green);
			this.SabotageText.gameObject.SetActive(false);
		}
		for (int i = 0; i < this.CountAreas.Length; i++)
		{
			CounterArea counterArea = this.CountAreas[i];
			if (!PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
			{
				PlainShipRoom plainShipRoom;
				if (ShipStatus.Instance.FastRooms.TryGetValue(counterArea.RoomType, out plainShipRoom) && plainShipRoom.roomArea)
				{
					int num = plainShipRoom.roomArea.OverlapCollider(this.filter, this.buffer);
					int num2 = num;
					for (int j = 0; j < num; j++)
					{
						Collider2D collider2D = this.buffer[j];
						if (!(collider2D.tag == "DeadBody"))
						{
							PlayerControl component = collider2D.GetComponent<PlayerControl>();
							if (!component || component.Data == null || component.Data.Disconnected || component.Data.IsDead)
							{
								num2--;
							}
						}
					}
					counterArea.UpdateCount(num2);
				}
				else
				{
					Debug.LogWarning("Couldn't find counter for:" + counterArea.RoomType.ToString());
				}
			}
			else
			{
				counterArea.UpdateCount(0);
			}
		}
	}
}
