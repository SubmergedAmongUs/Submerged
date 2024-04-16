using System;
using UnityEngine;

[RequireComponent(typeof(Scroller))]
public class ScrollToSelection : MonoBehaviour
{
	private Scroller scrollRect;

	private UIScrollbarHelper[] childElements;

	public float wantedValue;

	private GameObject lastSelectedObject;

	public bool cursorEnabled;

	public bool killScroll;

	public bool onePage;

	private void Awake()
	{
		this.scrollRect = base.GetComponent<Scroller>();
	}

	private void OnEnable()
	{
		this.childElements = base.GetComponentsInChildren<UIScrollbarHelper>(false);
		this.scrollRect = base.GetComponent<Scroller>();
		this.wantedValue = 0f;
		for (int i = 0; i < this.childElements.Length; i++)
		{
			this.childElements[i].index = i;
		}
	}

	private void Start()
	{
		this.childElements = base.GetComponentsInChildren<UIScrollbarHelper>(false);
		for (int i = 0; i < this.childElements.Length; i++)
		{
			this.childElements[i].index = i;
		}
	}

	private void Update()
	{
		if (!this.scrollRect.enabled)
		{
			this.onePage = true;
			return;
		}
		this.cursorEnabled = (Controller.currentTouchType > Controller.TouchType.Joystick);
		if (this.cursorEnabled)
		{
			this.wantedValue = this.scrollRect.Inner.localPosition.y;
			return;
		}
		this.onePage = false;
		GameObject gameObject = ControllerManager.Instance.CurrentUiState.CurrentSelection ? ControllerManager.Instance.CurrentUiState.CurrentSelection.gameObject : null;
		if (this.lastSelectedObject != gameObject)
		{
			if (gameObject && gameObject.transform.IsChildOf(base.transform))
			{
				this.ScrollToRect(gameObject.transform);
			}
			this.lastSelectedObject = gameObject;
			if (!gameObject)
			{
				this.killScroll = true;
			}
		}
		if (this.scrollRect.allowY)
		{
			if (!this.killScroll && this.scrollRect.Inner.localPosition.y != this.wantedValue)
			{
				Vector3 localPosition = this.scrollRect.Inner.localPosition;
				localPosition.y = Mathf.Lerp(localPosition.y, this.wantedValue, Time.unscaledDeltaTime * 3f);
				this.scrollRect.Inner.localPosition = localPosition;
				return;
			}
			if (this.killScroll)
			{
				this.wantedValue = this.scrollRect.Inner.localPosition.y;
			}
		}
	}

	private void ScrollToRect(Transform targetRectTransform)
	{
		if (this.scrollRect.allowY)
		{
			UIScrollbarHelper uiscrollbarHelper = targetRectTransform.GetComponent<UIScrollbarHelper>();
			if (uiscrollbarHelper == null)
			{
				uiscrollbarHelper = targetRectTransform.GetComponentInParent<UIScrollbarHelper>();
			}
			float num = (this.scrollRect.Colliders.Length != 0) ? this.scrollRect.Colliders[0].transform.localPosition.y : 0f;
			if (uiscrollbarHelper == null)
			{
				this.wantedValue = -this.scrollRect.Inner.transform.localPosition.y + num;
				this.killScroll = true;
				return;
			}
			if (this.childElements.Length == 0)
			{
				this.childElements = base.GetComponentsInChildren<UIScrollbarHelper>(false);
				for (int i = 0; i < this.childElements.Length; i++)
				{
					this.childElements[i].index = i;
				}
				if (this.childElements.Length == 0)
				{
					return;
				}
			}
			this.killScroll = false;
			this.wantedValue = Mathf.Clamp(-uiscrollbarHelper.transform.localPosition.y + num, this.scrollRect.YBounds.min, this.scrollRect.YBounds.max);
		}
	}
}
