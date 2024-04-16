using System;
using System.ComponentModel;
using UnityEngine;

namespace Xbox
{
	[Obsolete("This is from the XDK, please use the GDK versions instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class XboxManager : MonoBehaviour
	{
		[HideInInspector]
		public bool GameSavesAllLoaded;

		[HideInInspector]
		public bool UserAllLoaded;
	}
}
