using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InnerNet;
using TMPro;
using UnityEngine;

public class HudManager : DestroyableSingleton<HudManager>
{
	public FollowerCamera PlayerCam;

	public Camera UICamera;

	public MeetingHud MeetingPrefab;

	public KillButtonManager KillButton;

	public UseButtonManager UseButton;

	public ReportButtonManager ReportButton;

	public TextMeshPro GameSettings;

	public GameObject TaskStuff;

	public ChatController Chat;

	public DialogueBox Dialogue;

	public TextMeshPro TaskText;

	public Transform TaskCompleteOverlay;

	private float taskDirtyTimer;

	public MeshRenderer ShadowQuad;

	public SpriteRenderer FullScreen;

	public SpriteRenderer MapButton;

	public KillOverlay KillOverlay;

	public IVirtualJoystick joystick;

	public MonoBehaviour[] Joysticks;

	public DiscussBehaviour discussEmblem;

	public ShhhBehaviour shhhEmblem;

	public IntroCutscene IntroPrefab;

	public OptionsMenuBehaviour GameMenu;

	public NotificationPopper Notifier;

	public RoomTracker roomTracker;

	public AudioClip TaskCompleteSound;

	public AudioClip TaskUpdateSound;

	public GameObject[] consoleUIObjects;

	private StringBuilder tasksString = new StringBuilder();

	private bool isIntroDisplayed;

	private DualshockLightManager.LightOverlayHandle lightFlashHandle;

	public Coroutine ReactorFlash { get; set; }

	public Coroutine OxyFlash { get; set; }

