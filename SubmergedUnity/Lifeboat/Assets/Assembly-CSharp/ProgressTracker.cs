using System;
using System.Linq;
using UnityEngine;

public class ProgressTracker : MonoBehaviour
{
	public MeshRenderer TileParent;

	private float curValue;

	public void Start()
	{
		this.TileParent.material.SetFloat("_Buckets", 1f);
		this.TileParent.material.SetFloat("_FullBuckets", 0f);
	}

	public void FixedUpdate()
	{
		if (PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
		{
			this.TileParent.enabled = false;
			return;
		}
		if (!this.TileParent.enabled)
		{
			this.TileParent.enabled = true;
		}
		GameData instance = GameData.Instance;
		if (instance && instance.TotalTasks > 0)
		{
			int num = DestroyableSingleton<TutorialManager>.InstanceExists ? 1 : (instance.AllPlayers.Count - PlayerControl.GameOptions.NumImpostors);
			num -= instance.AllPlayers.Count((GameData.PlayerInfo p) => p.Disconnected);
			switch (PlayerControl.GameOptions.TaskBarMode)
			{
			case TaskBarMode.Normal:
				break;
			case TaskBarMode.MeetingOnly:
				if (!MeetingHud.Instance)
				{
					goto IL_108;
				}
				break;
			case TaskBarMode.Invisible:
				base.gameObject.SetActive(false);
				goto IL_108;
			default:
				goto IL_108;
			}
			float num2 = (float)instance.CompletedTasks / (float)instance.TotalTasks * (float)num;
			this.curValue = Mathf.Lerp(this.curValue, num2, Time.fixedDeltaTime * 2f);
			IL_108:
			this.TileParent.material.SetFloat("_Buckets", (float)num);
			this.TileParent.material.SetFloat("_FullBuckets", this.curValue);
		}
	}
}
