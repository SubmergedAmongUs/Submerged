using System;

namespace GooglePlayGames.BasicApi.Nearby
{
	public interface IMessageListener
	{
		void OnMessageReceived(string remoteEndpointId, byte[] data, bool isReliableMessage);

		void OnRemoteEndpointDisconnected(string remoteEndpointId);
	}
}
