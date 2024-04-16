using System;
using System.IO;
using System.Net;
using Beebyte.Obfuscator;

[Skip]
public class ServerInfo
{
	public readonly string Name = "Custom";

	public readonly string Ip;

	public readonly ushort Port;

	public int Players;

	public int ConnectionFailures;

	public ServerInfo(string name, string ip, ushort port)
	{
		this.Name = name;
		this.Ip = ip;
		this.Port = port;
	}

	public void Serialize(BinaryWriter writer)
	{
		writer.Write(this.Name);
		writer.Write((uint)IPAddress.Parse(this.Ip).Address);
		writer.Write(this.Port);
		writer.Write(this.ConnectionFailures);
	}

	public static ServerInfo Deserialize(BinaryReader reader)
	{
		return new ServerInfo(reader.ReadString(), new IPAddress((long)((ulong)reader.ReadUInt32())).ToString(), reader.ReadUInt16())
		{
			ConnectionFailures = reader.ReadInt32()
		};
	}

	public override string ToString()
	{
		return string.Format("{0}: {1}:{2}", this.Name, this.Ip, this.Port);
	}

	public override int GetHashCode()
	{
		return this.Ip.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		ServerInfo serverInfo = obj as ServerInfo;
		return serverInfo != null && serverInfo.Ip == this.Ip && serverInfo.Port == this.Port;
	}
}
