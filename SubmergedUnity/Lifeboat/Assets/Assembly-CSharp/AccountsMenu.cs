using System;
using UnityEngine;

public class AccountsMenu : MonoBehaviour
{
	public void Open()
	{
		base.gameObject.SetActive(true);
	}

	public void SetValue(string val)
	{
		this.Close();
	}

	public void Close()
	{
		base.gameObject.SetActive(false);
	}
}
