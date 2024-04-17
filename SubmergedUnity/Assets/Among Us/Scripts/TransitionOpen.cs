using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class TransitionOpen : MonoBehaviour
{
	public float duration = 0.2f;

	public Button.ButtonClickedEvent OnClose = new Button.ButtonClickedEvent();

    [UsedImplicitly]
    public void Close()
	{
	}
}
