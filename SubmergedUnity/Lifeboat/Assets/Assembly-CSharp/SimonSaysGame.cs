using System;
using System.Collections;
using System.Collections.Generic;
using Rewired;
using UnityEngine;

public class SimonSaysGame : Minigame
{
	private Queue<int> operations = new Queue<int>();

	private const int FlashOp = 256;

	private const int AnimateOp = 128;

	private const int ReAnimateOp = 32;

	private const int FailOp = 64;

	private Color gray = new Color32(141, 141, 141, byte.MaxValue);

	private Color blue = new Color32(68, 168, byte.MaxValue, byte.MaxValue);

	private Color red = new Color32(byte.MaxValue, 58, 0, byte.MaxValue);

	private Color green = Color.green;

	public SpriteRenderer[] LeftSide;

	public SpriteRenderer[] Buttons;

	public SpriteRenderer[] LeftLights;

	public SpriteRenderer[] RightLights;

	private float flashTime = 0.25f;

	private float userButtonFlashTime = 0.175f;

	public AudioClip ButtonPressSound;

	public AudioClip FailSound;

	public Transform selectorHighlightObject;

	public float diagonalRoundingWidth = 0.6f;

	public float inputAngleIndex = -1f;

	public int roundDownIndex;

	public int roundUpIndex;

	private int[] orderedButtonIndices = new int[]
	{
		5,
		2,
		1,
		0,
		3,
		6,
		7,
		8
	};

	private int IndexCount
	{
		get
		{
			return (int)this.MyNormTask.Data[0];
		}
		set
		{
			this.MyNormTask.Data[0] = (byte)value;
		}
	}

	private byte this[int idx]
	{
		get
		{
			return this.MyNormTask.Data[idx + 1];
		}
		set
		{
			this.MyNormTask.Data[idx + 1] = value;
		}
	}

	public override void Begin(PlayerTask task)
	{
		for (int i = 0; i < this.LeftSide.Length; i++)
		{
			this.LeftSide[i].color = Color.clear;
		}
		base.Begin(task);
		if (this.IndexCount == 0)
		{
			this.operations.Enqueue(128);
		}
		else
		{
			this.operations.Enqueue(32);
		}
		base.SetupInput(true);
		base.StartCoroutine(this.CoRun());
	}

	public void HitButton(int bIdx)
	{
		if (this.MyNormTask.IsComplete || this.MyNormTask.taskStep >= this.IndexCount)
		{
			return;
		}
		if ((int)this[this.MyNormTask.taskStep] == bIdx)
		{
			this.MyNormTask.NextStep();
			this.SetLights(this.RightLights, this.MyNormTask.taskStep);
			if (this.MyNormTask.IsComplete)
			{
				this.SetLights(this.LeftLights, this.LeftLights.Length);
				for (int i = 0; i < this.Buttons.Length; i++)
				{
					SpriteRenderer spriteRenderer = this.Buttons[i];
					spriteRenderer.color = this.gray;
					base.StartCoroutine(this.FlashButton(-1, spriteRenderer, this.flashTime));
				}
				base.StartCoroutine(base.CoStartClose(0.75f));
				return;
			}
			this.operations.Enqueue(256 | bIdx);
			if (this.MyNormTask.taskStep >= this.IndexCount)
			{
				this.operations.Enqueue(128);
				return;
			}
		}
		else
		{
			this.IndexCount = 0;
			this.operations.Enqueue(64);
			this.operations.Enqueue(128);
		}
	}

