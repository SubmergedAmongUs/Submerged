using System;
using UnityEngine;

namespace GooglePlayGames.OurUtils
{
	public class Logger
	{
		private static bool debugLogEnabled = false;

		private static bool warningLogEnabled = true;

		public static bool DebugLogEnabled
		{
			get
			{
				return Logger.debugLogEnabled;
			}
			set
			{
				Logger.debugLogEnabled = value;
			}
		}

		public static bool WarningLogEnabled
		{
			get
			{
				return Logger.warningLogEnabled;
			}
			set
			{
				Logger.warningLogEnabled = value;
			}
		}

		public static void d(string msg)
		{
			if (Logger.debugLogEnabled)
			{
				PlayGamesHelperObject.RunOnGameThread(delegate
				{
					Debug.Log(Logger.ToLogMessage(string.Empty, "DEBUG", msg));
				});
			}
		}

		public static void w(string msg)
		{
			if (Logger.warningLogEnabled)
			{
				PlayGamesHelperObject.RunOnGameThread(delegate
				{
					Debug.LogWarning(Logger.ToLogMessage("!!!", "WARNING", msg));
				});
			}
		}

		public static void e(string msg)
		{
			if (Logger.warningLogEnabled)
			{
				PlayGamesHelperObject.RunOnGameThread(delegate
				{
					Debug.LogWarning(Logger.ToLogMessage("***", "ERROR", msg));
				});
			}
		}

		public static string describe(byte[] b)
		{
			if (b != null)
			{
				return "byte[" + b.Length.ToString() + "]";
			}
			return "(null)";
		}

		private static string ToLogMessage(string prefix, string logType, string msg)
		{
			string text = null;
			try
			{
				text = DateTime.Now.ToString("MM/dd/yy H:mm:ss zzz");
			}
			catch (Exception)
			{
				PlayGamesHelperObject.RunOnGameThread(delegate
				{
					Debug.LogWarning("*** [Play Games Plugin 0.10.12] ERROR: Failed to format DateTime.Now");
				});
				text = string.Empty;
			}
			return string.Format("{0} [Play Games Plugin 0.10.12] {1} {2}: {3}", new object[]
			{
				prefix,
				text,
				logType,
				msg
			});
		}
	}
}
