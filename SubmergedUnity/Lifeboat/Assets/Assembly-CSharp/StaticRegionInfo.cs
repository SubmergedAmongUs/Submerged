using System;
using System.IO;
using System.Linq;
using Beebyte.Obfuscator;

[Skip]
public class StaticRegionInfo : IRegionInfo
{
	public string Name { get; }

	public string PingServer { get; }

	public ServerInfo[] Servers { get; }

	public StringNames TranslateName { get; }

	public StaticRegionInfo(string name, StringNames translateName, string pingServer, ServerInfo[] servers)
	{
		this.Name = name;
		this.PingServer = pingServer;
		this.Servers = servers;
		this.TranslateName = translateName;
	}

	public static StaticRegionInfo Deserialize(BinaryReader reader)
	{
		string name = reader.ReadString();
		string pingServer = reader.ReadString();
		ServerInfo[] array = new ServerInfo[reader.ReadInt32()];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = ServerInfo.Deserialize(reader);
		}
		return new StaticRegionInfo(name, StringNames.NoTranslation, pingServer, array);
	}

	public IRegionInfo Duplicate()
	{
		ServerInfo[] array = new ServerInfo[this.Servers.Length];
		for (int i = 0; i < array.Length; i++)
		{
			ServerInfo serverInfo = this.Servers[i];
			array[i] = new ServerInfo(serverInfo.Name, serverInfo.Ip, serverInfo.Port)
			{
				ConnectionFailures = serverInfo.ConnectionFailures,
				Players = serverInfo.Players
			};
		}
		return new StaticRegionInfo(this.Name, this.TranslateName, this.PingServer, array);
	}

	public bool Validate()
	{
		if (!string.IsNullOrWhiteSpace(this.PingServer))
		{
			return this.Servers.All((ServerInfo s) => !string.IsNullOrWhiteSpace(s.Ip));
		}
		return false;
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
