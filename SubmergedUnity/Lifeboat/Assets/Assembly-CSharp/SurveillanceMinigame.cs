using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class SurveillanceMinigame : Minigame
{
	public Camera CameraPrefab;

	public GameObject Viewables;

	public MeshRenderer[] ViewPorts;

	public TextMeshPro[] SabText;

	private PlainShipRoom[] FilteredRooms;

	private RenderTexture[] textures;

	public MeshRenderer FillQuad;

	public Material DefaultMaterial;

	public Material StaticMaterial;

	private bool isStatic;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		DestroyableSingleton<HudManager>.Instance.PlayerCam.Locked = true;
		this.FilteredRooms = (from i in ShipStatus.Instance.AllRooms
		where i.survCamera
		select i).ToArray<PlainShipRoom>();
		this.textures = new RenderTexture[this.FilteredRooms.Length];
		for (int j = 0; j < this.FilteredRooms.Length; j++)
		{
			PlainShipRoom plainShipRoom = this.FilteredRooms[j];
			Camera camera = UnityEngine.Object.Instantiate<Camera>(this.CameraPrefab);
			camera.transform.SetParent(base.transform);
			camera.transform.position = plainShipRoom.transform.position + plainShipRoom.survCamera.Offset;
			camera.orthographicSize = plainShipRoom.survCamera.CamSize;
			RenderTexture temporary = RenderTexture.GetTemporary((int)(256f * plainShipRoom.survCamera.CamAspect), 256, 16, (RenderTextureFormat) 0);
			this.textures[j] = temporary;
			camera.targetTexture = temporary;
			this.ViewPorts[j].material.SetTexture("_MainTex", temporary);
		}
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
			for (int i = 0; i < this.ViewPorts.Length; i++)
			{
				this.ViewPorts[i].sharedMaterial = this.DefaultMaterial;
				this.ViewPorts[i].material.SetTexture("_MainTex", this.textures[i]);
				this.SabText[i].gameObject.SetActive(false);
			}
			return;
		}
		if (!this.isStatic && PlayerTask.PlayerHasTaskOfType<HudOverrideTask>(PlayerControl.LocalPlayer))
		{
			this.isStatic = true;
			for (int j = 0; j < this.ViewPorts.Length; j++)
			{
				this.ViewPorts[j].sharedMaterial = this.StaticMaterial;
				this.SabText[j].gameObject.SetActive(true);
			}
		}
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
		for (int i = 0; i < this.textures.Length; i++)
		{
			this.textures[i].Release();
		}
	}
}
