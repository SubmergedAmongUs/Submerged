using System;

namespace Microsoft.Xbox
{
	internal class HR
	{
		internal static bool SUCCEEDED(int hr)
		{
			return hr >= 0;
		}

		internal static bool FAILED(int hr)
		{
			return hr < 0;
		}
	}
}
