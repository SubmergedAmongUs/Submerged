using Reactor.Utilities.Attributes;
using Submerged.Enums;
using Submerged.Minigames.CustomMinigames.CamsSabotage.MonoBehaviours;
using Submerged.Minigames.MonoBehaviours;
using Submerged.Localization.Strings;
using Submerged.Map;
using Submerged.Systems.SecuritySabotage;
using TMPro;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.CamsSabotage;

[RegisterInIl2Cpp]
public sealed class SetupCamsMinigame(nint ptr) : Minigame(ptr)
{
    private const float ACCURACY = 0.03f;
    private const float IN_RANGE_TIMER_TARGET = 0.75f;

    private static readonly int _noiseAlpha = Shader.PropertyToID("_NoiseAlpha");

    public MinigameProperties minigameProperties;

    public MeshRenderer cameraScreen;
    public SpriteRenderer offScreen;
    public CamSlider slider;

    public bool myUnderStanding;

    private int _cameraIdx;
    private bool _initialCameraState;
    private float _inRangeTimer;
    private ClickableSprite _onButton;
    private bool _screenOn;
    private Vector3 _sliderPos;
    private float _sliderValue;
    private float _startPos;

    private TextMeshPro _statusText;
    private float _target;

    private void Awake()
    {
        _onButton = transform.Find("UI/On_Off_Button").gameObject.AddComponent<ClickableSprite>();
        _onButton.onUp += PressCamSwitch;

        cameraScreen = transform.Find("SecurityCam").GetComponent<MeshRenderer>();
        offScreen = transform.Find("Background/Overlay").GetComponent<SpriteRenderer>();
        slider = transform.Find("UI/Slide_Switch").gameObject.AddComponent<CamSlider>();
        _sliderPos = slider.transform.localPosition;
        GetClosestCam();

        _initialCameraState = SubmarineSecuritySabotageSystem.Instance.IsSabotaged((byte) _cameraIdx);

        if (_initialCameraState)
        {
            _target = UnityRandom.Range(0, 1f);
            float randomStartPos;

            do
            {
                randomStartPos = UnityRandom.Range(0, 1f);
            }
            while (Mathf.Abs(_target - randomStartPos) < 0.1f);

            slider.SetSliderValue(randomStartPos);
        }
        else
        {
            _target = UnityRandom.Range(0, 1f);
            slider.SetSliderValue(_target);
            slider.forceStop = true;
        }

        _statusText = transform.Find("UI/StatusText").GetComponent<TextMeshPro>();
        _startPos = slider.SliderValue;
        myUnderStanding = _initialCameraState;
    }

    private void Start()
    {
        minigameProperties = GetComponent<MinigameProperties>();

        if (!_initialCameraState || PlayerControl.LocalPlayer.Data.IsDead)
        {
            SoundManager.Instance.PlaySound(minigameProperties.audioClips[0], false);
            _screenOn = !_screenOn;
            _statusText.gameObject.SetActive(!_statusText.gameObject.active);
            offScreen.gameObject.SetActive(!_screenOn);
        }
    }

    private void Update()
    {
        if (PlayerControl.LocalPlayer.Data.IsDead)
        {
            slider.SetSliderValue(_startPos);
        }
    }

    public void LateUpdate()
    {
        if (PlayerControl.LocalPlayer.Data.IsDead)
        {
            slider.SetSliderValue(_startPos);
        }

        _statusText.text = SubmarineSecuritySabotageSystem.Instance.IsSabotaged((byte) _cameraIdx)
            ? Tasks.CamsSabotage_Disabled
            : Tasks.CamsSabotage_Active;

        slider.transform.localPosition = new Vector3(Mathf.Clamp(slider.transform.localPosition.x, slider.localXRange.min, slider.localXRange.max), _sliderPos.y, _sliderPos.z);
        _sliderValue = slider.SliderValue;
        cameraScreen.material.SetFloat(_noiseAlpha, Mathf.Clamp(Mathf.Abs(_target - _sliderValue) * 8f, 0f, 1f));

        if (!_screenOn || amClosing != CloseState.None)
        {
            return;
        }

        if (SubmarineSecuritySabotageSystem.Instance.IsSabotaged((byte) _cameraIdx) != _initialCameraState)
        {
            SoundManager.Instance.PlaySound(HudManager.Instance.TaskUpdateSound, false);
            StartCoroutine(CoStartClose());

            return;
        }

        if (Mathf.Abs(_sliderValue - _target) < ACCURACY)
        {
            if (_initialCameraState)
            {
                _inRangeTimer += Time.deltaTime;
                _statusText.text = Tasks.CamsSabotage_Repairing;

                if (IN_RANGE_TIMER_TARGET - _inRangeTimer <= 0)
                {
                    if (!myUnderStanding)
                    {
                        myUnderStanding = true;
                        ShipStatus.Instance.RpcUpdateSystem(CustomSystemTypes.SecuritySabotage, (byte) (_cameraIdx * 10 + 1));
                    }
                }

                return;
            }

            return;
        }

        _inRangeTimer = 0;

        if (myUnderStanding)
        {
            myUnderStanding = false;
            ShipStatus.Instance.RpcUpdateSystem(CustomSystemTypes.SecuritySabotage, (byte) (_cameraIdx * 10));
        }
    }

    private void GetClosestCam()
    {
        float distance = 1000f;
        int index = -1;

        for (int i = 0; i < ShipStatus.Instance.AllCameras.Count; i++)
        {
            SurvCamera cam = ShipStatus.Instance.AllCameras[i];
            float dist = Vector2.Distance(PlayerControl.LocalPlayer.transform.position, cam.transform.position);

            if (dist < distance)
            {
                index = i;
                distance = dist;
            }
        }

        _cameraIdx = index;
    }

    public void PressCamSwitch()
    {
        if (PlayerControl.LocalPlayer.Data.IsDead) return;

        SoundManager.Instance.PlaySound(minigameProperties.audioClips[0], false);
        _screenOn = !_screenOn;
        _statusText.gameObject.SetActive(!_statusText.gameObject.active);
        offScreen.gameObject.SetActive(!_screenOn);

        if (SubmarineSecuritySabotageSystem.Instance.IsSabotaged((byte) _cameraIdx) == _initialCameraState && !_initialCameraState && !PlayerControl.LocalPlayer.Data.IsDead)
        {
            SoundManager.Instance.PlaySound(SubmarineStatus.instance.minigameProperties.audioClips[0], false);
            StartCoroutine(CoStartClose());
            _inRangeTimer = 0;
            _onButton.onUp -= PressCamSwitch;
            ShipStatus.Instance.RpcUpdateSystem(CustomSystemTypes.SecuritySabotage, (byte) (_cameraIdx * 10));
        }
    }
}
