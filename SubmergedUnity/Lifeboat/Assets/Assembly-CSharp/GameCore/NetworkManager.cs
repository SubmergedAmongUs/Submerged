using System;

namespace GameCore
{
	public class NetworkManager
	{
		public NetworkManager.OnInvitedCallback OnInvited;

		public bool IsInviteHandled { get; set; } = true;

		public string ConnectionString { get; private set; }

		public delegate void OnInvitedCallback(string connectionString);
	}
}
