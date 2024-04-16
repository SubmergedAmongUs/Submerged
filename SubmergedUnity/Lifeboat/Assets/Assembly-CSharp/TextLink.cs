using System;
using UnityEngine;

public class TextLink : MonoBehaviour
{
	public BoxCollider2D boxCollider;

	public string targetUrl;

	public bool needed;

	public void Set(Vector2 from, Vector2 to, string target)
	{
		this.targetUrl = target;
		Vector2 vector = to + from;
		base.transform.localPosition = new Vector3(vector.x / 2f, vector.y / 2f, -1f);
		vector = to - from;
		vector.y = -vector.y;
		this.boxCollider.size = vector;
	}

	public void Click()
	{
		if (this.targetUrl == "httpstore")
		{
			DestroyableSingleton<StoreMenu>.Instance.Open();
			return;
		}
		Application.OpenURL(this.targetUrl);
	}
}
