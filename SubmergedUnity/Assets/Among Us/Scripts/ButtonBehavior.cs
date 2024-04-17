using UnityEngine;
using UnityEngine.UI;

public class ButtonBehavior : UiElement
{
	public bool OnUp = true;
	public bool OnDown;
	public bool Repeat;
	public Button.ButtonClickedEvent OnClick = new Button.ButtonClickedEvent();
	public SpriteRenderer spriteRenderer;
}
