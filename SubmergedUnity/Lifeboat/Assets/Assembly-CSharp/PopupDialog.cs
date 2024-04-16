using System;
using TMPro;
using UnityEngine;

public class PopupDialog : MonoBehaviour
{
	public TextMeshPro workingText;

	public float secondsBetweenDots = 0.7f;

	public int maxDots = 3;

	public string currentProgressText = "";

	private float textUpdateTimer;

	private static PopupDialog instance;

	public static void Display()
	{
		if (!PopupDialog.instance)
		{
			PopupDialog.instance = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("WaitForConnectionDialog")).GetComponent<PopupDialog>();
		}
	}

	public static void Dispose()
	{
		if (PopupDialog.instance)
		{
			 UnityEngine.Object.Destroy(PopupDialog.instance);
		}
	}

	private void Start()
	{
		ControllerManager.Instance.enabled = false;
	}

	private void OnDestroy()
	{
		ControllerManager.Instance.enabled = true;
	}

	public void Update()
	{
		this.textUpdateTimer -= Time.unscaledDeltaTime;
		if (this.textUpdateTimer <= 0f)
		{
			this.textUpdateTimer += this.secondsBetweenDots;
			if (this.currentProgressText.Length == this.maxDots)
			{
				this.currentProgressText = "";
			}
			else
			{
				this.currentProgressText += ".";
			}
			this.workingText.text = this.currentProgressText;
		}
	}
}
