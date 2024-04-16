using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Microsoft.Xbox
{
	public class XboxSdk : MonoBehaviour
	{
		public Text dlcOutputTextBox;

		[Header("You can find the value of the scid in your MicrosoftGame.config")]
		public string scid;

		public Text gamertagLabel;

		public bool signInOnStart = true;

		private static XboxSdk _xboxHelpers;

		private static bool _initialized;

		private const string _GAME_SAVE_CONTAINER_NAME = "x_game_save_default_container";

		private const string _GAME_SAVE_BLOB_NAME = "x_game_save_default_blob";

		public static XboxSdk Helpers
		{
			get
			{
				if (XboxSdk._xboxHelpers == null)
				{
					XboxSdk[] array = Object.FindObjectsOfType<XboxSdk>();
					if (array.Length != 0)
					{
						XboxSdk._xboxHelpers = array[0];
						XboxSdk._xboxHelpers._Initialize();
					}
					else
					{
						XboxSdk._LogError("Error: Could not find Xbox prefab. Make sure you have added the Xbox prefab to your scene.");
					}
				}
				return XboxSdk._xboxHelpers;
			}
		}

		public event XboxSdk.OnGameSaveLoadedHandler OnGameSaveLoaded;

		public event XboxSdk.OnErrorHandler OnError;

		private void Start()
		{
			this._Initialize();
		}

		private void _Initialize()
		{
			if (XboxSdk._initialized)
			{
				return;
			}
			XboxSdk._initialized = true;
			Object.DontDestroyOnLoad(base.gameObject);
		}

		public void SignIn()
		{
		}

		public void Save(byte[] data)
		{
		}

		public void LoadSaveData()
		{
		}

		public void UnlockAchievement(string achievementId)
		{
		}

		private void Update()
		{
		}

		protected static bool Succeeded(int hresult, string operationFriendlyName)
		{
			bool result = false;
			if (HR.SUCCEEDED(hresult))
			{
				result = true;
			}
			else
			{
				string text = hresult.ToString("X8");
				string text2 = operationFriendlyName + " failed.";
				XboxSdk._LogError(string.Format("{0} Error code: hr=0x{1}", text2, text));
				if (XboxSdk.Helpers.OnError != null)
				{
					XboxSdk.Helpers.OnError(XboxSdk.Helpers, new ErrorEventArgs(text, text2));
				}
			}
			return result;
		}

		private static void _LogError(string message)
		{
			Debug.Log(message);
		}

		public delegate void OnGameSaveLoadedHandler(object sender, GameSaveLoadedArgs e);

		public delegate void OnErrorHandler(object sender, ErrorEventArgs e);
	}
}
