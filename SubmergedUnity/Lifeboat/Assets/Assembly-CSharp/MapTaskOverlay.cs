using System;
using System.Collections.Generic;
using UnityEngine;

public class MapTaskOverlay : MonoBehaviour
{
	public ObjectPoolBehavior icons;

	private Dictionary<string, PooledMapIcon> data = new Dictionary<string, PooledMapIcon>();

	public void Show()
	{
		base.gameObject.SetActive(true);
		if (PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
		{
			return;
		}
		for (int i = 0; i < PlayerControl.LocalPlayer.myTasks.Count; i++)
		{
			try
			{
				PlayerTask playerTask = PlayerControl.LocalPlayer.myTasks[i];
				if (playerTask.HasLocation && !playerTask.IsComplete)
				{
					this.SetIconLocation(playerTask);
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}
	}

	public void Update()
	{
		if (PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
		{
			return;
		}
		for (int i = 0; i < PlayerControl.LocalPlayer.myTasks.Count; i++)
		{
			PlayerTask playerTask = PlayerControl.LocalPlayer.myTasks[i];
			if (playerTask.HasLocation && !playerTask.IsComplete && playerTask.LocationDirty)
			{
				this.SetIconLocation(playerTask);
			}
		}
	}

	private void SetIconLocation(PlayerTask task)
	{
		List<Vector2> locations = task.Locations;
		for (int i = 0; i < locations.Count; i++)
		{
			Vector3 localPosition = locations[i] / ShipStatus.Instance.MapScale;
			localPosition.z = -1f;
			PooledMapIcon pooledMapIcon = this.icons.Get<PooledMapIcon>();
			pooledMapIcon.transform.localScale = new Vector3(pooledMapIcon.NormalSize, pooledMapIcon.NormalSize, pooledMapIcon.NormalSize);
			if (PlayerTask.TaskIsEmergency(task))
			{
				pooledMapIcon.rend.color = Color.red;
				pooledMapIcon.alphaPulse.enabled = true;
				pooledMapIcon.rend.material.SetFloat("_Outline", 1f);
			}
			else
			{
				pooledMapIcon.rend.color = Color.yellow;
			}
			pooledMapIcon.name = task.name;
			pooledMapIcon.lastMapTaskStep = task.TaskStep;
			pooledMapIcon.transform.localPosition = localPosition;
			if (task.TaskStep > 0)
			{
				pooledMapIcon.alphaPulse.enabled = true;
				pooledMapIcon.rend.material.SetFloat("_Outline", 1f);
			}
			string text = task.name;
			text += i.ToString();
			this.data.Add(text, pooledMapIcon);
		}
	}

	public void Hide()
	{
		foreach (KeyValuePair<string, PooledMapIcon> keyValuePair in this.data)
		{
			keyValuePair.Value.OwnerPool.Reclaim(keyValuePair.Value);
		}
		this.data.Clear();
		base.gameObject.SetActive(false);
	}
}
