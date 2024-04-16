using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PowerTools;
using TMPro;
using UnityEngine;

public class SpawnInMinigame : Minigame
{
	public SpawnInMinigame.SpawnLocation[] Locations;

	public PassiveButton[] LocationButtons;

	public TextMeshPro Text;

	public AudioClip DefaultRolloverSound;

	[Header("Console Controller Navigation")]
	public UiElement DefaultButtonSelected;

	public List<UiElement> ControllerSelectable;

	private bool gotButton;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		SpawnInMinigame.SpawnLocation[] array = this.Locations.ToArray<SpawnInMinigame.SpawnLocation>();
		array.Shuffle(0);
		array = (from s in array.Take(this.LocationButtons.Length)
		orderby s.Location.x, s.Location.y descending
		select s).ToArray<SpawnInMinigame.SpawnLocation>();
		for (int i = 0; i < this.LocationButtons.Length; i++)
		{
			PassiveButton passiveButton = this.LocationButtons[i];
			SpawnInMinigame.SpawnLocation pt = array[i];
			passiveButton.OnClick.AddListener(delegate()
			{
				this.SpawnAt(pt.Location);
			});
			passiveButton.GetComponent<SpriteAnim>().Stop();
			passiveButton.GetComponent<SpriteRenderer>().sprite = pt.Image;
			passiveButton.GetComponentInChildren<TextMeshPro>().text = DestroyableSingleton<TranslationController>.Instance.GetString(pt.Name, Array.Empty<object>());
			ButtonAnimRolloverHandler component = passiveButton.GetComponent<ButtonAnimRolloverHandler>();
			component.StaticOutImage = pt.Image;
			component.RolloverAnim = pt.Rollover;
			component.HoverSound = (pt.RolloverSfx ? pt.RolloverSfx : this.DefaultRolloverSound);
		}
		PlayerControl.LocalPlayer.gameObject.SetActive(false);
		PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(new Vector2(-25f, 40f));
		base.StartCoroutine(this.RunTimer());
		ControllerManager.Instance.OpenOverlayMenu(base.name, null, this.DefaultButtonSelected, this.ControllerSelectable, false);
		PlayerControl.HideCursorTemporarily();
		ConsoleJoystick.SetMode_Menu();
	}

	public override void Close()
	{
		ControllerManager.Instance.CloseOverlayMenu(base.name);
		if (!this.gotButton)
		{
			this.LocationButtons.Random<PassiveButton>().ReceiveClickUp();
		}
		base.Close();
	}

	private IEnumerator RunTimer()
	{
		for (float time = 10f; time >= 0f; time -= Time.deltaTime)
		{
			this.Text.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.TimeRemaining, new object[]
			{
				Mathf.CeilToInt(time)
			});
			yield return null;
		}
		this.LocationButtons.Random<PassiveButton>().ReceiveClickUp();
		yield break;
	}

	private void SpawnAt(Vector3 spawnAt)
	{
		if (this.amClosing != Minigame.CloseState.None)
		{
			return;
		}
		this.gotButton = true;
		PlayerControl.LocalPlayer.gameObject.SetActive(true);
		base.StopAllCoroutines();
		PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(spawnAt);
		DestroyableSingleton<HudManager>.Instance.PlayerCam.SnapToTarget();
		this.Close();
	}

	public IEnumerator WaitForFinish()
	{
		yield return null;
		while (this.amClosing == Minigame.CloseState.None)
		{
			yield return null;
		}
		yield break;
	}

	[Serializable]
	public struct SpawnLocation
	{
		public StringNames Name;

		public Sprite Image;

		public AnimationClip Rollover;

		public AudioClip RolloverSfx;

		public Vector3 Location;
	}
}
