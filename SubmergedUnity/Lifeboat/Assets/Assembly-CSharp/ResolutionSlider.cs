using System;
using System.Linq;
using TMPro;
using UnityEngine;

public class ResolutionSlider : MonoBehaviour
{
	private int targetIdx;

	private Resolution targetResolution;

	private bool targetFullscreen;

	private Resolution[] allResolutions;

	public SlideBar slider;

	public ToggleButtonBehaviour Fullscreen;

	public ToggleButtonBehaviour VSync;

	public TextMeshPro Display;

	public void OnEnable()
	{
		this.allResolutions = (from r in Screen.resolutions
		where r.height > 480
		select r).ToArray<Resolution>();
		this.targetResolution = Screen.currentResolution;
		this.targetFullscreen = Screen.fullScreen;
		this.targetIdx = this.allResolutions.IndexOf((Resolution e) => e.width == this.targetResolution.width && e.height == this.targetResolution.height);
		this.slider.Value = (float)this.targetIdx / ((float)this.allResolutions.Length - 1f);
		this.Display.text = string.Format("{0}x{1}", this.targetResolution.width, this.targetResolution.height);
		this.Fullscreen.UpdateText(this.targetFullscreen);
		this.VSync.UpdateText(SaveManager.VSync);
	}

	public void ToggleVSync()
	{
		SaveManager.VSync = !SaveManager.VSync;
		if (SaveManager.VSync)
		{
			QualitySettings.vSyncCount = 1;
		}
		else
		{
			QualitySettings.vSyncCount = 0;
		}
		this.VSync.UpdateText(SaveManager.VSync);
	}

	public void ToggleFullscreen()
	{
		this.targetFullscreen = !this.targetFullscreen;
		this.Fullscreen.UpdateText(this.targetFullscreen);
	}

	public void OnResChange()
	{
		int num = Mathf.RoundToInt((float)(this.allResolutions.Length - 1) * this.slider.Value);
		if (num != this.targetIdx)
		{
			this.targetIdx = num;
			this.targetResolution = this.allResolutions[num];
			this.Display.text = string.Format("{0}x{1}", this.targetResolution.width, this.targetResolution.height);
		}
		this.slider.Value = (float)this.targetIdx / ((float)this.allResolutions.Length - 1f);
	}

	public void SaveChange()
	{
		ResolutionManager.SetResolution(this.targetResolution.width, this.targetResolution.height, this.targetFullscreen);
	}
}
