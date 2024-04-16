using System;
using InnerNet;

public interface IDisconnectHandler
{
	void HandleDisconnect(PlayerControl pc, DisconnectReasons reason);

	void HandleDisconnect();
}
