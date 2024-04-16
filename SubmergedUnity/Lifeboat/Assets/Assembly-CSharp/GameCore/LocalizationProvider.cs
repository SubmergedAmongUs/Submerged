using System;
using UnityEngine;

namespace GameCore
{
	public abstract class LocalizationProvider : MonoBehaviour
	{
		public abstract string GetLocalizedText(LocalizationKeys key);
	}
}
