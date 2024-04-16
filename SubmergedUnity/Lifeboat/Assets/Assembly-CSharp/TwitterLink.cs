using System;
using UnityEngine;

public class TwitterLink : MonoBehaviour
{
	public string LinkUrl = "https://www.twitter.com/InnerslothDevs";

	public void Click()
	{
		Application.OpenURL(this.LinkUrl);
	}
}
