using System;
using System.Collections.Generic;
using System.Linq;
using Reactor.Utilities.Attributes;
using Submerged.BaseGame.Extensions;
using Submerged.Minigames.MonoBehaviours;
using Submerged.Localization.Strings;
using Submerged.Map;
using Submerged.Systems.SecuritySabotage;
using TMPro;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.Surveillance;

[RegisterInIl2Cpp]
public sealed class SubmarineSurvillanceMinigame(nint ptr) : Minigame(ptr)
{
    private static readonly int _hsvRangeMin = Shader.PropertyToID("_HSVRangeMin");

    public Transform[] tabs;
    public ClickableSprite[] tabButtons;

    public List<Transform> selectedTabs;
    public int selectedCam;

    public int lastLower;
    public int lastUpper;

    public MeshRenderer lowerRenderer;
    public MeshRenderer upperRenderer;

    public GameObject lowerStatic;
    public GameObject upperStatic;

    public GameObject screenStatic;
    public GameObject screenText;

    public GameObject lowerRecording;
    public GameObject upperRecording;

    public Camera lowerCamera;
    public Camera upperCamera;

    public ClickableSprite lowerWindowButton;
    public ClickableSprite upperWindowButton;

    private float _baseWindowZ;

    // =========================================

    private Transform _lowerWindow;

    private MinigameProperties _minigameProperties;

    private AudioClip _switchSound;
    private Transform _upperWindow;

    private void Awake()
    {
        transform.Find("LowerWindow/Tabs/Lobby/Text").GetComponent<TextMeshPro>().text = Locations.Lobby.ToUpper();
        transform.Find("UpperWindow/Tabs/Lobby/Text").GetComponent<TextMeshPro>().text = Locations.Lobby.ToUpper();
        transform.Find("UpperWindow/Tabs/Y.Hallway/Text").GetComponent<TextMeshPro>().text = Locations.Central.ToUpper();

        transform.Find("LowerWindow/Text").GetComponent<TextMeshPro>().text = Locations.Deck_Lower.ToUpper();
        transform.Find("UpperWindow/Text").GetComponent<TextMeshPro>().text = Locations.Deck_Upper.ToUpper();
    }

    private void Start()
    {
        if (!PlayerControl.LocalPlayer.Data.IsDead)
        {
            ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Security, 1);
        }
        else
        {
            HudManager.Instance.SetHudActive(false);
        }

        _minigameProperties = GetComponent<MinigameProperties>();

        _lowerWindow = transform.Find("LowerWindow");
        _upperWindow = transform.Find("UpperWindow");
        _baseWindowZ = _lowerWindow.localPosition.z;

        lowerRenderer = _lowerWindow.Find("Viewable").GetComponent<MeshRenderer>();
        upperRenderer = _upperWindow.Find("Viewable").GetComponent<MeshRenderer>();

        lowerStatic = lowerRenderer.transform.Find("Static").gameObject;
        upperStatic = upperRenderer.transform.Find("Static").gameObject;

        lowerCamera = GameObject.Find("LowerMoveableCamera").GetComponent<Camera>();
        upperCamera = GameObject.Find("UpperMoveableCamera").GetComponent<Camera>();

        screenStatic = transform.Find("ScreenStatic").gameObject;
        screenText = screenStatic.transform.GetChild(0).gameObject;

        lowerRecording = _lowerWindow.Find("Text/RecordingDot").gameObject;
        upperRecording = _upperWindow.Find("Text/RecordingDot").gameObject;

        tabs = _minigameProperties.transforms;
        tabButtons = new ClickableSprite[tabs.Length];
        selectedTabs = tabs.ToList();

        for (int i = 0; i < tabs.Length; i++)
        {
            int index = i;
            Transform tab = tabs[i];
            ClickableSprite tabButton = tab.gameObject.AddComponent<ClickableSprite>();
            tabButton.onDown += () => ClickTab(index);
            tabButtons[i] = tabButton;
            tab.SetLocalZ(i * 0.1f);
        }

        lowerWindowButton = lowerRenderer.gameObject.AddComponent<ClickableSprite>();
        upperWindowButton = upperRenderer.gameObject.AddComponent<ClickableSprite>();

        lowerWindowButton.onUpAsButton = () => ClickTab(lastLower);
        upperWindowButton.onUpAsButton = () => ClickTab(lastUpper);

