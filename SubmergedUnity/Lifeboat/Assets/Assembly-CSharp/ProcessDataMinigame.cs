using System;
using System.Collections;
using System.Text;
using PowerTools;
using TMPro;
using UnityEngine;

public class ProcessDataMinigame : Minigame
{
	private string[] DocTopics = new string[]
	{
		"important",
		"amongis",
		"lifeform",
		"danger",
		"mining",
		"rocks",
		"minerals",
		"dirt",
		"soil",
		"life",
		"specimen",
		"lookatthis",
		"wut",
		"happy_birthday",
		"1internet",
		"cake",
		"pineapple"
	};

	private string[] DocTypes = new string[]
	{
		"data",
		"srsbiz",
		"finances",
		"report",
		"growth",
		"results",
		"investigation"
	};

	private string[] DocExtensions = new string[]
	{
		".png",
		".tiff",
		".txt",
		".csv",
		".doc",
		".file",
		".data",
		".jpg",
		".raw",
		".xsl",
		".dot",
		".dat",
		".doof",
		".mira",
		".space"
	};

	public float Duration = 5f;

	public ParallaxController scenery;

	public PassiveButton StartButton;

	public TextMeshPro EstimatedText;

	public TextMeshPro PercentText;

	public SpriteAnim LeftFolder;

	public SpriteAnim RightFolder;

	public AnimationClip OpenFolderClip;

	public AnimationClip CloseFolderClip;

	public GameObject Status;

	public SpriteRenderer Runner;

	public HorizontalGauge Gauge;

	private bool running = true;

	public FloatRange SceneRange = new FloatRange(0f, 50f);

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultButtonSelected;

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(base.name);
	}

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		PlayerControl.LocalPlayer.SetPlayerMaterialColors(this.Runner);
		ControllerManager.Instance.OpenOverlayMenu(base.name, this.BackButton, this.DefaultButtonSelected);
	}

	public void StartStopFill()
	{
		this.StartButton.enabled = false;
		base.StartCoroutine(this.CoDoAnimation());
	}

	private IEnumerator CoDoAnimation()
	{
		this.LeftFolder.Play(this.OpenFolderClip, 1f);
		yield return this.Transition();
		base.StartCoroutine(this.DoText());
		for (float timer = 0f; timer < this.Duration; timer += Time.deltaTime)
		{
			float num = timer / this.Duration;
			this.Gauge.Value = num;
			this.PercentText.text = Mathf.RoundToInt(num * 100f).ToString() + "%";
			this.scenery.SetParallax(this.SceneRange.Lerp(num));
			yield return null;
		}
		this.running = false;
		this.EstimatedText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.WeatherComplete, Array.Empty<object>());
		this.RightFolder.Play(this.CloseFolderClip, 1f);
		this.MyNormTask.NextStep();
		yield return base.CoStartClose(0.75f);
		yield break;
	}

	private IEnumerator Transition()
	{
		yield return Effects.ScaleIn(this.StartButton.transform, 1f, 0f, 0.15f);
		this.Status.SetActive(true);
		for (float t = 0f; t < 0.15f; t += Time.deltaTime)
		{
			this.Gauge.transform.localScale = new Vector3(t / 0.15f, 1f, 1f);
			yield return null;
		}
		this.Gauge.transform.localScale = new Vector3(1f, 1f, 1f);
		yield break;
	}

	private IEnumerator DoText()
	{
		StringBuilder txt = new StringBuilder(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Processing, Array.Empty<object>()) + ": ");
		int len = txt.Length;
		while (this.running)
		{
			txt.Append(this.DocTopics.Random<string>());
			txt.Append("_");
			txt.Append(this.DocTypes.Random<string>());
			txt.Append(this.DocExtensions.Random<string>());
			this.EstimatedText.text = txt.ToString();
			yield return Effects.Wait(FloatRange.Next(0.025f, 0.15f));
			txt.Length = len;
		}
		yield break;
	}
}
