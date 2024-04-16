using System;
using System.Text;
using TMPro;
using UnityEngine;

public class NotificationPopper : MonoBehaviour
{
	public TextMeshPro TextArea;

	public float zPos = -350f;

	private float alphaTimer;

	public float ShowDuration = 5f;

	public float FadeDuration = 1f;

	public Color textColor = Color.white;

	private StringBuilder builder = new StringBuilder();

	public AudioClip NotificationSound;

	public void Update()
	{
		if (this.alphaTimer > 0f)
		{
			float num = Camera.main.orthographicSize * Camera.main.aspect;
			if (!DestroyableSingleton<HudManager>.Instance.TaskText.isActiveAndEnabled)
			{
				float y = DestroyableSingleton<HudManager>.Instance.GameSettings.bounds.size.y;
				Transform transform = DestroyableSingleton<HudManager>.Instance.GameSettings.transform;
				base.transform.localPosition = new Vector3(-num + 0.1f, transform.localPosition.y - y, this.zPos);
			}
			else
			{
				float y2 = DestroyableSingleton<HudManager>.Instance.TaskText.textBounds.size.y;
				Transform parent = DestroyableSingleton<HudManager>.Instance.TaskText.transform.parent;
				base.transform.localPosition = new Vector3(-num + 0.1f, parent.localPosition.y - y2 - 0.2f, this.zPos);
			}
			this.alphaTimer -= Time.deltaTime;
			this.textColor.a = Mathf.Clamp(this.alphaTimer / this.FadeDuration, 0f, 1f);
			this.TextArea.color = this.textColor;
			if (this.alphaTimer <= 0f)
			{
				this.builder.Clear();
				this.TextArea.text = string.Empty;
			}
		}
	}

	public void AddItem(string item)
	{
		this.builder.AppendLine(item);
		this.TextArea.text = this.builder.ToString();
		this.alphaTimer = this.ShowDuration;
		SoundManager.Instance.PlaySound(this.NotificationSound, false, 1f);
	}
}
