using System;

namespace GooglePlayGames.BasicApi.Nearby
{
	public interface IDiscoveryListener
	{
		void OnEndpointFound(EndpointDetails discoveredEndpoint);

		void OnEndpointLost(string lostEndpointId);
	}
}
