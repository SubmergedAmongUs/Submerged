using System;
using UnityEngine;
using UnityEngine.Analytics;

public class ManageDataCollectionButton : MonoBehaviour
{
	public GenericPopup PopupPrefab;

	public void ManageData()
	{
		//DataPrivacy.FetchPrivacyUrl(new Action<string>(Application.OpenURL), new Action<string>(this.ShowPopup));
	}

	private void ShowPopup(string error)
	{
		if (this.PopupPrefab)
		{
			GenericPopup genericPopup = UnityEngine.Object.Instantiate<GenericPopup>(this.PopupPrefab);
			genericPopup.TextAreaTMP.text = error;
			genericPopup.transform.SetWorldZ(base.transform.position.z - 1f);
		}
	}
}