        upperStatic.SetActive(true);

        _switchSound = MapLoader.Airship.GetComponentsInChildren<SystemConsole>()
                                .Select(c => c.MinigamePrefab.GetComponent<PlanetSurveillanceMinigame>())
                                .First(t => t != null)
                                .ChangeSound;

        ClickTab(3);
        ClickTab(0);
    }

    private void Update()
    {
        bool timeEven = DateTime.Now.Second % 2 == 0;
        bool active = IsCamActive(selectedCam);
        bool isLower = selectedCam < 3;

        lowerRecording.SetActive(timeEven && isLower);
        upperRecording.SetActive(timeEven && !isLower);

        lowerCamera.enabled = isLower;
        upperCamera.enabled = !isLower;

        lowerRenderer.material.SetFloat(_hsvRangeMin, isLower ? 1 : 0);
        upperRenderer.material.SetFloat(_hsvRangeMin, isLower ? 0 : 1);

        if (isLower)
            lowerStatic.SetActive(!active);
        else
            upperStatic.SetActive(!active);

        bool commsActive = IsCommsActive();
        screenStatic.SetActive(commsActive);

        if (!commsActive) return;
        screenText.SetActive(timeEven);
    }

    // =========================================
    // Systems stuff

    /* Camera Ids
     L_W.Hall -> 0
     L_Electric -> 1
     L_Lobby -> 2
     U_W.Hall -> 3
     U_Y.Hall -> 4
     U_Comms -> 5
     U_Lobby -> 6
    */

    private bool IsCamActive(int cameraIndex) => !SubmarineSecuritySabotageSystem.Instance.IsSabotaged((byte) cameraIndex);

    private bool IsCommsActive() => PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer);

    private void ClickTab(int index)
    {
        if (IsCommsActive() || selectedCam == index) return;
        selectedCam = index;

        // Set Positions
        _lowerWindow.SetLocalZ(index < 3 ? _baseWindowZ - 10 : _baseWindowZ);
        _upperWindow.SetLocalZ(index >= 3 ? _baseWindowZ - 10 : _baseWindowZ);

        Transform selectedTab = tabs[index];

        if (index < 3)
            lastLower = index;
        else
            lastUpper = index;

        switch (index) // DW about this
        {
            case 0:
                selectedTabs.Insert(0, tabs[1]);
                selectedTabs.Insert(0, tabs[0]);

                break;
            case 1:
                selectedTabs.Insert(0, tabs[1]);

                break;
            case 2:
                selectedTabs.Insert(0, tabs[1]);
                selectedTabs.Insert(0, tabs[2]);

                break;
            case 3:
                selectedTabs.Insert(0, tabs[5]);
                selectedTabs.Insert(0, tabs[4]);
                selectedTabs.Insert(0, tabs[3]);

                break;
            case 4:
                selectedTabs.Insert(0, tabs[5]);
                selectedTabs.Insert(0, tabs[3]);
                selectedTabs.Insert(0, tabs[4]);

                break;
            case 5:
                selectedTabs.Insert(0, tabs[4]);
                selectedTabs.Insert(0, tabs[6]);
                selectedTabs.Insert(0, tabs[5]);

                break;
            case 6:
                selectedTabs.Insert(0, tabs[4]);
                selectedTabs.Insert(0, tabs[5]);
                selectedTabs.Insert(0, tabs[6]);

                break;
        }

        selectedTabs = selectedTabs.Distinct().ToList();
        selectedTabs.Remove(selectedTab);
        selectedTabs.Insert(0, selectedTab);

        for (int i = 0; i < selectedTabs.Count; i++) selectedTabs[i].SetLocalZ(i * 0.1f);

        SurvCamera survCam = ShipStatus.Instance.AllCameras[selectedCam];
        lowerCamera.orthographicSize = survCam.CamSize;
        upperCamera.orthographicSize = survCam.CamSize;

        Vector3 position = survCam.transform.position;
        position.z = -100f;
        lowerCamera.transform.position = position + survCam.Offset;
        upperCamera.transform.position = position + survCam.Offset;

        SoundManager.Instance.PlaySound(_switchSound, false);
    }

    public override void Close()
    {
        ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Security, 2);

        if (PlayerControl.LocalPlayer.Data.IsDead)
        {
            HudManager.Instance.SetHudActive(true);
        }

        this.BaseClose();
    }
}
