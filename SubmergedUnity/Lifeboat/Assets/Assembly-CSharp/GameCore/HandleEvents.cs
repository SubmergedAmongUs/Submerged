using System;
using UnityEngine;

namespace GameCore
{
	public class HandleEvents : MonoBehaviour
	{
		public SpriteRenderer FillScreen;

		public static HandleEvents Instance { get; private set; }
	}
}
