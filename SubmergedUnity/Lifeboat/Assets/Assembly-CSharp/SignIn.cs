using System;
using UnityEngine;

public class SignIn : MonoBehaviour
{
	[SerializeField]
	private GameObject mainScreen;

	[SerializeField]
	private GameObject signInScreen;

	[SerializeField]
	private GameObject createAccountScreen;

	public void Close()
	{
		base.GetComponent<TransitionOpen>().Close();
	}

	public void Open()
	{
		base.GetComponent<TransitionOpen>().OnEnable();
	}
}
