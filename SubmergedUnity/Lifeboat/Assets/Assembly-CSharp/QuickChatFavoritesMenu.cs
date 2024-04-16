using System;
using UnityEngine;

public class QuickChatFavoritesMenu : MonoBehaviour
{
	public PassiveButton ButtonPrefab;

	public Scroller ButtonParent;

	public float ButtonStart = 0.5f;

	public float ButtonHeight = 0.5f;

	private PassiveButton[] AllButtons;

	private TextBoxTMP[] AllTextBoxes;

	private void OnEnable()
	{
		if (this.AllButtons != null)
		{
			foreach (PassiveButton uiElement in this.AllButtons)
			{
				ControllerManager.Instance.AddSelectableUiElement(uiElement, false);
			}
			ControllerManager.Instance.ClearDestroyedSelectableUiElements();
		}
	}

	public void Start()
	{
		Collider2D component = this.ButtonParent.GetComponent<Collider2D>();
		bool flag = this.AllButtons == null;
		Vector3 localPosition = new Vector3(0f, this.ButtonStart, -0.5f);
		this.AllButtons = new PassiveButton[20];
		this.AllTextBoxes = new TextBoxTMP[20];
		for (int i = 0; i < 20; i++)
		{
			PassiveButton passiveButton = UnityEngine.Object.Instantiate<PassiveButton>(this.ButtonPrefab, this.ButtonParent.Inner);
			this.AllButtons[i] = passiveButton;
			passiveButton.ClickMask = component;
			passiveButton.transform.localPosition = localPosition;
			localPosition.y -= this.ButtonHeight;
			TextBoxTMP component2 = passiveButton.GetComponent<TextBoxTMP>();
			string input = (SaveManager.QuickChatFavorites[i] == null) ? "___" : SaveManager.QuickChatFavorites[i];
			component2.SetText(input, "");
			this.AllTextBoxes[i] = component2;
			int tempIndex = i;
			component2.OnFocusLost.AddListener(delegate()
			{
				this.UpdateQuickChatFavorite(tempIndex);
			});
			if (flag)
			{
				ControllerManager.Instance.AddSelectableUiElement(passiveButton, false);
			}
		}
		this.ButtonParent.YBounds.max = 20f * this.ButtonHeight - 2f * this.ButtonStart - 0.1f;
	}

	public void UpdateQuickChatFavorite(int which)
	{
		Debug.LogError("Updating quick chat favorite #" + which.ToString());
		string[] quickChatFavorites = SaveManager.QuickChatFavorites;
		quickChatFavorites[which] = this.AllTextBoxes[which].text;
		SaveManager.QuickChatFavorites = quickChatFavorites;
	}
}
