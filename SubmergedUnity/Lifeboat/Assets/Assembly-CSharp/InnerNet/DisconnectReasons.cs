using System;

namespace InnerNet
{
	public enum DisconnectReasons
	{
		ExitGame,
		GameFull,
		GameStarted,
		GameNotFound,
		IncorrectVersion = 5,
		Banned,
		Kicked,
		Custom,
		InvalidName,
		Hacking,
		NotAuthorized,
		Destroy = 16,
		Error,
		IncorrectGame,
		ServerRequest,
		ServerFull,
		IntentionalLeaving = 208,
		FocusLostBackground = 207,
		FocusLost = 209,
		NewConnection
	}
}
