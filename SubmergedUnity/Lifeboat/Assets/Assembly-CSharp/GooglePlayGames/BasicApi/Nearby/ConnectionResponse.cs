using System;
using GooglePlayGames.OurUtils;

namespace GooglePlayGames.BasicApi.Nearby
{
	public struct ConnectionResponse
	{
		private static readonly byte[] EmptyPayload = new byte[0];

		private readonly long mLocalClientId;

		private readonly string mRemoteEndpointId;

		private readonly ConnectionResponse.Status mResponseStatus;

		private readonly byte[] mPayload;

		private ConnectionResponse(long localClientId, string remoteEndpointId, ConnectionResponse.Status code, byte[] payload)
		{
			this.mLocalClientId = localClientId;
			this.mRemoteEndpointId = Misc.CheckNotNull<string>(remoteEndpointId);
			this.mResponseStatus = code;
			this.mPayload = Misc.CheckNotNull<byte[]>(payload);
		}

		public long LocalClientId
		{
			get
			{
				return this.mLocalClientId;
			}
		}

		public string RemoteEndpointId
		{
			get
			{
				return this.mRemoteEndpointId;
			}
		}

		public ConnectionResponse.Status ResponseStatus
		{
			get
			{
				return this.mResponseStatus;
			}
		}

		public byte[] Payload
		{
			get
			{
				return this.mPayload;
			}
		}

		public static ConnectionResponse Rejected(long localClientId, string remoteEndpointId)
		{
			return new ConnectionResponse(localClientId, remoteEndpointId, ConnectionResponse.Status.Rejected, ConnectionResponse.EmptyPayload);
		}

		public static ConnectionResponse NetworkNotConnected(long localClientId, string remoteEndpointId)
		{
			return new ConnectionResponse(localClientId, remoteEndpointId, ConnectionResponse.Status.ErrorNetworkNotConnected, ConnectionResponse.EmptyPayload);
		}

		public static ConnectionResponse InternalError(long localClientId, string remoteEndpointId)
		{
			return new ConnectionResponse(localClientId, remoteEndpointId, ConnectionResponse.Status.ErrorInternal, ConnectionResponse.EmptyPayload);
		}

		public static ConnectionResponse EndpointNotConnected(long localClientId, string remoteEndpointId)
		{
			return new ConnectionResponse(localClientId, remoteEndpointId, ConnectionResponse.Status.ErrorEndpointNotConnected, ConnectionResponse.EmptyPayload);
		}

		public static ConnectionResponse Accepted(long localClientId, string remoteEndpointId, byte[] payload)
		{
			return new ConnectionResponse(localClientId, remoteEndpointId, ConnectionResponse.Status.Accepted, payload);
		}

		public static ConnectionResponse AlreadyConnected(long localClientId, string remoteEndpointId)
		{
			return new ConnectionResponse(localClientId, remoteEndpointId, ConnectionResponse.Status.ErrorAlreadyConnected, ConnectionResponse.EmptyPayload);
		}

		public enum Status
		{
			Accepted,
			Rejected,
			ErrorInternal,
			ErrorNetworkNotConnected,
			ErrorEndpointNotConnected,
			ErrorAlreadyConnected
		}
	}
}
