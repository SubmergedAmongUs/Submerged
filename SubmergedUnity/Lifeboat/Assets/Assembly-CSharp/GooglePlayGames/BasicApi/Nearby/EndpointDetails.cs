using System;
using GooglePlayGames.OurUtils;

namespace GooglePlayGames.BasicApi.Nearby
{
	public struct EndpointDetails
	{
		private readonly string mEndpointId;

		private readonly string mName;

		private readonly string mServiceId;

		public EndpointDetails(string endpointId, string name, string serviceId)
		{
			this.mEndpointId = Misc.CheckNotNull<string>(endpointId);
			this.mName = Misc.CheckNotNull<string>(name);
			this.mServiceId = Misc.CheckNotNull<string>(serviceId);
		}

		public string EndpointId
		{
			get
			{
				return this.mEndpointId;
			}
		}

		public string Name
		{
			get
			{
				return this.mName;
			}
		}

		public string ServiceId
		{
			get
			{
				return this.mServiceId;
			}
		}
	}
}
