using System;

namespace GooglePlayGames.BasicApi
{
	public class CommonTypesUtil
	{
		public static bool StatusIsSuccess(ResponseStatus status)
		{
			return status > (ResponseStatus)0;
		}
	}
}
