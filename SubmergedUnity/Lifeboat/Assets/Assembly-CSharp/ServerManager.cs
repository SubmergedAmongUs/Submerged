using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Beebyte.Obfuscator;
using Hazel;
using Hazel.UPnP;
using Newtonsoft.Json;
using UnityEngine;

public class ServerManager : DestroyableSingleton<ServerManager>
{
	public static readonly IRegionInfo[] DefaultRegions = new IRegionInfo[]
	{
		new DnsRegionInfo("na.mm.among.us", "North America", StringNames.ServerNA, "50.116.1.42", 22023),
		new DnsRegionInfo("eu.mm.among.us", "Europe", StringNames.ServerEU, "172.105.251.170", 22023),
		new DnsRegionInfo("as.mm.among.us", "Asia", StringNames.ServerAS, "139.162.111.196", 22023)
	};

	private string serverInfoFileOld;

	private string serverInfoFileJson;

	private ServerManager.UpdateState state;

	internal void AddOrUpdateRegion(IRegionInfo newRegion)
	{
		if (ServerManager.DefaultRegions.All((IRegionInfo r) => !r.Name.Equals(newRegion.Name, StringComparison.OrdinalIgnoreCase)))
		{
			IEnumerable<IRegionInfo> source = from r in this.AvailableRegions
			where !r.Name.Equals(newRegion.Name, StringComparison.OrdinalIgnoreCase)
			select r;
			if (source.Count<IRegionInfo>() == this.AvailableRegions.Length)
			{
				Debug.Log("Added new region: " + newRegion.Name);
				this.AvailableRegions = this.AvailableRegions.Append(newRegion).ToArray<IRegionInfo>();
			}
			else
			{
				Debug.Log("Updated existing region: " + newRegion.Name);
				this.AvailableRegions = source.Append(newRegion).ToArray<IRegionInfo>();
			}
			this.CurrentRegion = newRegion;
			this.CurrentServer = newRegion.Servers.Random<ServerInfo>();
			this.SaveServers();
		}
	}

	public IRegionInfo CurrentRegion { get; private set; }

	public ServerInfo CurrentServer { get; private set; }

	public IRegionInfo[] AvailableRegions { get; private set; } = ServerManager.DefaultRegions;

	private ServerInfo[] AvailableServers
	{
		get
		{
			return this.CurrentRegion.Servers;
		}
	}

	public string OnlineNetAddress
	{
		get
		{
			return this.CurrentServer.Ip;
		}
	}

	public ushort OnlineNetPort
	{
		get
		{
			return this.CurrentServer.Port;
		}
	}

	public override void Awake()
	{
		base.Awake();
		if (DestroyableSingleton<ServerManager>.Instance != this)
		{
			return;
		}
		this.serverInfoFileOld = Path.Combine(PlatformPaths.persistentDataPath, "regionInfo.dat");
		this.serverInfoFileJson = Path.Combine(PlatformPaths.persistentDataPath, "regionInfo.json");
		this.LoadServers();
		Task.Run(new Action(this.HandleUpnp));
	}

	private void HandleUpnp()
	{
		try
		{
			using (UPnPHelper upnPHelper = new UPnPHelper(NullLogger.Instance))
			{
				try
				{
					upnPHelper.DeleteForwardingRule(22024);
				}
				catch
				{
				}
				try
				{
					upnPHelper.DeleteForwardingRule(22023);
				}
				catch
				{
				}
			}
		}
		catch
		{
		}
	}

	[ContextMenu("Reselect Server")]
	internal void ReselectServer()
	{
		Debug.Log("ServerManager::ReselectServer");
		this.CurrentRegion = (this.CurrentRegion ?? ServerManager.DefaultRegions[0].Duplicate());
		if (this.AvailableServers.All((ServerInfo s) => s.Players == 0))
		{
			this.AvailableServers.Shuffle(0);
		}
		this.CurrentServer = (from s in this.AvailableServers
		orderby s.ConnectionFailures, s.Players
		select s).First<ServerInfo>();
		Debug.Log(string.Format("Selected server: {0}", this.CurrentServer));
		this.state = ServerManager.UpdateState.Success;
	}

	public IEnumerator ReselectRegionFromDefaults()
	{
		Debug.Log("ServerManager::ReselectRegionFromDefaults");
		this.AvailableRegions = ServerManager.DefaultRegions;
		ServerManager.PingWrapper[] pings = new ServerManager.PingWrapper[ServerManager.DefaultRegions.Length];
		for (int i = 0; i < pings.Length; i++)
		{
			IRegionInfo regionInfo = ServerManager.DefaultRegions[i];
			pings[i] = new ServerManager.PingWrapper(regionInfo, new Ping(regionInfo.PingServer));
		}
		for (;;)
		{
			if (pings.Any((ServerManager.PingWrapper p) => p.Ping.isDone && p.Ping.time > 0))
			{
				break;
			}
			yield return null;
		}
		IRegionInfo regionInfo2 = ServerManager.DefaultRegions.First<IRegionInfo>();
		int num = int.MaxValue;
		foreach (ServerManager.PingWrapper pingWrapper in pings)
		{
			if (pingWrapper.Ping.isDone && pingWrapper.Ping.time > 0)
			{
				Debug.Log("Ping time: " + pingWrapper.Region.Name + " @ " + pingWrapper.Ping.time.ToString());
				if (pingWrapper.Ping.time < num)
				{
					regionInfo2 = pingWrapper.Region;
					num = pingWrapper.Ping.time;
				}
			}
			pingWrapper.Ping.DestroyPing();
		}
		this.CurrentRegion = regionInfo2.Duplicate();
		this.ReselectServer();
		this.SaveServers();
		yield break;
	}

