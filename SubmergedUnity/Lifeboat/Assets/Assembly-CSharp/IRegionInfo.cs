using System;
using Beebyte.Obfuscator;

[Skip]
public interface IRegionInfo
{
	string Name { get; }

	string PingServer { get; }

	ServerInfo[] Servers { get; }

	StringNames TranslateName { get; }

	IRegionInfo Duplicate();

	bool Validate();
}
