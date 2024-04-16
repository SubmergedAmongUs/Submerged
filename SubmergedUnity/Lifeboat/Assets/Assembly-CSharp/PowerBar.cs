using System;
using UnityEngine;

public class PowerBar : MonoBehaviour
{
	public SpriteRenderer SegmentPrefab;

	public Sprite greenImage;

	public Sprite yellowImage;

	public Sprite redImage;

	public Sprite greenEmptyImage;

	public Sprite yellowEmptyImage;

	public Sprite redEmptyImage;

	public int numberGreen = 11;

	public int numberYellow = 6;

	public int numberRed = 3;

	public float Width;

	private float value = 0.5f;

	private SpriteRenderer[] Segments;

	public int NumSegments
	{
		get
		{
			return this.numberGreen + this.numberRed + this.numberYellow;
		}
	}

	public void Awake()
	{
		this.Segments = new SpriteRenderer[this.NumSegments];
		float num = this.Width / (float)(this.NumSegments - 1);
		float num2 = this.Width / -2f;
		for (int i = 0; i < this.Segments.Length; i++)
		{
			SpriteRenderer spriteRenderer = this.Segments[i] = UnityEngine.Object.Instantiate<SpriteRenderer>(this.SegmentPrefab, base.transform);
			spriteRenderer.transform.localPosition = new Vector3(num2 + (float)i * num, 0f, 0.0001f);
			if (i < this.numberGreen)
			{
				spriteRenderer.sprite = this.greenImage;
			}
			else if (i < this.numberGreen + this.numberYellow)
			{
				spriteRenderer.sprite = this.yellowImage;
			}
			else
			{
				spriteRenderer.sprite = this.redImage;
			}
		}
	}

	public void SetValue(float value)
	{
		this.value = value;
		this.Update();
	}

	public void Update()
	{
		for (int i = 0; i < this.Segments.Length; i++)
		{
			SpriteRenderer spriteRenderer = this.Segments[i];
			if ((float)i < this.value * (float)this.NumSegments)
			{
				if (i < this.numberGreen)
				{
					spriteRenderer.sprite = this.greenImage;
				}
				else if (i < this.numberGreen + this.numberYellow)
				{
					spriteRenderer.sprite = this.yellowImage;
				}
				else
				{
					spriteRenderer.sprite = this.redImage;
				}
				spriteRenderer.color = Color.white;
			}
			else
			{
				if (i < this.numberGreen)
				{
					spriteRenderer.sprite = this.greenEmptyImage;
				}
				else if (i < this.numberGreen + this.numberYellow)
				{
					spriteRenderer.sprite = this.yellowEmptyImage;
				}
				else
				{
					spriteRenderer.sprite = this.redEmptyImage;
				}
				spriteRenderer.color = Color.gray;
			}
		}
	}
}