	public IEnumerator WaitForServers()
	{
		while (this.state == ServerManager.UpdateState.Connecting)
		{
			yield return null;
		}
		yield break;
	}

	internal void SetRegion(IRegionInfo region)
	{
		this.CurrentRegion = region;
		this.ReselectServer();
		this.SaveServers();
	}

	public void SaveServers()
	{
		try
		{
			ServerManager.JsonServerData jsonServerData = default(ServerManager.JsonServerData);
			jsonServerData.CurrentRegionIdx = this.AvailableRegions.IndexOf((IRegionInfo r) => r.Name == this.CurrentRegion.Name);
			jsonServerData.Regions = this.AvailableRegions;
			FileIO.WriteAllText(this.serverInfoFileJson, JsonConvert.SerializeObject(jsonServerData, new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.Auto
			}));
		}
		catch
		{
		}
	}

	public void LoadServers()
	{
		Debug.Log("ServerManager::LoadServers");
		if (FileIO.Exists(this.serverInfoFileOld))
		{
			this.LoadServersOld();
			FileIO.Delete(this.serverInfoFileOld);
			this.SaveServers();
			return;
		}
		if (FileIO.Exists(this.serverInfoFileJson))
		{
			try
			{
				ServerManager.JsonServerData jsonServerData = JsonConvert.DeserializeObject<ServerManager.JsonServerData>(FileIO.ReadAllText(this.serverInfoFileJson), new JsonSerializerSettings
				{
					TypeNameHandling = TypeNameHandling.Auto
				});
				jsonServerData.CleanAndMerge(ServerManager.DefaultRegions);
				this.AvailableRegions = jsonServerData.Regions;
				this.CurrentRegion = this.AvailableRegions[jsonServerData.CurrentRegionIdx.Wrap(this.AvailableRegions.Length)];
				this.CurrentServer = this.CurrentRegion.Servers.Random<ServerInfo>();
				this.state = ServerManager.UpdateState.Success;
				return;
			}
			catch (Exception ex)
			{
				Debug.Log(string.Format("Couldn't load regions: {0}", ex));
				Debug.LogException(ex, this);
				base.StartCoroutine(this.ReselectRegionFromDefaults());
				return;
			}
		}
		base.StartCoroutine(this.ReselectRegionFromDefaults());
	}

	public void LoadServersOld()
	{
		if (File.Exists(this.serverInfoFileOld))
		{
			try
			{
				using (FileStream fileStream = File.OpenRead(this.serverInfoFileOld))
				{
					using (BinaryReader binaryReader = new BinaryReader(fileStream))
					{
						int num = binaryReader.ReadInt32();
						this.CurrentRegion = StaticRegionInfo.Deserialize(binaryReader);
						this.CurrentServer = this.CurrentRegion.Servers[num];
						Debug.Log(string.Format("Loaded server: {0}", this.CurrentServer));
						this.AvailableRegions = ServerManager.DefaultRegions.Append(this.CurrentRegion).ToArray<IRegionInfo>();
						this.state = ServerManager.UpdateState.Success;
					}
				}
				return;
			}
			catch (Exception ex)
			{
				Debug.Log(string.Format("Couldn't load regions: {0}", ex));
				Debug.LogException(ex, this);
				base.StartCoroutine(this.ReselectRegionFromDefaults());
				return;
			}
		}
		base.StartCoroutine(this.ReselectRegionFromDefaults());
	}

	internal bool TrackServerFailure(string networkAddress)
	{
		ServerInfo srv = this.AvailableServers.FirstOrDefault((ServerInfo s) => s.Ip == networkAddress);
		if (srv != null)
		{
			srv.ConnectionFailures++;
			ServerInfo serverInfo = (from s in this.AvailableServers
			orderby s.Players
			select s).FirstOrDefault((ServerInfo s) => s.ConnectionFailures < srv.ConnectionFailures);
			if (serverInfo != null)
			{
				this.CurrentServer = serverInfo;
				AmongUsClient.Instance.SetEndpoint(serverInfo.Ip, serverInfo.Port);
				Debug.Log("Attempting another server: " + serverInfo.Name);
				return true;
			}
		}
		return false;
	}

	private enum UpdateState
	{
		Connecting,
		Failed,
		Success
	}

	[Skip]
	[JsonObject]
	private struct JsonServerData
	{
		public int CurrentRegionIdx;

		public IRegionInfo[] Regions;

		internal void CleanAndMerge(IRegionInfo[] defaultRegions)
		{
			List<IRegionInfo> list = (from r in this.Regions
			where r.Validate()
			select r).ToList<IRegionInfo>();
			for (int i = defaultRegions.Length - 1; i >= 0; i--)
			{
				IRegionInfo region = defaultRegions[i];
				if (!this.Regions.Any((IRegionInfo r) => r.Name.Equals(region.Name, StringComparison.OrdinalIgnoreCase)))
				{
					list.Insert(0, region);
				}
			}
			this.Regions = list.ToArray();
		}
	}

	private struct PingWrapper
	{
		public IRegionInfo Region;

		public Ping Ping;

		public PingWrapper(IRegionInfo region, Ping ping)
		{
			this.Region = region;
			this.Ping = ping;
		}
	}
}
