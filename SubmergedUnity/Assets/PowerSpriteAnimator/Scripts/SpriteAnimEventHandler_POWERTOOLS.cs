//-----------------------------------------
//          PowerSprite Animator
//  Copyright © 2017 Powerhoof Pty Ltd
//			  powerhoof.com
//----------------------------------------

using UnityEngine;
using System.Collections;

namespace PowerTools
{

/// Component allowing Sprite Animator events to be passed upwards thorugh an object (and possibly more event functionality in future)
[DisallowMultipleComponent]
public abstract class SpriteAnimEventHandler_POWERTOOLS : MonoBehaviour
{
	/// Utility class contains functions to split an event's string parameter to a message name and value. Used for PowerSprite animation events
	public static class EventParser
	{
		public static readonly char MESSAGE_DELIMITER = '\t';
		public static readonly string MESSAGE_NOPARAM = "_Anim";
		public static readonly string MESSAGE_INT = "_AnimInt";
		public static readonly string MESSAGE_FLOAT = "_AnimFloat";
		public static readonly string MESSAGE_STRING = "_AnimString";
		public static readonly string MESSAGE_OBJECT_FUNCNAME = "_AnimObjectFunc";
		public static readonly string MESSAGE_OBJECT_DATA = "_AnimObjectData";

		/// Parses value from the passed messageString, and modifies it to just contain the message function name
		public static int ParseInt( ref string messageString )
		{
			// Data is in form "<functionname>\t<int>"
			int splitAt = messageString.IndexOf(MESSAGE_DELIMITER);
			int result = 0;
			int.TryParse(messageString.Substring(splitAt+1), out result);
			messageString = messageString.Substring(0,splitAt);
			return result;
		}

		/// Parses value from the passed messageString, and modifies it to just contain the message function name
		public static float ParseFloat( ref string messageString )
		{
			// Data is in form "<functionname>\t<float>"
			int splitAt = messageString.IndexOf(MESSAGE_DELIMITER);
			float result = 0;
			float.TryParse(messageString.Substring(splitAt+1), out result);
			messageString = messageString.Substring(0,splitAt);
			return result;
		}

		/// Parses value from the passed messageString, and modifies it to just contain the message function name
		public static string ParseString( ref string messageString )
		{
			// Data is in form "<functionname>\t<string>"
			int splitAt = messageString.IndexOf(MESSAGE_DELIMITER);
			string result = messageString.Substring(splitAt+1);
			messageString = messageString.Substring(0,splitAt);
			return result;
		}
	}

	string m_eventWithObjectMessage = null;
	object m_eventWithObjectData = null;

	#region Funcs: Anim Events

	/*
		These messages are used so you can have a sprite nested under the object with logic and still have animation events sent to the parent object.
		- The function name and data is encoded in a delimited string
		- It's rather inefficient, but you're generally not going to have loads of anim events each frame, so shouldn't matter
		- If "require reciever" is desired, a duplicate set of functions should be added with different function names
		- They're prefixed with _ to make it obvious they're not normal messages and make it quick for editor to check the prefix
	*/

	// Anim event with No param
	void _Anim(string function)
	{
		SendMessageUpwards(function, SendMessageOptions.DontRequireReceiver);
	}

	// Anim event with Int param
	void _AnimInt(string messageString)
	{
		int param = EventParser.ParseInt(ref messageString);
		SendMessageUpwards( messageString, param, SendMessageOptions.DontRequireReceiver );
	}

	// Anim event with Float param
	void _AnimFloat(string messageString)
	{
		float param = EventParser.ParseFloat(ref messageString);
		SendMessageUpwards( messageString, param, SendMessageOptions.DontRequireReceiver );
	}

	// Anim event with String param
	void _AnimString(string messageString)
	{
		string param = EventParser.ParseString( ref messageString );
		SendMessageUpwards( messageString, param, SendMessageOptions.DontRequireReceiver );
	}

	// Anim event with Object params are split into 2 functions in order to get the function name as well as object. Big hack, bleah.
	void _AnimObjectFunc(string funcName)
	{
		if ( m_eventWithObjectData != null )
		{
			SendMessageUpwards( funcName, m_eventWithObjectData, SendMessageOptions.DontRequireReceiver );
			m_eventWithObjectMessage = null;
			m_eventWithObjectData = null;
		}
		else
		{
			if ( string.IsNullOrEmpty(m_eventWithObjectMessage) == false )
				Debug.LogWarning("Animation event with object parameter had no object");
			m_eventWithObjectMessage = funcName;
		}
	}
	void _AnimObjectData(Object data)
	{
		if ( string.IsNullOrEmpty( m_eventWithObjectMessage ) == false )
		{
			SendMessageUpwards( m_eventWithObjectMessage, data, SendMessageOptions.DontRequireReceiver );
			m_eventWithObjectMessage = null;
			m_eventWithObjectData = null;
		}
		else
		{
			if ( m_eventWithObjectData != null )
				Debug.LogWarning("Animation event with object parameter had no object");
			m_eventWithObjectData = data;
		}
	}

	#endregion
}

}
