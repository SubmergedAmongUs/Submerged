using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SaveIconCamera : DestroyableSingleton<SaveIconCamera>
{
	private Camera cam;

	public ConditionalRenderTexture platformRenderTextures;

	private RenderTexture targetTexture;

	public PlayerControl saveIconDummy;

	private static object lockObject = 1;

	private static volatile bool needsRender = false;

	private static byte[] renderedPNG;

	private new void Awake()
	{
		this.targetTexture = this.platformRenderTextures;
	}

	private void Start()
	{
		this.cam = base.GetComponent<Camera>();
		if (this.cam)
		{
			this.cam.enabled = false;
		}
		this.saveIconDummy.gameObject.SetActive(false);
	}

	private void LateUpdate()
	{
		object obj = SaveIconCamera.lockObject;
		lock (obj)
		{
			if (SaveIconCamera.needsRender)
			{
				SaveIconCamera.renderedPNG = this.RenderSaveIconLocal();
				SaveIconCamera.needsRender = false;
			}
		}
	}

	[ContextMenu("Test Render Icon")]
	private void TestIcon()
	{
		this.RenderSaveIconLocal();
	}

	public static byte[] RenderSaveIcon()
	{
		object obj = SaveIconCamera.lockObject;
		lock (obj)
		{
			SaveIconCamera.needsRender = true;
			goto IL_2E;
		}
		IL_24:
		Debug.Log("Waiting for render");
		IL_2E:
		if (!SaveIconCamera.needsRender)
		{
			return SaveIconCamera.renderedPNG;
		}
		goto IL_24;
	}

	private byte[] RenderSaveIconLocal()
	{
		this.saveIconDummy.gameObject.SetActive(true);
		this.cam.targetTexture = this.targetTexture;
		this.saveIconDummy.SetAppearanceFromSaveData();
		if (this.saveIconDummy.CurrentPet)
		{
			PlayerControl.SetPlayerMaterialColors((int)SaveManager.BodyColor, this.saveIconDummy.CurrentPet.rend);
			PetBehaviour currentPet = this.saveIconDummy.CurrentPet;
			currentPet.enabled = false;
			currentPet.transform.SetParent(this.saveIconDummy.transform);
			currentPet.transform.position = this.saveIconDummy.transform.position + new Vector3(-0.5f, 0f, -5f);
			SpriteRenderer[] componentsInChildren = currentPet.GetComponentsInChildren<SpriteRenderer>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.layer = this.saveIconDummy.gameObject.layer;
			}
		}
		this.saveIconDummy.nameText.GetComponent<MeshRenderer>().material.SetInt("_Mask", 0);
		this.cam.Render();
		this.saveIconDummy.gameObject.SetActive(false);
		RenderTexture.active = this.targetTexture;
		Texture2D texture2D = new Texture2D(this.targetTexture.width, this.targetTexture.height, TextureFormat.RGBA32, false);
		texture2D.ReadPixels(new Rect(0f, 0f, (float)this.targetTexture.width, (float)this.targetTexture.height), 0, 0);
		RenderTexture.active = null;
		return ImageConversion.EncodeToPNG(texture2D);
	}
}
