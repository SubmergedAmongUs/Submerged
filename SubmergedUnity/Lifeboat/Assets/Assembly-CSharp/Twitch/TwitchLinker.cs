using System;
using System.Collections;
using UnityEngine;

namespace Twitch
{
	public class TwitchLinker : MonoBehaviour
	{
		public void LaunchImplicitAuth()
		{
			DestroyableSingleton<TwitchManager>.Instance.LaunchImplicitAuth(base.transform);
		}

		private IEnumerator ShakeGlitch()
		{
			while (DestroyableSingleton<TwitchManager>.Instance.running)
			{
				yield return Effects.Bounce(base.transform, 1f, 0.2f);
				yield return Effects.Wait(0.3f);
			}
			yield break;
		}
	}
}
