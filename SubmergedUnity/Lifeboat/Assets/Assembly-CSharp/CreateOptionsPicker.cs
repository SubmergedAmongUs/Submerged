using System;
using System.Collections.Generic;
using InnerNet;
using TMPro;
using UnityEngine;

public class CreateOptionsPicker : MonoBehaviour
{
	private const float MaxPlayerButtonWidth = 0.5f;

	public SpriteRenderer MaxPlayerButtonPrefab;

	private List<SpriteRenderer> MaxPlayerButtons = new List<SpriteRenderer>();

	public Transform MaxPlayersRoot;

	public SpriteRenderer[] ImpostorButtons;

	public TextMeshPro LanguageButton;

	public SpriteRenderer[] MapButtons;

	public SettingsMode mode;

	public CrewVisualizer CrewArea;

	public CreateGameOptions optionsMenu;

	public void Start()
	{
		if (this.MaxPlayersRoot)
		{
			for (int i = 4; i <= 15; i++)
			{
				SpriteRenderer spriteRenderer = UnityEngine.Object.Instantiate<SpriteRenderer>(this.MaxPlayerButtonPrefab, this.MaxPlayersRoot);
				spriteRenderer.transform.localPosition = new Vector3((float)(i - 4) * 0.5f, 0f, 0f);
				int numPlayers = i;
				spriteRenderer.name = numPlayers.ToString();
				PassiveButton component = spriteRenderer.GetComponent<PassiveButton>();
				component.OnClick.AddListener(delegate()
				{
					this.SetMaxPlayersButtons(numPlayers);
				});
				spriteRenderer.GetComponentInChildren<TextMeshPro>().text = spriteRenderer.name;
				this.MaxPlayerButtons.Add(spriteRenderer);
				this.optionsMenu.ControllerSelectable.Add(component);
			}
		}
		for (int j = 0; j < this.MapButtons.Length; j++)
		{
			if (j < AmongUsClient.Instance.ShipPrefabs.Count)
			{
				this.MapButtons[j].gameObject.SetActive(true);
			}
			else
			{
				this.MapButtons[j].gameObject.SetActive(false);
			}
		}
		GameOptionsData targetOptions = this.GetTargetOptions();
		this.UpdateImpostorsButtons(targetOptions.NumImpostors);
		this.UpdateMaxPlayersButtons(targetOptions);
		this.UpdateLanguageButton((uint)targetOptions.Keywords);
		this.UpdateMapButtons((int)targetOptions.MapId);
	}

	public GameOptionsData GetTargetOptions()
	{
		if (this.mode == SettingsMode.Host)
		{
			return SaveManager.GameHostOptions;
		}
		GameOptionsData gameSearchOptions = SaveManager.GameSearchOptions;
		if (gameSearchOptions.MapId == 0)
		{
			gameSearchOptions.ToggleMapFilter(0);
			SaveManager.GameSearchOptions = gameSearchOptions;
		}
		return gameSearchOptions;
	}

	private void SetTargetOptions(GameOptionsData data)
	{
		if (this.mode == SettingsMode.Host)
		{
			SaveManager.GameHostOptions = data;
			return;
		}
		SaveManager.GameSearchOptions = data;
	}

	public void SetMaxPlayersButtons(int maxPlayers)
	{
		GameOptionsData targetOptions = this.GetTargetOptions();
		if (maxPlayers < GameOptionsData.MinPlayers[targetOptions.NumImpostors])
		{
			return;
		}
		targetOptions.MaxPlayers = maxPlayers;
		this.SetTargetOptions(targetOptions);
		if (DestroyableSingleton<FindAGameManager>.InstanceExists)
		{
			DestroyableSingleton<FindAGameManager>.Instance.ResetTimer();
		}
		this.UpdateMaxPlayersButtons(targetOptions);
	}

	private void UpdateMaxPlayersButtons(GameOptionsData opts)
	{
		if (this.CrewArea)
		{
			this.CrewArea.SetCrewSize(opts.MaxPlayers, opts.NumImpostors);
		}
		for (int i = 0; i < this.MaxPlayerButtons.Count; i++)
		{
			SpriteRenderer spriteRenderer = this.MaxPlayerButtons[i];
			spriteRenderer.enabled = (spriteRenderer.name == opts.MaxPlayers.ToString());
			spriteRenderer.GetComponentInChildren<TextMeshPro>().color = ((int.Parse(spriteRenderer.name) < GameOptionsData.MinPlayers[opts.NumImpostors]) ? Palette.DisabledGrey : Color.white);
		}
	}

	public void SetImpostorButtons(int numImpostors)
	{
		GameOptionsData targetOptions = this.GetTargetOptions();
		targetOptions.NumImpostors = numImpostors;
		this.SetTargetOptions(targetOptions);
		this.SetMaxPlayersButtons(Mathf.Max(targetOptions.MaxPlayers, GameOptionsData.MinPlayers[numImpostors]));
		this.UpdateImpostorsButtons(numImpostors);
	}

	private void UpdateImpostorsButtons(int numImpostors)
	{
		for (int i = 0; i < this.ImpostorButtons.Length; i++)
		{
			SpriteRenderer spriteRenderer = this.ImpostorButtons[i];
			spriteRenderer.enabled = (spriteRenderer.name == numImpostors.ToString());
		}
	}

	public void SetMap(int mapid)
	{
		GameOptionsData targetOptions = this.GetTargetOptions();
		if (this.mode == SettingsMode.Host)
		{
			targetOptions.MapId = (byte)mapid;
		}
		else
		{
			targetOptions.ToggleMapFilter((byte)mapid);
		}
		this.SetTargetOptions(targetOptions);
		if (DestroyableSingleton<FindAGameManager>.InstanceExists)
		{
			DestroyableSingleton<FindAGameManager>.Instance.ResetTimer();
		}
		this.UpdateMapButtons(mapid);
	}

	private void UpdateMapButtons(int mapid)
	{
		if (this.mode == SettingsMode.Host)
		{
			if (this.CrewArea)
			{
				this.CrewArea.SetMap(mapid);
			}
			for (int i = 0; i < this.MapButtons.Length; i++)
			{
				SpriteRenderer spriteRenderer = this.MapButtons[i];
				spriteRenderer.color = ((spriteRenderer.name == mapid.ToString()) ? Color.white : Palette.Black);
			}
		}
		else
		{
			GameOptionsData targetOptions = this.GetTargetOptions();
			for (int j = 0; j < this.MapButtons.Length; j++)
			{
				SpriteRenderer spriteRenderer2 = this.MapButtons[j];
				spriteRenderer2.color = (targetOptions.FilterContainsMap(byte.Parse(spriteRenderer2.name)) ? Color.white : Palette.DisabledGrey);
			}
		}
		if (Constants.ShouldFlipSkeld())
		{
			this.MapButtons[0].flipX = true;
		}
	}

	public void SetLanguageFilter(uint keyword)
	{
		GameOptionsData targetOptions = this.GetTargetOptions();
		targetOptions.Keywords = (GameKeywords)keyword;
		this.SetTargetOptions(targetOptions);
		if (DestroyableSingleton<FindAGameManager>.InstanceExists)
		{
			DestroyableSingleton<FindAGameManager>.Instance.ResetTimer();
		}
		this.UpdateLanguageButton(keyword);
	}

	private void UpdateLanguageButton(uint flag)
	{
		if (ChatLanguageSet.Instance.GetString(flag) == "Other")
		{
			this.LanguageButton.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.OtherLanguage, Array.Empty<object>());
			return;
		}
		this.LanguageButton.text = ChatLanguageSet.Instance.GetString(flag);
	}
}
