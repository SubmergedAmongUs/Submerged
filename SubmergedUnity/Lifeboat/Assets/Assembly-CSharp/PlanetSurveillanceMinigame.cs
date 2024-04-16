using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class PlanetSurveillanceMinigame : Minigame
{
	public Camera Camera;

	public GameObject Viewables;

	public MeshRenderer ViewPort;

	public TextMeshPro LocationName;

	public TextMeshPro SabText;

	private RenderTexture texture;

	public MeshRenderer FillQuad;

	public Material DefaultMaterial;

	public Material StaticMaterial;

	private bool isStatic;

	private SurvCamera[] survCameras;

	public Transform DotParent;

	private SpriteRenderer[] Dots;

	public Sprite DotEnabled;

	public Sprite DotDisabled;

	private int currentCamera;

	public AudioClip ChangeSound;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		DestroyableSingleton<HudManager>.Instance.PlayerCam.Locked = true;
		RenderTexture temporary = RenderTexture.GetTemporary(330, 230, 16, (RenderTextureFormat) 0);
		this.texture = temporary;
		this.Camera.targetTexture = temporary;
		this.ViewPort.material.SetTexture("_MainTex", temporary);
		this.survCameras = ShipStatus.Instance.AllCameras;
		this.Dots = new SpriteRenderer[this.survCameras.Length];
		for (int i = 0; i < this.Dots.Length; i++)
		{
			GameObject gameObject = new GameObject("Dot" + i.ToString(), new Type[]
			{
				typeof(SpriteRenderer)
			});
			gameObject.layer = base.gameObject.layer;
			gameObject.transform.SetParent(this.DotParent);
			SpriteRenderer component = gameObject.GetComponent<SpriteRenderer>();
			component.sprite = this.DotDisabled;
			this.Dots[i] = component;
		}
		DotAligner.Align(this.DotParent, 0.25f, true);
		this.NextCamera(0);
		if (!PlayerControl.LocalPlayer.Data.IsDead)
		{
			ShipStatus.Instance.RpcRepairSystem(SystemTypes.Security, 1);
		}
		base.SetupInput(true);
	}

	public void Update()
	{
		if (this.isStatic && !PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
		{
			this.isStatic = false;
			this.ViewPort.sharedMaterial = this.DefaultMaterial;
			this.ViewPort.material.SetTexture("_MainTex", this.texture);
			this.SabText.gameObject.SetActive(false);
			return;
		}
		if (!this.isStatic && PlayerTask.PlayerHasTaskOfType<HudOverrideTask>(PlayerControl.LocalPlayer))
		{
			this.isStatic = true;
			this.ViewPort.sharedMaterial = this.StaticMaterial;
			this.SabText.gameObject.SetActive(true);
		}
	}

	public void NextCamera(int direction)
	{
		if (direction != 0 && Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.ChangeSound, false, 1f);
		}
		this.Dots[this.currentCamera].sprite = this.DotDisabled;
		this.currentCamera = (this.currentCamera + direction).Wrap(this.survCameras.Length);
		this.Dots[this.currentCamera].sprite = this.DotEnabled;
		SurvCamera survCamera = this.survCameras[this.currentCamera];
		this.Camera.transform.position = survCamera.transform.position + this.survCameras[this.currentCamera].Offset;
		this.LocationName.text = ((survCamera.NewName > StringNames.ExitButton) ? DestroyableSingleton<TranslationController>.Instance.GetString(survCamera.NewName, Array.Empty<object>()) : survCamera.CamName);
		if (!PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
		{
			base.StartCoroutine(this.PulseStatic());
		}
	}

	private IEnumerator PulseStatic()
	{
		this.ViewPort.sharedMaterial = this.StaticMaterial;
		this.ViewPort.material.SetTexture("_MainTex", null);
		yield return Effects.Wait(0.2f);
		this.ViewPort.sharedMaterial = this.DefaultMaterial;
		this.ViewPort.material.SetTexture("_MainTex", this.texture);
		this.isStatic = false;
		yield break;
	}

	protected override IEnumerator CoAnimateOpen()
	{
		this.Viewables.SetActive(false);
		this.FillQuad.material.SetFloat("_Center", -5f);
		this.FillQuad.material.SetColor("_Color2", Color.clear);
		for (float timer = 0f; timer < 0.25f; timer += Time.deltaTime)
		{
			this.FillQuad.material.SetColor("_Color2", Color.Lerp(Color.clear, Color.black, timer / 0.25f));
			yield return null;
		}
		this.FillQuad.material.SetColor("_Color2", Color.black);
		this.Viewables.SetActive(true);
		for (float timer = 0f; timer < 0.1f; timer += Time.deltaTime)
		{
			this.FillQuad.material.SetFloat("_Center", Mathf.Lerp(-5f, 0f, timer / 0.1f));
			yield return null;
		}
		for (float timer = 0f; timer < 0.15f; timer += Time.deltaTime)
		{
			this.FillQuad.material.SetFloat("_Center", Mathf.Lerp(-3f, 0.4f, timer / 0.15f));
			yield return null;
		}
		this.FillQuad.material.SetFloat("_Center", 0.4f);
		yield break;
	}

	private IEnumerator CoAnimateClose()
	{
		for (float timer = 0f; timer < 0.1f; timer += Time.deltaTime)
		{
			this.FillQuad.material.SetFloat("_Center", Mathf.Lerp(0.4f, -5f, timer / 0.1f));
			yield return null;
		}
		this.Viewables.SetActive(false);
		for (float timer = 0f; timer < 0.3f; timer += Time.deltaTime)
		{
			this.FillQuad.material.SetColor("_Color2", Color.Lerp(Color.black, Color.clear, timer / 0.3f));
			yield return null;
		}
		this.FillQuad.material.SetColor("_Color2", Color.clear);
		yield break;
	}

	protected override IEnumerator CoDestroySelf()
	{
		DestroyableSingleton<HudManager>.Instance.PlayerCam.Locked = false;
		yield return this.CoAnimateClose();
		 UnityEngine.Object.Destroy(base.gameObject);
		yield break;
	}

	public override void Close()
	{
		ShipStatus.Instance.RpcRepairSystem(SystemTypes.Security, 2);
		base.Close();
	}

	public void OnDestroy()
	{
		this.texture.Release();
	}
}
