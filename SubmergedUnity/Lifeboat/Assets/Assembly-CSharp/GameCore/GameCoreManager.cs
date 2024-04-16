using System;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameCore
{
	public class GameCoreManager : MonoBehaviour
	{
		public LocalizationProvider LocalizationProvider;

		public Action OnSuspend;

		private Thread dispatchJob;

		private bool stopExecution;

		public static GameCoreManager Instance { get; private set; }

		public UserManager UserManager { get; private set; }

		public NetworkManager NetworkManager { get; private set; }

		public SaveManager SaveManager { get; private set; }

		private void Awake()
		{
			if (GameCoreManager.Instance != null && GameCoreManager.Instance != this)
			{
				 UnityEngine.Object.Destroy(this);
				return;
			}
			GameCoreManager.Instance = this;
			Object.DontDestroyOnLoad(this);
		}
	}
}
