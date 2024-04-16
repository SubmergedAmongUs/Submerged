using System;
using System.Collections;
using System.Linq;
using Rewired;
using TMPro;
using UnityEngine;

public class WaterPlantsGame : Minigame
{
	public GameObject stage1;

	public GameObject stage2;

	public AudioClip CanGrabSound;

	public PassiveButton WaterCan;

	public SpriteRenderer[] Plants;

	public AudioClip WaterPlantSound;

	public AudioClip[] PlantGrowSounds;

	public AudioClip[] PlantFinishedSounds;

	public TextMeshPro FloatText;

	public Transform[] Locations;

	public Transform selectorObject;

	public GameObject grabCanSubObject;

	public GameObject holdingCanSubObject;

	public GameObject waterPlantsSubObject;

	private Controller c = new Controller();

	public SpriteRenderer[] playerHandObjects;

	public ParticleSystem waterParticles;

	private bool Watered(int x)
	{
		return this.MyNormTask.Data[x] > 0;
	}

	private void Watered(int x, bool b)
	{
		this.MyNormTask.Data[x] = (byte) (b ? 1 : 0);
	}

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		if (this.MyNormTask.taskStep == 0)
		{
			this.WaterCan.transform.localPosition = this.Locations.Random<Transform>().localPosition;
			this.WaterCan.GetComponent<SpriteRenderer>().flipX = BoolRange.Next(0.5f);
			this.stage1.gameObject.SetActive(true);
			this.stage2.gameObject.SetActive(false);
			base.SetupInput(false);
			this.grabCanSubObject.SetActive(true);
			this.holdingCanSubObject.SetActive(false);
			this.waterPlantsSubObject.SetActive(false);
			foreach (SpriteRenderer playerMaterialColors in this.playerHandObjects)
			{
				PlayerControl.LocalPlayer.SetPlayerMaterialColors(playerMaterialColors);
			}
			return;
		}
		if (this.MyNormTask.taskStep == 1)
		{
			this.stage1.gameObject.SetActive(false);
			this.stage2.gameObject.SetActive(true);
			for (int j = 0; j < this.Plants.Length; j++)
			{
				if (this.Watered(j))
				{
					SpriteRenderer spriteRenderer = this.Plants[j];
					spriteRenderer.material.SetFloat("_Desat", 0f);
					spriteRenderer.transform.localScale = Vector3.one;
				}
			}
			base.SetupInput(false);
			this.grabCanSubObject.SetActive(false);
			this.holdingCanSubObject.SetActive(false);
			this.waterPlantsSubObject.SetActive(true);
			foreach (SpriteRenderer playerMaterialColors2 in this.playerHandObjects)
			{
				PlayerControl.LocalPlayer.SetPlayerMaterialColors(playerMaterialColors2);
			}
		}
	}

	private void Update()
	{
		this.c.Update();
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			ReInput.players.GetPlayer(0).GetButton(11);
			Vector3 position = this.selectorObject.transform.position;
			position.x = VirtualCursor.currentPosition.x;
			position.y = VirtualCursor.currentPosition.y;
			this.selectorObject.transform.position = position;
		}
	}

	public void PickWaterCan()
	{
		this.grabCanSubObject.SetActive(false);
		this.holdingCanSubObject.SetActive(true);
		this.WaterCan.enabled = false;
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.CanGrabSound, false, 1f);
		}
		this.MyNormTask.NextStep();
		base.StartCoroutine(this.CoPickWaterCan());
	}

	private IEnumerator CoPickWaterCan()
	{
		this.FloatText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.WaterPlantsGetCan, Array.Empty<object>());
		this.FloatText.gameObject.SetActive(true);
		yield return Effects.All(new IEnumerator[]
		{
			Effects.ColorFade(this.WaterCan.GetComponent<SpriteRenderer>(), Color.white, Palette.ClearWhite, 0.25f),
			Effects.Slide2D(this.FloatText.transform, this.WaterCan.transform.localPosition + new Vector3(0f, 0.1f, 0f), this.WaterCan.transform.localPosition + new Vector3(0f, 0.5f, 0f), 0.75f),
			Effects.ColorFade(this.FloatText, Color.white, Palette.ClearWhite, 0.75f)
		});
		yield return base.CoStartClose(0.75f);
		yield break;
	}

	public void WaterPlant(int num)
	{
		if (this.Watered(num))
		{
			return;
		}
		this.Watered(num, true);
		if (Enumerable.Range(0, 4).All(new Func<int, bool>(this.Watered)))
		{
			this.MyNormTask.NextStep();
			base.StartCoroutine(base.CoStartClose(0.75f));
		}
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.WaterPlantSound, false, 1f);
		}
		base.StartCoroutine(this.CoGrowPlant(num));
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			this.waterParticles.Play();
		}
	}

	private IEnumerator CoGrowPlant(int num)
	{
		SpriteRenderer plant = this.Plants[num];
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.PlantGrowSounds.Random<AudioClip>(), false, 1f).pitch = FloatRange.Next(0.9f, 1.1f);
		}
		for (float timer = 0f; timer < 1f; timer += Time.deltaTime)
		{
			float num2 = timer / 1f;
			plant.material.SetFloat("_Desat", (1f - num2) * 0.8f);
			plant.transform.localScale = new Vector3(0.8f, Mathf.Lerp(0.8f, 1.1f, num2), 1f);
			yield return null;
		}
		plant.material.SetFloat("_Desat", 0f);
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.PlantFinishedSounds.Random<AudioClip>(), false, 1f).pitch = FloatRange.Next(0.9f, 1.1f);
		}
		for (float timer = 0f; timer < 0.1f; timer += Time.deltaTime)
		{
			float num3 = timer / 0.1f;
			plant.transform.localScale = new Vector3(Mathf.Lerp(0.8f, 1.1f, num3), Mathf.Lerp(1.1f, 0.95f, num3), 1f);
			yield return null;
		}
		for (float timer = 0f; timer < 0.1f; timer += Time.deltaTime)
		{
			float num4 = timer / 0.1f;
			plant.transform.localScale = new Vector3(Mathf.Lerp(1.1f, 1f, num4), Mathf.Lerp(0.95f, 1f, num4), 1f);
			yield return null;
		}
		yield break;
	}
}
