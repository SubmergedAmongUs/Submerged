using System;
using System.Linq;
using PowerTools;
using Rewired;
using TMPro;
using UnityEngine;

public class DrillMinigame : Minigame
{
	public SpriteRenderer CaseImage;

	public TextMeshPro statusText;

	public SpriteAnim[] Buttons;

	public AnimationClip BadAnim;

	public AudioClip ButtonSound;

	private int MaxState = 4;

	private int[] states = new int[4];

	private SpriteAnim prevFixedButton;

	private float changeButtonDelay;

	private int[] drillButtonMaps = new int[]
	{
		20,
		22,
		21,
		24
	};

	public void Start()
	{
		PlayerControl.LocalPlayer.SetPlayerMaterialColors(this.CaseImage);
	}

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		base.SetupInput(true);
		do
		{
			for (int i = 0; i < this.states.Length; i++)
			{
				int num = this.states[i] = 0;
				this.Buttons[i].transform.localScale = Vector3.one * Mathf.Lerp(1f, 0.5f, (float)num / (float)this.MaxState);
				this.Buttons[i].gameObject.SetActive(num != this.MaxState);
			}
		}
		while (this.states.All((int s) => s == this.MaxState));
	}

	private void Update()
	{
		Player player = ReInput.players.GetPlayer(0);
		if (this.prevFixedButton != null && this.changeButtonDelay >= 0f)
		{
			this.changeButtonDelay -= Time.deltaTime;
			if (this.changeButtonDelay <= 0f)
			{
				this.prevFixedButton = null;
			}
		}
		for (int i = 0; i < 4; i++)
		{
			if (player.GetButtonDown(this.drillButtonMaps[i]) && (this.prevFixedButton == null || this.prevFixedButton == this.Buttons[i]))
			{
				this.prevFixedButton = this.Buttons[i];
				this.changeButtonDelay = 0.25f;
				this.FixButton(this.Buttons[i]);
			}
		}
	}

	public void FixButton(SpriteAnim button)
	{
		int num = this.Buttons.IndexOf(button);
		if (this.states[num] == this.MaxState)
		{
			return;
		}
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.ButtonSound, false, 1f);
		}
		int[] array = this.states;
		int num2 = num;
		int num3 = array[num2] + 1;
		array[num2] = num3;
		int num4 = num3;
		this.Buttons[num].transform.localScale = Vector3.one * Mathf.Lerp(1f, 0.5f, (float)num4 / (float)this.MaxState);
		this.Buttons[num].gameObject.SetActive(num4 != this.MaxState);
		if (num4 == this.MaxState)
		{
			this.changeButtonDelay = 0f;
		}
		if (this.states.All((int ss) => ss == this.MaxState))
		{
			this.statusText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Fine, Array.Empty<object>());
			this.statusText.color = Color.green;
			this.MyNormTask.NextStep();
			base.StartCoroutine(base.CoStartClose(0.75f));
		}
	}
}
