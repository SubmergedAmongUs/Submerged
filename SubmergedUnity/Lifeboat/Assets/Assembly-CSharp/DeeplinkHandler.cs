using System;
using System.Collections.Generic;
using System.Linq;
using ImaginationOverflow.UniversalDeepLinking;
using UnityEngine;

public class DeeplinkHandler : MonoBehaviour
{
	private void Start()
	{
		DeepLinkManager.Instance.LinkActivated += new LinkActivationHandler(this.Instance_LinkActivated);
	}

	private void Instance_LinkActivated(LinkActivation s)
	{
		Debug.Log("Got init deeplink: " + s.Uri);
		if (!s.Uri.StartsWith("amongus://init/", StringComparison.OrdinalIgnoreCase) && !s.Uri.StartsWith("amongus://init?", StringComparison.OrdinalIgnoreCase))
		{
			return;
		}
		Debug.Log("Got init deeplink p2: " + string.Join("    ", from k in s.QueryString
		select k.Key + "=" + k.Value));
		string s2;
		ushort port;
		string text;
		string name;
		if (s.QueryString.TryGetValue("serverport", out s2) && ushort.TryParse(s2, out port) && s.QueryString.TryGetValue("serverip", out text) && s.QueryString.TryGetValue("servername", out name))
		{
			ServerInfo serverInfo = new ServerInfo(base.name, text, port);
			StaticRegionInfo newRegion = new StaticRegionInfo(name, StringNames.NoTranslation, text, new ServerInfo[]
			{
				serverInfo
			});
			DestroyableSingleton<ServerManager>.Instance.AddOrUpdateRegion(newRegion);
		}
	}
}
