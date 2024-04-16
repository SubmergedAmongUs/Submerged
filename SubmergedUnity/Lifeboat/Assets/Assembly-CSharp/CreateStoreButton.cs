using System;
using UnityEngine;

public class CreateStoreButton : MonoBehaviour
{
	public GameObject ConnectIcon;

	public void Click()
	{
		DestroyableSingleton<StoreMenu>.Instance.Open();
	}
}
