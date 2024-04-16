using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ActionMapGlyphDisplay : MonoBehaviour
{
	public RewiredConstsEnum.Action actionToDisplayMappedGlyphFor;

	private SpriteRenderer sr;

	private void Awake()
	{
		this.sr = base.GetComponent<SpriteRenderer>();
		this.UpdateGlyphDisplay();
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Combine(ActiveInputManager.CurrentInputSourceChanged, new Action(this.UpdateGlyphDisplay));
	}

	private void OnDestroy()
	{
		ActiveInputManager.CurrentInputSourceChanged = (Action)Delegate.Remove(ActiveInputManager.CurrentInputSourceChanged, new Action(this.UpdateGlyphDisplay));
	}

	public void UpdateGlyphDisplay()
	{
		if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick)
		{
			if (this.sr)
			{
				this.sr.gameObject.SetActive(true);
				GlyphCollection.ErrorCode errorCode;
				Sprite sprite = GlyphCollection.FindGlyph((int)this.actionToDisplayMappedGlyphFor, out errorCode);
				if (errorCode != GlyphCollection.ErrorCode.NoController)
				{
					this.sr.sprite = sprite;
					return;
				}
				this.sr.gameObject.SetActive(false);
				return;
			}
		}
		else if (this.sr)
		{
			this.sr.gameObject.SetActive(false);
		}
	}
}
