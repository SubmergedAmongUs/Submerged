using System;
using System.Collections;
using GameCore;
using UnityEngine;

public class SignInUserOnButtonPress : MonoBehaviour
{
	public Action OnButtonPress;

	public SceneChanger SceneChanger;

	public SpriteRenderer FillScreen;

	public TextTranslatorTMP Text;

	private bool inviteReceived;

	private void OnEnable()
	{
		NetworkManager networkManager = GameCoreManager.Instance.NetworkManager;
		networkManager.OnInvited = (NetworkManager.OnInvitedCallback)Delegate.Combine(networkManager.OnInvited, new NetworkManager.OnInvitedCallback(this.HandleInvite));
	}

	private void OnDisable()
	{
		NetworkManager networkManager = GameCoreManager.Instance.NetworkManager;
		networkManager.OnInvited = (NetworkManager.OnInvitedCallback)Delegate.Remove(networkManager.OnInvited, new NetworkManager.OnInvitedCallback(this.HandleInvite));
	}

	private void Update()
	{
	}

	public void AddUserWithUI()
	{
	}

	private void AddUserCompleted(UserManager.UserOpResult result)
	{
	}

	private IEnumerator ContinueToNextScene()
	{
		base.StartCoroutine(this.FadeToBlack());
		yield return null;
		this.SceneChanger.AllowFinishLoadingScene();
		yield break;
	}

	public void HandleInvite(string connectionString)
	{
		this.SceneChanger.BeginLoadingSceneAdditive();
		this.inviteReceived = true;
		this.AddUserWithUI();
	}

	private IEnumerator FadeToBlack()
	{
		if (this.FillScreen)
		{
			this.FillScreen.gameObject.SetActive(true);
			for (float time = 0f; time < 0.25f; time += Time.deltaTime)
			{
				this.FillScreen.color = Color.Lerp(Color.clear, Color.black, time / 0.25f);
				yield return null;
			}
			this.FillScreen.color = Color.black;
		}
		yield break;
	}
}
