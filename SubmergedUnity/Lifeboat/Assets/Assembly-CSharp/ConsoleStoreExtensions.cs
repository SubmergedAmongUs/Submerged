using System;
using UnityEngine;
using UnityEngine.Purchasing;

public class ConsoleStoreExtensions : IExtensionProvider
{
	public T GetExtension<T>() where T : IStoreExtension
	{
		Debug.LogError("Trying to get extension " + typeof(T).ToString() + " from ConsoleStoreExtensions, this does nothing!");
		return default(T);
	}
}
