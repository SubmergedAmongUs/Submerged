using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChanger : MonoBehaviour
{
	public string TargetScene;

	public bool disallowBasedOnSwitchParentalControls;

	public bool disallowBasedOnAssetPackDownloads;

	public GameObject ConnectIcon;

	public Button.ButtonClickedEvent BeforeSceneChange;

	private AsyncOperation loadOp;

	public void Click()
	{
		if (this.AllowSceneChange())
		{
			this.ChangeScene();
		}
	}

	private void ChangeScene()
	{
		Debug.Log("SceneChanger::ChangeScene to: " + this.TargetScene);
		this.BeforeSceneChange.Invoke();
		SceneChanger.ChangeScene(this.TargetScene);
	}

	public static void ChangeScene(string target)
	{
		SceneManager.LoadScene(target);
	}

	public void ExitGame()
	{
		Application.Quit();
	}

	public void BeginLoadingScene()
	{
		this.BeginLoadingSceneInternal(0);
	}

	public void BeginLoadingSceneAdditive()
	{
		this.BeginLoadingSceneInternal(LoadSceneMode.Additive);
	}

	private void BeginLoadingSceneInternal(LoadSceneMode mode)
	{
		SceneChanger.SceneManagerCallbacks.Init();
		if (this.loadOp == null)
		{
			Debug.Log("Begin async loading " + this.TargetScene);
			this.loadOp = SceneManager.LoadSceneAsync(this.TargetScene, mode);
			this.loadOp.allowSceneActivation = false;
		}
	}

	public void AllowFinishLoadingScene()
	{
		if (this.loadOp != null)
		{
			Debug.LogError(string.Concat(new string[]
			{
				"Allow async load for ",
				this.TargetScene,
				" to complete, currently at ",
				(this.loadOp.progress * 100f).ToString(),
				"%"
			}));
			this.loadOp.allowSceneActivation = true;
			this.loadOp = null;
		}
	}

	private bool AllowSceneChange()
	{
		if (this.disallowBasedOnAssetPackDownloads)
		{
			MainMenuManager mainMenuManager = UnityEngine.Object.FindObjectOfType<MainMenuManager>();
			if (mainMenuManager && mainMenuManager.googlePlayAssetHandler.RejectedDownload())
			{
				base.StartCoroutine(mainMenuManager.googlePlayAssetHandler.PromptUser());
				return false;
			}
		}
		return true;
	}

	public static class SceneManagerCallbacks
	{
		static SceneManagerCallbacks()
		{
			Debug.Log("Hooked up SceneManager callback");
			SceneManager.activeSceneChanged += new UnityAction<Scene, Scene>(SceneChanger.SceneManagerCallbacks.SceneManager_activeSceneChanged);
			SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(SceneChanger.SceneManagerCallbacks.SceneManager_sceneLoaded);
		}

		private static void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
		{
		}

		private static void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
		{
			if (mode == LoadSceneMode.Additive)
			{
				SceneManager.SetActiveScene(scene);
			}
		}

		public static void Init()
		{
		}
	}
}
