using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PowerTools
{
	[DisallowMultipleComponent]
	public class SpriteAnimEventHandler : MonoBehaviour
	{
		private string m_eventWithObjectMessage;

		private object m_eventWithObjectData;

		private void _Anim(string function)
		{
			base.SendMessageUpwards(function, 1);
		}

		private void _AnimInt(string messageString)
		{
			int num = SpriteAnimEventHandler.EventParser.ParseInt(ref messageString);
			base.SendMessageUpwards(messageString, num, SendMessageOptions.DontRequireReceiver);
		}

		private void _AnimFloat(string messageString)
		{
			float num = SpriteAnimEventHandler.EventParser.ParseFloat(ref messageString);
			base.SendMessageUpwards(messageString, num, SendMessageOptions.DontRequireReceiver);
		}

		private void _AnimString(string messageString)
		{
			string text = SpriteAnimEventHandler.EventParser.ParseString(ref messageString);
			base.SendMessageUpwards(messageString, text, SendMessageOptions.DontRequireReceiver);
		}

		private void _AnimObjectFunc(string funcName)
		{
			if (this.m_eventWithObjectData != null)
			{
				base.SendMessageUpwards(funcName, this.m_eventWithObjectData, SendMessageOptions.DontRequireReceiver);
				this.m_eventWithObjectMessage = null;
				this.m_eventWithObjectData = null;
				return;
			}
			if (!string.IsNullOrEmpty(this.m_eventWithObjectMessage))
			{
				Debug.LogError("Animation event with object parameter had no object");
			}
			this.m_eventWithObjectMessage = funcName;
		}

		private void _AnimObjectData(Object data)
		{
			if (!string.IsNullOrEmpty(this.m_eventWithObjectMessage))
			{
				base.SendMessageUpwards(this.m_eventWithObjectMessage, data, SendMessageOptions.DontRequireReceiver);
				this.m_eventWithObjectMessage = null;
				this.m_eventWithObjectData = null;
				return;
			}
			if (this.m_eventWithObjectData != null)
			{
				Debug.LogError("Animation event with object parameter had no object");
			}
			this.m_eventWithObjectData = data;
		}

		public static class EventParser
		{
			public static readonly char MESSAGE_DELIMITER = '\t';

			public static readonly string MESSAGE_NOPARAM = "_Anim";

			public static readonly string MESSAGE_INT = "_AnimInt";

			public static readonly string MESSAGE_FLOAT = "_AnimFloat";

			public static readonly string MESSAGE_STRING = "_AnimString";

			public static readonly string MESSAGE_OBJECT_FUNCNAME = "_AnimObjectFunc";

			public static readonly string MESSAGE_OBJECT_DATA = "_AnimObjectData";

			public static int ParseInt(ref string messageString)
			{
				int num = messageString.IndexOf(SpriteAnimEventHandler.EventParser.MESSAGE_DELIMITER);
				int result = 0;
				int.TryParse(messageString.Substring(num + 1), out result);
				messageString = messageString.Substring(0, num);
				return result;
			}

			public static float ParseFloat(ref string messageString)
			{
				int num = messageString.IndexOf(SpriteAnimEventHandler.EventParser.MESSAGE_DELIMITER);
				float result = 0f;
				float.TryParse(messageString.Substring(num + 1), out result);
				messageString = messageString.Substring(0, num);
				return result;
			}

			public static string ParseString(ref string messageString)
			{
				int num = messageString.IndexOf(SpriteAnimEventHandler.EventParser.MESSAGE_DELIMITER);
				string result = messageString.Substring(num + 1);
				messageString = messageString.Substring(0, num);
				return result;
			}
		}
	}
}
