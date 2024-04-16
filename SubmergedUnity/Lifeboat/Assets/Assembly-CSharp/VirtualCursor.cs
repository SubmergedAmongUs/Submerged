using System;
using Rewired;
using Rewired.ControllerExtensions;
using UnityEngine;
using Object = UnityEngine.Object;

public class VirtualCursor : MonoBehaviour
{
	public float speed = 5f;

	private float currentSpeed;

	public float acceleration = 1f;

	public Vector2 screenBounds;

	public Vector3 position;

	private const float deadzone = 0.03f;

	private const float touchpadSensitivity = 4f;

	public Camera cam;

	public static Vector2 currentPosition;

	public static bool buttonDown;

	public static bool joystickMoved;

	public static VirtualCursor instance;

	public static int horizontalAxis = 9;

	public static int verticalAxis = 10;

	private int framesVisible;

	private const int minFramesToAppear = 3;

	private SpriteRenderer sr;

	private Vector2 prevTouchPos;

	private bool setTouchPos;

	public static bool CursorActive
	{
		get
		{
			return VirtualCursor.instance && VirtualCursor.instance.isActiveAndEnabled && SpecialInputHandler.disableVirtualCursorCount == 0;
		}
	}

	private void Awake()
	{
		if (VirtualCursor.instance)
		{
			 UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		VirtualCursor.instance = this;
		Object.DontDestroyOnLoad(base.gameObject);
		this.sr = base.GetComponentInChildren<SpriteRenderer>(true);
	}

	private void OnEnable()
	{
		this.cam = Camera.main;
		if (this.cam)
		{
			this.SetScreenPosition(Vector2.zero);
		}
		this.framesVisible = 0;
		this.sr.enabled = false;
	}

	private void OnDestroy()
	{
		if (VirtualCursor.instance == this)
		{
			VirtualCursor.instance = null;
		}
	}

	private void Start()
	{
		this.cam = Camera.main;
		this.position = base.transform.position;
		if (this.cam)
		{
			float num = (float)Screen.width / (float)Screen.height;
			this.screenBounds = new Vector2(num * this.cam.orthographicSize, this.cam.orthographicSize);
		}
	}

	public void SetWorldPosition(Vector2 worldPos)
	{
		VirtualCursor.currentPosition = (base.transform.position = worldPos);
		this.position = worldPos - new Vector2(this.cam.transform.position.x, this.cam.transform.position.y);
	}

	public void SetScreenPosition(Vector2 screenPos)
	{
		this.position = screenPos;
		VirtualCursor.currentPosition = (base.transform.position = this.position + this.cam.transform.position);
	}

	private void Update()
	{
		Player player = ReInput.players.GetPlayer(0);
		Vector3 vector = new Vector3(player.GetAxis(VirtualCursor.horizontalAxis), player.GetAxis(VirtualCursor.verticalAxis), 0f);
		VirtualCursor.joystickMoved = (vector.x != 0f || vector.y != 0f);
		if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick && SpecialInputHandler.count == 0)
		{
			if (this.framesVisible < 3)
			{
				this.framesVisible++;
			}
			else
			{
				this.sr.enabled = true;
			}
		}
		else
		{
			this.framesVisible = 0;
			this.sr.enabled = false;
		}
		if (ControllerManager.Instance.IsUiControllerActive)
		{
			base.gameObject.SetActive(false);
			return;
		}
		if (player.controllers.joystickCount > 0)
		{
			IDualShock4Extension extension = player.controllers.Joysticks[0].GetExtension<IDualShock4Extension>();
			if (extension != null && extension.touchCount > 0)
			{
				extension.GetTouchPosition(0, out Vector2 vector2);
				float num = (float)Screen.width / (float)Screen.height;
				if (this.setTouchPos)
				{
					Vector2 vector3 = vector2 - this.prevTouchPos;
					this.position.x = this.position.x + vector3.x * (4f * num);
					this.position.y = this.position.y + vector3.y * 4f;
				}
				this.setTouchPos = true;
				this.prevTouchPos = vector2;
			}
			else
			{
				this.setTouchPos = false;
			}
		}
		if (vector.magnitude > 0.03f)
		{
			this.currentSpeed += Time.deltaTime * this.acceleration;
			if (this.currentSpeed > this.speed)
			{
				this.currentSpeed = this.speed;
			}
			this.position += vector * Time.deltaTime * this.currentSpeed;
		}
		else
		{
			this.currentSpeed = 0f;
		}
		if (!this.cam)
		{
			this.cam = Camera.main;
		}
		if (this.cam)
		{
			float num2 = (float)Screen.width / (float)Screen.height;
			this.screenBounds = new Vector2(num2 * this.cam.orthographicSize, this.cam.orthographicSize);
			this.position.x = Mathf.Clamp(this.position.x, -this.screenBounds.x, this.screenBounds.x);
			this.position.y = Mathf.Clamp(this.position.y, -this.screenBounds.y, this.screenBounds.y);
			VirtualCursor.currentPosition = (base.transform.position = this.position + this.cam.transform.position);
		}
		VirtualCursor.buttonDown = player.GetButton(11);
	}
}
