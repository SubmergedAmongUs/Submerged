using System;
using UnityEngine;

public class KidAccount : MonoBehaviour
{
	[SerializeField]
	private GameObject signInButton;

	[SerializeField]
	private GameObject createAccountButton;

	[SerializeField]
	private GameObject randomizeNameButton;

	[SerializeField]
	private GameObject editNameButton;

	[SerializeField]
	private GameObject requestPermission;

	[SerializeField]
	private GameObject logOutButton;

	public void CanSetCustomName(bool canSetName)
	{
		this.randomizeNameButton.SetActive(!canSetName);
		this.editNameButton.SetActive(canSetName);
	}

	public void CanSignIntoAccount(bool canSignIn)
	{
		this.signInButton.SetActive(canSignIn);
		this.createAccountButton.SetActive(false);
		this.requestPermission.SetActive(!canSignIn);
	}

	public void HasSignedIntoAccount(bool hasSignedIn)
	{
		this.signInButton.SetActive(!hasSignedIn);
		this.logOutButton.SetActive(hasSignedIn);
		this.requestPermission.SetActive(false);
		this.createAccountButton.SetActive(false);
	}
}
