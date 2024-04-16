using System;
using System.Collections;
using System.Text;
using PowerTools;
using TMPro;
using UnityEngine;

public class UploadDataGame : Minigame
{
	public SpriteAnim LeftFolder;

	public SpriteAnim RightFolder;

	public AnimationClip FolderOpen;

	public AnimationClip FolderClose;

	public SpriteRenderer Runner;

	public HorizontalGauge Gauge;

	public TextMeshPro PercentText;

	public TextMeshPro EstimatedText;

	public TextMeshPro SourceText;

	public TextMeshPro TargetText;

	public SpriteRenderer Button;

	public TextTranslatorTMP translator;

	public GameObject Status;

	public GameObject Tower;

	private int count;

	private float timer;

	public const float RandomChunks = 5f;

	public const float ConstantTime = 3f;

	private bool running = true;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultButtonSelected;

	private void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(base.name);
	}

	public override void Begin(PlayerTask task)
	{
		PlayerControl.LocalPlayer.SetPlayerMaterialColors(this.Runner);
		base.Begin(task);
		if (this.MyNormTask.taskStep == 0)
		{
			this.translator.TargetText = StringNames.Download;
			this.Tower.SetActive(false);
			this.SourceText.text = DestroyableSingleton<TranslationController>.Instance.GetString(this.MyTask.StartAt);
			this.TargetText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MyTablet, Array.Empty<object>());
		}
		else
		{
			this.SourceText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MyTablet, Array.Empty<object>());
			this.TargetText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Headquarters, Array.Empty<object>());
		}
		ControllerManager.Instance.OpenOverlayMenu(base.name, this.BackButton, this.DefaultButtonSelected);
	}

	public void Click()
	{
		base.StartCoroutine(this.Transition());
	}

	private IEnumerator Transition()
	{
		this.Button.gameObject.SetActive(false);
		this.Status.SetActive(true);
		float target = this.Gauge.transform.localScale.x;
		for (float t = 0f; t < 0.15f; t += Time.deltaTime)
		{
			this.Gauge.transform.localScale = new Vector3(t / 0.15f * target, 1f, 1f);
			yield return null;
		}
		base.StartCoroutine(this.PulseText());
		base.StartCoroutine(this.DoRun());
		base.StartCoroutine(this.DoText());
		base.StartCoroutine(this.DoPercent());
		yield break;
	}

	private IEnumerator PulseText()
	{
		MeshRenderer rend2 = this.PercentText.GetComponent<MeshRenderer>();
		MeshRenderer rend1 = this.EstimatedText.GetComponent<MeshRenderer>();
		Color gray = new Color(0.3f, 0.3f, 0.3f, 1f);
		while (this.running)
		{
			yield return new WaitForLerp(0.4f, delegate(float t)
			{
				Color color = Color.Lerp(Color.black, gray, t);
				rend2.material.SetColor("_OutlineColor", color);
				rend1.material.SetColor("_OutlineColor", color);
			});
			yield return new WaitForLerp(0.4f, delegate(float t)
			{
				Color color = Color.Lerp(gray, Color.black, t);
				rend2.material.SetColor("_OutlineColor", color);
				rend1.material.SetColor("_OutlineColor", color);
			});
		}
		rend2.material.SetColor("_OutlineColor", Color.black);
		rend1.material.SetColor("_OutlineColor", Color.black);
		yield break;
	}

	private IEnumerator DoPercent()
	{
		while (this.running)
		{
			float num = (float)this.count / 5f * 0.7f + this.timer / 3f * 0.3f;
			if (num >= 1f)
			{
				this.running = false;
			}
			num = Mathf.Clamp(num, 0f, 1f);
			this.Gauge.Value = num;
			this.PercentText.text = Mathf.RoundToInt(num * 100f).ToString() + "%";
			yield return null;
		}
		yield break;
	}

	private IEnumerator DoText()
	{
		StringBuilder txt = new StringBuilder("Estimated Time: ");
		int baselen = txt.Length;
		int max = 604800;
		this.count = 0;
		while ((float)this.count < 5f)
		{
			txt.Length = baselen;
			int num = IntRange.Next(max / 6, max);
			int num2 = num / 86400;
			int num3 = num / 3600 % 24;
			int num4 = num / 60 % 60;
			int num5 = num % 60;
			string @string;
			if (num2 > 0)
			{
				@string = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.DownloadTestEstTimeDHMS, new object[]
				{
					num2,
					num3,
					num4,
					num5
				});
			}
			else if (num3 > 0)
			{
				@string = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.DownloadTestEstTimeHMS, new object[]
				{
					num3,
					num4,
					num5
				});
			}
			else if (num4 > 0)
			{
				@string = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.DownloadTestEstTimeMS, new object[]
				{
					num4,
					num5
				});
			}
			else
			{
				@string = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.DownloadTestEstTimeS, new object[]
				{
					num5
				});
			}
			this.EstimatedText.text = @string;
			max /= 4;
			yield return new WaitForSeconds(FloatRange.Next(0.6f, 1.2f));
			this.count++;
		}
		this.timer = 0f;
		while (this.timer < 3f)
		{
			txt.Length = baselen;
			int num6 = Mathf.RoundToInt(3f - this.timer);
			this.EstimatedText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.DownloadTestEstTimeS, new object[]
			{
				num6
			});
			yield return null;
			this.timer += Time.deltaTime;
		}
		yield break;
	}

	private IEnumerator DoRun()
	{
		while (this.running)
		{
			LeftFolder.Play(this.FolderOpen);
			var pos = this.Runner.transform.localPosition;
			yield return new WaitForLerp(1.125f, f =>
			{
				pos.x = Mathf.Lerp(-1.25f, 0.5625f, f);
				this.Runner.transform.localPosition = pos;
			});
			this.LeftFolder.Play(this.FolderClose);
			this.RightFolder.Play(this.FolderOpen);
			yield return new WaitForLerp(1.375f, t => 
			{ 
				pos.x = Mathf.Lerp(0.5625f, 1.25f, t);
				Runner.transform.localPosition = pos;
			});
			
			yield return new WaitForAnimationFinish(this.RightFolder, this.FolderClose);
		}
		this.EstimatedText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.DownloadComplete, Array.Empty<object>());
		this.MyNormTask.NextStep();
		base.StartCoroutine(base.CoStartClose(0.75f));
		yield break;
	}
}
