using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TextController : MonoBehaviour
{
	public float Scale = 1f;

	[Multiline]
	public string Text;

	private string displaying;

	[HideInInspector]
	private Texture2D texture;

	[HideInInspector]
	private Texture2D colorTexture;

	private MeshRenderer rend;

	private float _scale = float.NegativeInfinity;

	public Color Color = Color.white;

	private Color lastColor;

	public Vector3 Offset;

	public bool topAligned;

	public void Update()
	{
		if (!this.rend)
		{
			this.rend = base.GetComponent<MeshRenderer>();
		}
		if (string.IsNullOrEmpty(this.Text))
		{
			this.rend.enabled = false;
			return;
		}
		if (this.displaying == null || this.displaying.GetHashCode() != this.Text.GetHashCode() || this.Color != this.lastColor)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 1;
			for (int i = 0; i < this.Text.Length; i++)
			{
				if (this.Text[i] == '\n')
				{
					num2 = 0;
					num3++;
				}
				else
				{
					num2++;
					if (num2 > num)
					{
						num = num2;
					}
				}
			}
			if (!this.texture || !this.colorTexture)
			{
				if (!this.texture)
				{
					this.texture = new Texture2D(num, num3, (TextureFormat) 5, false);
					this.texture.filterMode = 0;
					this.texture.wrapMode = (TextureWrapMode) 1;
				}
				if (!this.colorTexture)
				{
					this.colorTexture = new Texture2D(num, num3, (TextureFormat) 5, false);
					this.colorTexture.filterMode = 0;
					this.colorTexture.wrapMode = (TextureWrapMode) 1;
				}
			}
			else if (this.texture.width != num || this.texture.height != num3)
			{
				this.texture.Resize(num, num3, (TextureFormat) 5, false);
				this.colorTexture.Resize(num, num3, (TextureFormat) 5, false);
			}
			Color[] array = new Color[num * num3];
			array.SetAll(this.Color);
			this.colorTexture.SetPixels(array);
			array.SetAll(new Color(0.125f, 0f, 0f));
			this.texture.SetPixels(array);
			int num4 = 0;
			int num5 = this.texture.height - 1;
			Color color = this.Color;
			for (int j = 0; j < this.Text.Length; j++)
			{
				char c = this.Text[j];
				if (c != '\r')
				{
					if (c == '\n')
					{
						num4 = 0;
						num5--;
					}
					else
					{
						this.texture.SetPixel(num4, num5, new Color((float)c / 256f, 0f, 0f));
						this.colorTexture.SetPixel(num4, num5, color);
						num4++;
					}
				}
			}
			this.texture.Apply(false, false);
			this.colorTexture.Apply(false, false);
			this.rend.enabled = true;
			this.rend.material.SetTexture("_InputTex", this.texture);
			this.rend.material.SetTexture("_ColorTex", this.colorTexture);
			this._scale = float.NegativeInfinity;
			this.displaying = this.Text;
			this.lastColor = this.Color;
		}
		if (this._scale != this.Scale)
		{
			this._scale = this.Scale;
			base.transform.localScale = new Vector3((float)this.texture.width, (float)this.texture.height, 1f) * this.Scale;
			if (this.topAligned)
			{
				base.transform.localPosition = this.Offset + new Vector3((float)this.texture.width * this.Scale / 2f, (float)(-(float)this.texture.height) * this.Scale / 2f, 0f);
			}
		}
	}
}
