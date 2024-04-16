using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GooglePlayGames.OurUtils
{
	public class PlayGamesHelperObject : MonoBehaviour
	{
		private static PlayGamesHelperObject instance = null;

		private static bool sIsDummy = false;

		private static List<Action> sQueue = new List<Action>();

		private List<Action> localQueue = new List<Action>();

		private static volatile bool sQueueEmpty = true;

		private static List<Action<bool>> sPauseCallbackList = new List<Action<bool>>();

		private static List<Action<bool>> sFocusCallbackList = new List<Action<bool>>();

		public static void CreateObject()
		{
			if (PlayGamesHelperObject.instance != null)
			{
				return;
			}
			if (Application.isPlaying)
			{
				GameObject gameObject = new GameObject("PlayGames_QueueRunner");
				Object.DontDestroyOnLoad(gameObject);
				PlayGamesHelperObject.instance = gameObject.AddComponent<PlayGamesHelperObject>();
				return;
			}
			PlayGamesHelperObject.instance = new PlayGamesHelperObject();
			PlayGamesHelperObject.sIsDummy = true;
		}

		public void Awake()
		{
			Object.DontDestroyOnLoad(base.gameObject);
		}

		public void OnDisable()
		{
			if (PlayGamesHelperObject.instance == this)
			{
				PlayGamesHelperObject.instance = null;
			}
		}

		public static void RunCoroutine(IEnumerator action)
		{
			if (PlayGamesHelperObject.instance != null)
			{
				PlayGamesHelperObject.RunOnGameThread(delegate
				{
					PlayGamesHelperObject.instance.StartCoroutine(action);
				});
			}
		}

		public static void RunOnGameThread(Action action)
		{
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}
			if (PlayGamesHelperObject.sIsDummy)
			{
				return;
			}
			List<Action> obj = PlayGamesHelperObject.sQueue;
			lock (obj)
			{
				PlayGamesHelperObject.sQueue.Add(action);
				PlayGamesHelperObject.sQueueEmpty = false;
			}
		}

		public void Update()
		{
			if (PlayGamesHelperObject.sIsDummy || PlayGamesHelperObject.sQueueEmpty)
			{
				return;
			}
			this.localQueue.Clear();
			List<Action> obj = PlayGamesHelperObject.sQueue;
			lock (obj)
			{
				this.localQueue.AddRange(PlayGamesHelperObject.sQueue);
				PlayGamesHelperObject.sQueue.Clear();
				PlayGamesHelperObject.sQueueEmpty = true;
			}
			for (int i = 0; i < this.localQueue.Count; i++)
			{
				this.localQueue[i]();
			}
		}

		public void OnApplicationFocus(bool focused)
		{
			foreach (Action<bool> action in PlayGamesHelperObject.sFocusCallbackList)
			{
				try
				{
					action(focused);
				}
				catch (Exception ex)
				{
					Logger.e("Exception in OnApplicationFocus:" + ex.Message + "\n" + ex.StackTrace);
				}
			}
		}

		public void OnApplicationPause(bool paused)
		{
			foreach (Action<bool> action in PlayGamesHelperObject.sPauseCallbackList)
			{
				try
				{
					action(paused);
				}
				catch (Exception ex)
				{
					Logger.e("Exception in OnApplicationPause:" + ex.Message + "\n" + ex.StackTrace);
				}
			}
		}

		public static void AddFocusCallback(Action<bool> callback)
		{
			if (!PlayGamesHelperObject.sFocusCallbackList.Contains(callback))
			{
				PlayGamesHelperObject.sFocusCallbackList.Add(callback);
			}
		}

		public static bool RemoveFocusCallback(Action<bool> callback)
		{
			return PlayGamesHelperObject.sFocusCallbackList.Remove(callback);
		}

		public static void AddPauseCallback(Action<bool> callback)
		{
			if (!PlayGamesHelperObject.sPauseCallbackList.Contains(callback))
			{
				PlayGamesHelperObject.sPauseCallbackList.Add(callback);
			}
		}

		public static bool RemovePauseCallback(Action<bool> callback)
		{
			return PlayGamesHelperObject.sPauseCallbackList.Remove(callback);
		}
	}
}
