using System;

namespace InnerNet
{
	public enum JoinFailureReasons : byte
	{
		TooManyPlayers = 1,
		GameStarted,
		GameNotFound,
		HostNotReady,
		IncorrectVersion,
		Banned,
		Kicked
	}
}
