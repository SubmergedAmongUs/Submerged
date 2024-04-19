using System;

namespace InnerNet
{
	[Flags]
	public enum SpawnFlags : byte
	{
		None = 0,
		IsClientCharacter = 1
	}
}
