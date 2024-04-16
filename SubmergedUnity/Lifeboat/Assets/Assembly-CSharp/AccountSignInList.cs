using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class AccountSignInList : MonoBehaviour
{
	public AccountButton ButtonPrefab;

	public Scroller ButtonParent;

	public float ButtonStart = 0.5f;

	public float ButtonHeight = 0.5f;

	private AccountButton[] AllButtons;

	public AccountsMenu parent;

	public ControllerNavMenu controllerNavParent;

	public bool createAccount;

	public void Start()
	{
		Collider2D component = this.ButtonParent.GetComponent<Collider2D>();
		string[] array = new string[]
		{
			"Steam",
			"Epic",
			"Apple",
			"Google"
		};
		Vector3 localPosition = new Vector3(0f, this.ButtonStart, -0.5f);
		this.AllButtons = new AccountButton[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			AccountButton button = UnityEngine.Object.Instantiate<AccountButton>(this.ButtonPrefab, this.ButtonParent.Inner);
			this.AllButtons[i] = button;
			button.Text.text = array[i];
			if (!this.createAccount)
			{
				button.Button.OnClick.AddListener(delegate()
				{
					this.LogInWith(button);
				});
			}
			button.Button.ClickMask = component;
			button.transform.localPosition = localPosition;
			localPosition.y -= this.ButtonHeight;
			this.controllerNavParent.ControllerSelectable.Add(button.Button);
		}
		if (array.Length != 0)
		{
			this.controllerNavParent.DefaultButtonSelected = this.AllButtons[0].Button;
		}
		this.ButtonParent.YBounds.max = (float)array.Length * this.ButtonHeight - 2f * this.ButtonStart - 0.1f;
	}

	public void LogInWith(AccountButton selected)
	{
	}
}
