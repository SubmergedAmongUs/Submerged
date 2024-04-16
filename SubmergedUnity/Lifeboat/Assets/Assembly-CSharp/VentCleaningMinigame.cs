using System;
using System.Collections;
using System.Collections.Generic;
using Rewired;
using UnityEngine;
using Random = UnityEngine.Random;

public class VentCleaningMinigame : Minigame
{
	public FloatRange XSpan = new FloatRange(-1.15f, 1.15f);

	public FloatRange YSpan = new FloatRange(-1.15f, 1.15f);

	public ObjectPoolBehavior dirtPool;

	public GameObject ventLidClosed;

	public GameObject ventLidOpened;

	public UiElement backButton;

	public AudioClip VentOpenSound;

	public AudioClip ImpostorDiscoveredSound;

	public AudioClip CleanedSound;

	public SpriteRenderer[] SpiderWebs;

	public Transform selectorObject;

	public SpriteRenderer selectorHand;

	private int numberOfDirts;

	private int numberOfDirtsCleanedUp;

	private bool ventOpen;

	private VentilationSystem ventSystem;

	private int VentId
	{
		get
		{
			return base.ConsoleId;
		}
	}

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		this.ventOpen = false;
		this.ventLidClosed.SetActive(true);
		this.ventLidOpened.SetActive(false);
		this.numberOfDirts = (int)this.MyNormTask.Data[this.MyNormTask.MaxStep];
		this.numberOfDirtsCleanedUp = 0;
		this.ventSystem = (ShipStatus.Instance.Systems[SystemTypes.Ventilation] as VentilationSystem);
		for (int i = 0; i < this.numberOfDirts; i++)
		{
			this.SpawnDirt();
		}
		this.SpiderWebs.ForEach(delegate(SpriteRenderer s)
		{
			s.enabled = false;
		});
		this.SpiderWebs.RandomSet(2).ForEach(delegate(SpriteRenderer s)
		{
			s.enabled = true;
		});
		if (!PlayerControl.LocalPlayer.Data.IsDead)
		{
			VentilationSystem.Update(VentilationSystem.Operation.StartCleaning, this.VentId);
		}
		this.selectorObject.gameObject.SetActive(false);
		PlayerControl.LocalPlayer.SetPlayerMaterialColors(this.selectorHand);
		base.SetupInput(false);
	}

	public override void Close()
	{
		if (this.amClosing != Minigame.CloseState.Closing)
		{
			VentilationSystem.Update(VentilationSystem.Operation.StopCleaning, this.VentId);
		}
		base.Close();
	}

	public void OpenVent()
	{
		if (this.ventOpen)
		{
			return;
		}
		base.StartCoroutine(this.CoOpenVent());
	}

	private void FixedUpdate()
	{
		if (this.amOpening || this.amClosing != Minigame.CloseState.None)
		{
			return;
		}
		if (!PlayerControl.LocalPlayer.Data.IsDead && this.ventSystem != null && this.ventSystem.IsImpostorInsideVent(this.VentId))
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(this.ImpostorDiscoveredSound, false, 0.8f);
			}
			VentilationSystem.Update(VentilationSystem.Operation.BootImpostors, this.VentId);
			this.Close();
			return;
		}
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			this.HandleJoystick();
			return;
		}
		if (this.selectorObject.gameObject.activeSelf)
		{
			this.selectorObject.gameObject.SetActive(false);
		}
	}

	private void HandleJoystick()
	{
		Player player = ReInput.players.GetPlayer(0);
		bool button = player.GetButton(11);
		if (this.ventOpen && !this.selectorObject.gameObject.activeSelf)
		{
			this.selectorObject.gameObject.SetActive(true);
		}
		if (this.ventOpen)
		{
			Vector3 position = this.selectorObject.position;
			position.x = VirtualCursor.currentPosition.x;
			position.y = VirtualCursor.currentPosition.y;
			this.selectorObject.position = position;
			if (player.GetButtonUp(11))
			{
				float num = 0f;
				int num2 = -1;
				List<PoolableBehavior> activeChildren = this.dirtPool.activeChildren;
				for (int i = 0; i < activeChildren.Count; i++)
				{
					float sqrMagnitude = (activeChildren[i].transform.localPosition - this.selectorObject.localPosition).sqrMagnitude;
					if (sqrMagnitude <= 2f && (num2 == -1 || sqrMagnitude < num))
					{
						num = sqrMagnitude;
						num2 = i;
					}
				}
				if (num2 != -1)
				{
					PoolableBehavior poolableBehavior = activeChildren[num2];
					poolableBehavior.StopAllCoroutines();
					this.CleanUp(poolableBehavior.GetComponent<VentDirt>());
					return;
				}
			}
		}
		else if (button)
		{
			this.OpenVent();
		}
	}

	private IEnumerator CoOpenVent()
	{
		SoundManager.Instance.PlaySound(this.VentOpenSound, false, 1f);
		yield return Effects.Lerp(0.2f, delegate(float t)
		{
			this.ventLidClosed.transform.localScale = new Vector3(1f, Mathf.Lerp(1f, -0.5f, t), 1f);
			this.ventLidClosed.transform.localPosition = new Vector3(0f, Mathf.Lerp(0f, this.ventLidOpened.transform.localPosition.y, t), 3f);
		});
		this.ventLidClosed.SetActive(false);
		this.ventLidOpened.SetActive(true);
		this.ventOpen = true;
		yield break;
	}

	private void CleanUp(VentDirt ventDirt)
	{
		if (!this.ventOpen)
		{
			return;
		}
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.CleanedSound, false, 1f).pitch = FloatRange.Next(0.8f, 1.2f);
		}
		if (!this.MyNormTask.IsComplete)
		{
			base.StartCoroutine(ventDirt.CoDisappear());
			this.numberOfDirtsCleanedUp++;
			if (this.numberOfDirtsCleanedUp >= this.numberOfDirts)
			{
				this.MyNormTask.NextStep();
				base.StartCoroutine(base.CoStartClose(0.75f));
				foreach (PoolableBehavior poolableBehavior in this.dirtPool.activeChildren)
				{
					VentDirt ventDirt2 = (VentDirt)poolableBehavior;
					if (!(ventDirt2 == ventDirt))
					{
						base.StartCoroutine(ventDirt2.CoDisappear());
					}
				}
			}
		}
	}

	private void SpawnDirt()
	{
		VentDirt dirt = this.dirtPool.Get<VentDirt>();
		dirt.transform.localPosition = new Vector3(this.XSpan.Next(), this.YSpan.Next(), 4f);
		dirt.transform.localEulerAngles = new Vector3(0f, 0f, (float) Random.Range(0, 360));
		dirt.GetComponent<ButtonBehavior>().OnClick.AddListener(delegate()
		{
			this.CleanUp(dirt);
		});
	}
}
