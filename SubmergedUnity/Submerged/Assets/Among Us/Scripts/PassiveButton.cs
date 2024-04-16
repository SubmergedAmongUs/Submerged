using UnityEngine;
using UnityEngine.UI;

public class PassiveButton : PassiveUiElement
{
	public Button.ButtonClickedEvent OnClick = new Button.ButtonClickedEvent();
	public AudioClip ClickSound;
	public bool OnUp = true;
	public bool OnDown;
	public bool OnRepeat;
	public float RepeatDuration = 0.3f;
}
