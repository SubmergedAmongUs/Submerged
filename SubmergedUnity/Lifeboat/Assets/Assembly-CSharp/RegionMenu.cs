using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class RegionMenu : MonoBehaviour
{
	public ObjectPoolBehavior ButtonPool;

	public TextMeshPro RegionText;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	private UiElement defaultButtonSelected;

	private List<UiElement> controllerSelectable;

	private void Awake()
	{
		this.defaultButtonSelected = null;
		this.controllerSelectable = new List<UiElement>();
	}

	public void OnEnable()
	{
		this.controllerSelectable.Clear();
		int num = 0;
		foreach (IRegionInfo regionInfo in from s in DestroyableSingleton<ServerManager>.Instance.AvailableRegions
		orderby (uint)ServerManager.DefaultRegions.IndexOf((IRegionInfo d) => d.Name.Equals(s.Name))
		select s)
		{
			IRegionInfo region = regionInfo;
			ServerListButton serverListButton = this.ButtonPool.Get<ServerListButton>();
			serverListButton.transform.localPosition = new Vector3(0f, 2f - 0.5f * (float)num, 0f);
			serverListButton.SetTextTranslationId(regionInfo.TranslateName, regionInfo.Name);
			serverListButton.Text.text = DestroyableSingleton<TranslationController>.Instance.GetStringWithDefault(regionInfo.TranslateName, regionInfo.Name, Array.Empty<object>());
			serverListButton.Text.ForceMeshUpdate(false, false);
			serverListButton.Button.OnClick.AddListener(delegate()
			{
				this.ChooseOption(region);
			});
			serverListButton.SetSelected(DestroyableSingleton<ServerManager>.Instance.CurrentRegion.Equals(regionInfo));
			if (DestroyableSingleton<ServerManager>.Instance.CurrentRegion.Equals(regionInfo))
			{
				this.defaultButtonSelected = serverListButton.Button;
			}
			this.controllerSelectable.Add(serverListButton.Button);
			num++;
		}
		if (this.defaultButtonSelected == null && this.controllerSelectable.Count > 0)
		{
			this.defaultButtonSelected = this.controllerSelectable[0];
		}
		ControllerManager.Instance.OpenOverlayMenu(base.name, this.BackButton, this.defaultButtonSelected, this.controllerSelectable, false);
		num++;
	}

	private void OpenCustomRegion()
	{
		throw new NotImplementedException();
	}

	public void OnDisable()
	{
		this.ButtonPool.ReclaimAll();
		ControllerManager.Instance.CloseOverlayMenu(base.name);
	}

	public void Open()
	{
		base.gameObject.SetActive(true);
	}

	public void ChooseOption(IRegionInfo region)
	{
		DestroyableSingleton<ServerManager>.Instance.SetRegion(region);
		this.RegionText.text = DestroyableSingleton<TranslationController>.Instance.GetStringWithDefault(region.TranslateName, region.Name, Array.Empty<object>());
		this.Close();
	}

	public void Close()
	{
		base.gameObject.SetActive(false);
	}
}
