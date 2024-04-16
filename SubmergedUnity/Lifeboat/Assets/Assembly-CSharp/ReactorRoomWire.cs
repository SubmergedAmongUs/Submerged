using System;
using PowerTools;
using UnityEngine;

public class ReactorRoomWire : MonoBehaviour
{
	public global::Console myConsole;

	public SpriteAnim Image;

	public AnimationClip Normal;

	public AnimationClip MeltdownNeed;

	public AnimationClip MeltdownReady;

	private ReactorSystemType reactor;

	public void Update()
	{
		if (this.reactor == null)
		{
			ISystemType systemType;
			if (!ShipStatus.Instance || !ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Reactor, out systemType))
			{
				return;
			}
			this.reactor = (ReactorSystemType)systemType;
		}
		if (this.reactor.IsActive)
		{
			if (this.reactor.GetConsoleComplete(this.myConsole.ConsoleId))
			{
				if (!this.Image.IsPlaying(this.MeltdownReady))
				{
					this.Image.Play(this.MeltdownReady, 1f);
					return;
				}
			}
			else if (!this.Image.IsPlaying(this.MeltdownNeed))
			{
				this.Image.Play(this.MeltdownNeed, 1f);
				return;
			}
		}
		else if (!this.Image.IsPlaying(this.Normal))
		{
			this.Image.Play(this.Normal, 1f);
		}
	}
}