	private IEnumerator CoRun()
	{
		for (;;)
		{
			if (this.operations.Count <= 0)
			{
				Player player = ReInput.players.GetPlayer(0);
				Vector2 normalized = new Vector2(player.GetAxis(13), player.GetAxis(14));
				int num = 4;
				if (normalized.sqrMagnitude > 0.7f)
				{
					normalized = normalized.normalized;
					float num2 = Mathf.Atan2(normalized.y, normalized.x) * 57.29578f;
					if (num2 < 0f)
					{
						num2 += 360f;
					}
					num2 *= 0.0027777778f;
					this.inputAngleIndex = num2 * 8f;
					float num3 = this.inputAngleIndex - this.diagonalRoundingWidth;
					if (num3 < 0f)
					{
						num3 = 8f + num3;
					}
					float num4 = this.inputAngleIndex - Mathf.Floor(this.inputAngleIndex);
					if ((int)this.inputAngleIndex % 2 == 1)
					{
						if (num4 < this.diagonalRoundingWidth)
						{
							num = (int)this.inputAngleIndex;
						}
						else
						{
							num = (int)(this.inputAngleIndex + 0.5f);
						}
					}
					else if (num4 < 1f - this.diagonalRoundingWidth)
					{
						num = (int)this.inputAngleIndex;
					}
					else
					{
						num = (int)this.inputAngleIndex + 1;
					}
					this.roundUpIndex = Mathf.FloorToInt(this.inputAngleIndex + this.diagonalRoundingWidth);
					this.roundDownIndex = Mathf.FloorToInt(this.inputAngleIndex - this.diagonalRoundingWidth);
					num %= 8;
					num = this.orderedButtonIndices[num];
				}
				if (player.GetButtonDown(11))
				{
					this.HitButton(num);
					this.selectorHighlightObject.transform.localPosition = new Vector3(5000f, 5000f, this.selectorHighlightObject.transform.localPosition.z);
				}
				else
				{
					Vector3 localPosition = this.Buttons[num].transform.localPosition;
					localPosition.z = this.selectorHighlightObject.transform.localPosition.z;
					this.selectorHighlightObject.transform.localPosition = localPosition;
				}
				yield return null;
			}
			else
			{
				int num5 = this.operations.Dequeue();
				if (num5.HasAnyBit(256))
				{
					int num6 = num5 & -257;
					yield return this.FlashButton(num6, this.Buttons[num6], this.userButtonFlashTime);
				}
				else if (num5.HasAnyBit(128))
				{
					yield return this.CoAnimateNewLeftSide();
				}
				else if (num5.HasAnyBit(32))
				{
					yield return this.CoAnimateOldLeftSide();
				}
				else if (num5.HasAnyBit(64))
				{
					if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(this.FailSound, false, 1f);
					}
					this.SetAllColor(this.red);
					yield return new WaitForSeconds(this.flashTime);
					this.SetAllColor(Color.white);
					yield return new WaitForSeconds(this.flashTime);
					this.SetAllColor(this.red);
					yield return new WaitForSeconds(this.flashTime);
					this.SetAllColor(Color.white);
					yield return new WaitForSeconds(this.flashTime / 2f);
				}
			}
		}
		yield break;
	}

	private void AddIndex(int idxToAdd)
	{
		this[this.IndexCount] = (byte)idxToAdd;
		int indexCount = this.IndexCount;
		this.IndexCount = indexCount + 1;
	}

	private IEnumerator CoAnimateNewLeftSide()
	{
		this.SetLights(this.RightLights, 0);
		for (int i = 0; i < this.Buttons.Length; i++)
		{
			this.Buttons[i].color = this.gray;
		}
		this.AddIndex(this.Buttons.RandomIdx<SpriteRenderer>());
		yield return this.CoAnimateOldLeftSide();
		yield break;
	}

	private IEnumerator CoAnimateOldLeftSide()
	{
		yield return new WaitForSeconds(1f);
		this.SetLights(this.LeftLights, this.IndexCount);
		int num2;
		for (int i = 0; i < this.IndexCount; i = num2)
		{
			int num = (int)this[i];
			yield return this.FlashButton(num, this.LeftSide[num], this.flashTime);
			yield return new WaitForSeconds(0.1f);
			num2 = i + 1;
		}
		this.MyNormTask.taskStep = 0;
		for (int j = 0; j < this.Buttons.Length; j++)
		{
			this.Buttons[j].color = Color.white;
		}
		yield break;
	}

	private IEnumerator FlashButton(int id, SpriteRenderer butt, float flashTime)
	{
		if (id > -1 && Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.ButtonPressSound, false, 1f).pitch = Mathf.Lerp(0.5f, 1.5f, (float)id / 9f);
		}
		Color c = butt.color;
		butt.color = this.blue;
		yield return new WaitForSeconds(flashTime);
		butt.color = c;
		yield break;
	}

	private void SetLights(SpriteRenderer[] lights, int num)
	{
		for (int i = 0; i < lights.Length; i++)
		{
			if (i < num)
			{
				lights[i].color = this.green;
			}
			else
			{
				lights[i].color = this.gray;
			}
		}
	}

	private void SetAllColor(Color color)
	{
		for (int i = 0; i < this.Buttons.Length; i++)
		{
			this.Buttons[i].color = color;
		}
		for (int j = 0; j < this.RightLights.Length; j++)
		{
			this.RightLights[j].color = color;
		}
	}

	private void SetButtonColor(int i, Color color)
	{
		this.Buttons[i].color = color;
	}
}
