using System;
using System.Collections.Generic;
using InnerNet;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class FindAGameManager : DestroyableSingleton<FindAGameManager>, IGameListHandler
{
	private const float RefreshTime = 5f;

	private float timer;

	public ObjectPoolBehavior buttonPool;

	public SpinAnimator RefreshSpinner;

	public Scroller TargetArea;

	public TextMeshPro TotalText;

	public float ButtonStart = 1.75f;

	public float ButtonHeight = 0.6f;

	public const bool showPrivate = false;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultButtonSelected;

	public List<UiElement> ControllerSelectable;

	public void ResetTimer()
	{
		this.timer = 5f;
		this.RefreshSpinner.Appear();
		this.RefreshSpinner.StartPulse();
	}

	public void Start()
	{
		if (!AmongUsClient.Instance)
		{
			AmongUsClient.Instance = UnityEngine.Object.FindObjectOfType<AmongUsClient>();
			if (!AmongUsClient.Instance)
			{
				SceneManager.LoadScene("MMOnline");
				return;
			}
		}
		AmongUsClient.Instance.GameListHandlers.Add(this);
		AmongUsClient.Instance.RequestGameList(false, SaveManager.GameSearchOptions);
		ControllerManager.Instance.NewScene(base.name, this.BackButton, this.DefaultButtonSelected, this.ControllerSelectable, false);
	}

	public void Update()
	{
		this.timer += Time.deltaTime;
		GameOptionsData gameSearchOptions = SaveManager.GameSearchOptions;
		if ((this.timer < 0f || this.timer > 5f) && gameSearchOptions.MapId != 0)
		{
			this.RefreshSpinner.Appear();
		}
		else
		{
			this.RefreshSpinner.Disappear();
		}
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			this.ExitGame();
		}
	}

	public void RefreshList()
	{
		if (this.timer > 5f)
		{
			this.timer = -5f;
			this.RefreshSpinner.Play();
			AmongUsClient.Instance.RequestGameList(false, SaveManager.GameSearchOptions);
		}
	}

	public override void OnDestroy()
	{
		if (AmongUsClient.Instance)
		{
			AmongUsClient.Instance.GameListHandlers.Remove(this);
		}
		base.OnDestroy();
	}

	public void HandleList(InnerNetClient.TotalGameData totalGames, List<GameListing> availableGames)
	{
		if (totalGames.PerMapTotals != null)
		{
			string text = string.Empty;
			for (int i = 0; i < AmongUsClient.Instance.ShipPrefabs.Count; i++)
			{
				text += string.Format("Total {0} Games: {1}        ", GameOptionsData.MapNames[i], totalGames.PerMapTotals[i]);
			}
			text.TrimEnd(Array.Empty<char>());
			this.TotalText.text = text;
		}
		try
		{
			this.RefreshSpinner.Disappear();
			this.timer = 0f;
			availableGames.Sort(FindAGameManager.GameSorter.Instance);
			while (this.buttonPool.activeChildren.Count > availableGames.Count)
			{
				PoolableBehavior poolableBehavior = this.buttonPool.activeChildren[this.buttonPool.activeChildren.Count - 1];
				poolableBehavior.OwnerPool.Reclaim(poolableBehavior);
			}
			while (this.buttonPool.activeChildren.Count < availableGames.Count)
			{
				this.buttonPool.Get<PoolableBehavior>().transform.SetParent(this.TargetArea.Inner);
			}
			Vector3 vector = new Vector3(0f, this.ButtonStart, -1f);
			for (int j = 0; j < this.buttonPool.activeChildren.Count; j++)
			{
				MatchMakerGameButton matchMakerGameButton = (MatchMakerGameButton)this.buttonPool.activeChildren[j];
				matchMakerGameButton.SetGame(availableGames[j]);
				matchMakerGameButton.transform.localPosition = vector;
				vector.y -= this.ButtonHeight;
				PassiveButton component = matchMakerGameButton.gameObject.GetComponent<PassiveButton>();
				ControllerManager.Instance.AddSelectableUiElement(component, false);
			}
			this.TargetArea.YBounds.max = Mathf.Max(0f, -vector.y - this.ButtonStart);
		}
		catch (Exception ex)
		{
			Debug.LogError("FindAGameManager::HandleList: Exception: ");
			Debug.LogException(ex, this);
		}
	}

	public void ExitGame()
	{
		AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);
	}

	private class GameSorter : IComparer<GameListing>
	{
		public static readonly FindAGameManager.GameSorter Instance = new FindAGameManager.GameSorter();

		public int Compare(GameListing x, GameListing y)
		{
			return -x.PlayerCount.CompareTo(y.PlayerCount);
		}
	}
}