	public void Start()
	{
		if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick)
		{
			this.SetTouchType(ControlTypes.Controller);
			return;
		}
		this.SetTouchType(SaveManager.ControlMode);
	}

	public void ShowTaskComplete()
	{
		base.StartCoroutine(this.CoTaskComplete());
	}

	private IEnumerator CoTaskComplete()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.TaskCompleteSound, false, 1f);
		}
		this.TaskCompleteOverlay.gameObject.SetActive(true);
		yield return Effects.Slide2D(this.TaskCompleteOverlay, new Vector2(0f, -8f), Vector2.zero, 0.25f);
		for (float time = 0f; time < 0.75f; time += Time.deltaTime)
		{
			yield return null;
		}
		yield return Effects.Slide2D(this.TaskCompleteOverlay, Vector2.zero, new Vector2(0f, 8f), 0.25f);
		this.TaskCompleteOverlay.gameObject.SetActive(false);
		yield break;
	}

	public void SetJoystickSize(float size)
	{
		if (this.joystick != null && this.joystick is VirtualJoystick)
		{
			VirtualJoystick virtualJoystick = (VirtualJoystick)this.joystick;
			virtualJoystick.transform.localScale = new Vector3(size, size, 1f);
			AspectPosition component = virtualJoystick.GetComponent<AspectPosition>();
			float num = Mathf.Lerp(0.65f, 1.1f, FloatRange.ReverseLerp(size, 0.5f, 1.5f));
			component.DistanceFromEdge = new Vector3(num, num, -10f);
			component.AdjustPosition();
		}
	}

	public void SetTouchType(ControlTypes type)
	{
		if (this.joystick != null)
		{
			 UnityEngine.Object.Destroy((this.joystick as MonoBehaviour).gameObject);
		}
		MonoBehaviour monoBehaviour = UnityEngine.Object.Instantiate<MonoBehaviour>(this.Joysticks[Mathf.Clamp((int)type, 0, this.Joysticks.Length - 1)]);
		monoBehaviour.transform.SetParent(base.transform, false);
		this.joystick = monoBehaviour.GetComponent<IVirtualJoystick>();
		this.SetJoystickSize(SaveManager.JoystickSize);
	}

	public void OpenMap()
	{
		if (PlayerControl.LocalPlayer.Data == null)
		{
			return;
		}
		this.ShowMap(delegate(MapBehaviour m)
		{
			m.ShowNormalMap();
		});
	}

	public void ShowMap(Action<MapBehaviour> mapAction)
	{
		if (this.isIntroDisplayed)
		{
			return;
		}
		if (!ShipStatus.Instance)
		{
			return;
		}
		if (!MapBehaviour.Instance)
		{
			MapBehaviour.Instance = UnityEngine.Object.Instantiate<MapBehaviour>(ShipStatus.Instance.MapPrefab, base.transform);
			MapBehaviour.Instance.gameObject.SetActive(false);
		}
		mapAction(MapBehaviour.Instance);
	}

	public void SetHudActive(bool isActive)
	{
		this.UseButton.gameObject.SetActive(isActive);
		this.UseButton.Refresh();
		this.ReportButton.gameObject.SetActive(isActive);
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		GameData.PlayerInfo playerInfo = (localPlayer != null) ? localPlayer.Data : null;
		this.KillButton.gameObject.SetActive(isActive && playerInfo.IsImpostor && !playerInfo.IsDead);
		this.TaskText.transform.parent.gameObject.SetActive(isActive);
		this.roomTracker.gameObject.SetActive(isActive);
	}

	public void Update()
	{
		this.taskDirtyTimer += Time.deltaTime;
		if (this.taskDirtyTimer > 0.25f)
		{
			this.taskDirtyTimer = 0f;
			if (!PlayerControl.LocalPlayer)
			{
				this.TaskText.text = string.Empty;
				return;
			}
			GameData.PlayerInfo data = PlayerControl.LocalPlayer.Data;
			if (data == null)
			{
				return;
			}
			bool isImpostor = data.IsImpostor;
			this.tasksString.Clear();
			if (PlayerControl.LocalPlayer.myTasks.Count == 0)
			{
				this.tasksString.Append("None");
			}
			else
			{
				for (int i = 0; i < PlayerControl.LocalPlayer.myTasks.Count; i++)
				{
					PlayerTask playerTask = PlayerControl.LocalPlayer.myTasks[i];
					if (playerTask)
					{
						if (playerTask.TaskType == TaskTypes.FixComms && !isImpostor)
						{
							this.tasksString.Clear();
							playerTask.AppendTaskText(this.tasksString);
							break;
						}
						playerTask.AppendTaskText(this.tasksString);
					}
				}
				this.tasksString.TrimEnd();
			}
			this.TaskText.text = this.tasksString.ToString();
		}
	}

	public IEnumerator ShowEmblem(bool shhh)
	{
		if (shhh)
		{
			this.shhhEmblem.gameObject.SetActive(true);
			yield return this.shhhEmblem.PlayAnimation();
			this.shhhEmblem.gameObject.SetActive(false);
		}
		else
		{
			this.discussEmblem.gameObject.SetActive(true);
			yield return this.discussEmblem.PlayAnimation();
			this.discussEmblem.gameObject.SetActive(false);
		}
		yield break;
	}

	public void StartReactorFlash()
	{
		if (this.ReactorFlash == null)
		{
			this.ReactorFlash = base.StartCoroutine(this.CoReactorFlash());
		}
	}

	public void StartOxyFlash()
	{
		if (this.OxyFlash == null)
		{
			this.OxyFlash = base.StartCoroutine(this.CoReactorFlash());
		}
	}

	public void ShowPopUp(string text)
	{
		this.Dialogue.Show(text);
	}

	public void StopReactorFlash()
	{
		if (this.ReactorFlash != null)
		{
			base.StopCoroutine(this.ReactorFlash);
			this.ReactorFlash = null;
			this.FullScreen.enabled = false;
			PlainShipRoom plainShipRoom = ShipStatus.Instance.AllRooms.FirstOrDefault((PlainShipRoom r) => r is ReactorShipRoom);
			if (plainShipRoom)
			{
				((ReactorShipRoom)plainShipRoom).StopMeltdown();
			}
			if (this.lightFlashHandle != null)
			{
				this.lightFlashHandle.Dispose();
				this.lightFlashHandle = null;
			}
		}
	}

	public void StopOxyFlash()
	{
		if (this.OxyFlash != null)
		{
			base.StopCoroutine(this.OxyFlash);
			this.FullScreen.enabled = false;
			this.OxyFlash = null;
			if (this.lightFlashHandle != null)
			{
				this.lightFlashHandle.Dispose();
				this.lightFlashHandle = null;
			}
		}
	}

	public IEnumerator CoFadeFullScreen(Color source, Color target, float duration = 0.2f)
	{
		if (this.Chat.IsOpen)
		{
			base.StartCoroutine(this.Chat.CoClose());
		}
		if (this.FullScreen.enabled && this.FullScreen.color == target)
		{
			yield break;
		}
		this.FullScreen.enabled = true;
		for (float t = 0f; t < duration; t += Time.deltaTime)
		{
			if (!this.FullScreen)
			{
				yield break;
			}
			this.FullScreen.color = Color.Lerp(source, target, t / duration);
			yield return null;
		}
		if (this.FullScreen)
		{
			this.FullScreen.color = target;
			if (target.a < 0.05f)
			{
				this.FullScreen.enabled = false;
			}
		}
		yield break;
	}

	private IEnumerator CoReactorFlash()
	{
		WaitForSeconds wait = new WaitForSeconds(0.5f);
		this.FullScreen.color = new Color(1f, 0f, 0f, 0.37254903f);
		for (;;)
		{
			this.FullScreen.enabled = !this.FullScreen.enabled;
			SoundManager.Instance.PlaySound(ShipStatus.Instance.SabotageSound, false, 1f);
			if (this.lightFlashHandle == null)
			{
				this.lightFlashHandle = DestroyableSingleton<DualshockLightManager>.Instance.AllocateLight();
				this.lightFlashHandle.color = new Color(1f, 0f, 0f, 1f);
				this.lightFlashHandle.intensity = 1f;
			}
			this.lightFlashHandle.color.a = (this.FullScreen.enabled ? 1f : 0f);
			yield return wait;
		}
		yield break;
	}

	public IEnumerator CoShowIntro(List<PlayerControl> yourTeam)
	{
		while (!ShipStatus.Instance)
		{
			yield return null;
		}
		this.isIntroDisplayed = true;
		DestroyableSingleton<HudManager>.Instance.FullScreen.transform.localPosition = new Vector3(0f, 0f, -250f);
		yield return DestroyableSingleton<HudManager>.Instance.ShowEmblem(true);
		IntroCutscene introCutscene = UnityEngine.Object.Instantiate<IntroCutscene>(this.IntroPrefab, base.transform);
		yield return introCutscene.CoBegin(yourTeam, PlayerControl.LocalPlayer.Data.IsImpostor);
		PlayerControl.LocalPlayer.SetKillTimer(10f);
		((SabotageSystemType)ShipStatus.Instance.Systems[SystemTypes.Sabotage]).ForceSabTime(10f);
		yield return ShipStatus.Instance.PrespawnStep();
		yield return this.CoFadeFullScreen(Color.black, Color.clear, 0.2f);
		DestroyableSingleton<HudManager>.Instance.FullScreen.transform.localPosition = new Vector3(0f, 0f, -500f);
		this.isIntroDisplayed = false;
		yield break;
	}

	public void OpenMeetingRoom(PlayerControl reporter)
	{
		if (MeetingHud.Instance)
		{
			return;
		}
		Debug.Log("Opening meeting room: " + ((reporter != null) ? reporter.ToString() : null));
		ShipStatus.Instance.RepairGameOverSystems();
		MeetingHud.Instance = UnityEngine.Object.Instantiate<MeetingHud>(this.MeetingPrefab);
		MeetingHud.Instance.ServerStart(reporter.PlayerId);
		AmongUsClient.Instance.Spawn(MeetingHud.Instance, -2, SpawnFlags.None);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (this.lightFlashHandle != null)
		{
			this.lightFlashHandle.Dispose();
			this.lightFlashHandle = null;
		}
	}
}
