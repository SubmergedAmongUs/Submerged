using System;
using System.Net;
using Beebyte.Obfuscator;
using Newtonsoft.Json;

[Skip]
public class DnsRegionInfo : IRegionInfo
{
	public readonly string Fqdn;

	public readonly string DefaultIp;

	public readonly ushort Port = 22023;

	private ServerInfo[] cachedServers;

	public string Name { get; }

	[JsonIgnore]
	public string PingServer
	{
		get
		{
			return this.Servers.Random<ServerInfo>().Ip;
		}
	}

	[JsonIgnore]
	public ServerInfo[] Servers
	{
		get
		{
			if (this.cachedServers == null)
			{
				this.PopulateServers();
			}
			return this.cachedServers;
		}
	}

	public StringNames TranslateName { get; }

	public DnsRegionInfo(string fqdn, string name, StringNames translateName, string defaultIp, ushort port)
	{
		if (port == 0)
		{
			port = 22023;
		}
		this.Fqdn = fqdn;
		this.Name = name;
		this.TranslateName = translateName;
		this.DefaultIp = defaultIp;
		this.Port = port;
	}

	private void PopulateServers()
	{
		try
		{
			IPAddress[] hostAddresses = Dns.GetHostAddresses(this.Fqdn);
			ServerInfo[] array = new ServerInfo[hostAddresses.Length];
			for (int i = 0; i < hostAddresses.Length; i++)
			{
				array[i] = new ServerInfo(string.Format("{0}-{1}", this.Name, i), hostAddresses[i].ToString(), this.Port);
			}
			this.cachedServers = array;
		}
		catch
		{
			this.cachedServers = new ServerInfo[]
			{
				new ServerInfo(this.Name ?? "", this.DefaultIp, 22023)
			};
		}
	}

	private DnsRegionInfo(string fqdn, string name, StringNames translateName, ServerInfo[] servers)
	{
		this.Fqdn = fqdn;
		this.Name = name;
		this.TranslateName = translateName;
		this.cachedServers = servers;
	}

	public bool Validate()
	{
		return !string.IsNullOrWhiteSpace(this.Fqdn);
	}

	public IRegionInfo Duplicate()
	{
		return new DnsRegionInfo(this.Fqdn, this.Name, this.TranslateName, this.Servers);
	}

	public override int GetHashCode()
	{
		return this.Name.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		IRegionInfo regionInfo = obj as IRegionInfo;
		return regionInfo != null && regionInfo.Name.Equals(this.Name);
	}
}
