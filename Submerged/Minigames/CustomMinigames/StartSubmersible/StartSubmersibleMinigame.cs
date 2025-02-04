using Reactor.Utilities.Attributes;
using Submerged.Extensions;
using Submerged.Minigames.CustomMinigames.StartSubmersible.MonoBehaviours;
using Submerged.Minigames.MonoBehaviours;
using Submerged.Localization.Strings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Submerged.Minigames.CustomMinigames.StartSubmersible;

[RegisterInIl2Cpp]
public sealed class StartSubmersibleMinigame(nint ptr) : Minigame(ptr)
{
    public SubmersibleSlider shieldSlider;
    public SubmersibleSlider weaponsSlider;
    public GameObject sliderBackground;

    public TextMeshPro text;
    public MinigameProperties minigameProperties;

    public AudioClip backgroundSound;
    private readonly Controller _controller = new();

    private AudioSource _audio;

    private int _flickedSwitches;

    private void Start()
    {
        minigameProperties = GetComponent<MinigameProperties>();
        text = transform.Find("Display/Text").GetComponent<TextMeshPro>();
        backgroundSound = minigameProperties.audioClips[0];

        SetupNeedles();
        SetupKey();
        SetupSwitches();
        SetupSliders();

        transform.Find("Background/ShieldsText").GetComponent<TextMeshPro>().text = Tasks.StartSubmersible_Shields;
        transform.Find("Background/WeaponsText").GetComponent<TextMeshPro>().text = Tasks.StartSubmersible_Weapons;
        transform.Find("Switches/MainEngines").GetComponent<TextMeshPro>().text = Tasks.StartSubmersible_MainEngines;
        transform.Find("Switches/AuxEngines").GetComponent<TextMeshPro>().text = Tasks.StartSubmersible_AuxEngines;
        transform.Find("Switches/ExternalSystems").GetComponent<TextMeshPro>().text = Tasks.StartSubmersible_ExternalSystems;

        EnableEverything();
    }

    private void Update()
    {
        _controller.Update();

        if (amClosing == CloseState.None && (_keyComplete || _flickedSwitches > 0 || weaponsSlider.complete || shieldSlider.complete))
        {
            _audio = SoundManager.Instance.PlaySound(backgroundSound, false, 0.25f);
            text.text = Tasks.StartSubmersible_Status_Starting;
        }

        if (!_keyComplete) CheckKey();

        if (!weaponsSlider.enabled && !shieldSlider.enabled)
        {
            sliderBackground.gameObject.SetActive(true);
        }

        if (_keyComplete && _flickedSwitches == 3 && weaponsSlider.complete && shieldSlider.complete && amClosing == CloseState.None)
        {
            text.text = Tasks.StartSubmersible_Status_Active;
            _audio.Stop();
            if (MyNormTask != null)
            {
                MyNormTask.NextStep();
            }
            StartCoroutine(CoStartClose());
        }
    }

    public void OnDestroy()
    {
        SoundManager.Instance.StopSound(backgroundSound);
    }

    public void SetupKey()
    {
        key = transform.Find("KeySlot/KeyHole/Key").gameObject;
        keyOnLight = transform.Find("KeySlot/Lights/OnLight").gameObject;
        keyOffLight = transform.Find("KeySlot/Lights/OffLight").gameObject;
        keyHole = transform.Find("KeySlot/KeyHole").GetComponent<SpriteRenderer>();
        keyCollider = key.GetComponent<Collider2D>();

        keyOnLight.SetActive(false);
        keyOffLight.SetActive(true);

        keyHole.material.SetFloat("_Outline", 1);
    }

    public void SetupSwitches()
    {
        Transform switches = transform.Find("Switches");

        for (int i = 0; i < 3; i++)
        {
            GameObject switchObj = switches.GetChild(i).gameObject;
            GameObject onObj = switchObj.transform.Find("On").gameObject;
            GameObject offObj = switchObj.transform.Find("Off").gameObject;

            offObj.SetActive(true);
            onObj.SetActive(false);

            PassiveButton button = switchObj.AddComponent<PassiveButton>();
            button.OnMouseOut = new Button.ButtonClickedEvent();
            button.OnMouseOver = new Button.ButtonClickedEvent();
            button.OnClick = new Button.ButtonClickedEvent();
            button.OnClick.AddListener(() =>
            {
                if (!offObj.activeSelf) return;
                offObj.SetActive(false);
                onObj.SetActive(true);
                SoundManager.Instance.PlaySound(minigameProperties.audioClips[1], false);
                _flickedSwitches++;
            });
        }
    }

    public void SetupSliders()
    {
        shieldSlider = transform.Find("Sliders/ShieldsSlider").gameObject.AddComponent<SubmersibleSlider>();
        weaponsSlider = transform.Find("Sliders/WeaponsSlider").gameObject.AddComponent<SubmersibleSlider>();
        sliderBackground = transform.Find("SliderHighlight").gameObject;

        shieldSlider.enabled = false;
        weaponsSlider.enabled = false;
        sliderBackground.gameObject.SetActive(false);
    }

    public void SetupNeedles()
    {
        NeedleBehaviour wheelSpriteNeedle = transform.Find("Wheel/WheelSprite").gameObject.AddComponent<NeedleBehaviour>();
        wheelSpriteNeedle.movementType = NeedleBehaviour.Movement.ConstantBounce;
        wheelSpriteNeedle.duration = 0.075f;
        wheelSpriteNeedle.amount = 1.5f;

        NeedleBehaviour wheelShadowNeedle = transform.Find("Wheel/WheelShadow").gameObject.AddComponent<NeedleBehaviour>();
        wheelShadowNeedle.movementType = NeedleBehaviour.Movement.ConstantBounce;
        wheelShadowNeedle.duration = 0.075f;
        wheelShadowNeedle.amount = 1.5f;

        NeedleBehaviour topLeft = transform.Find("Dials/TopLeft").gameObject.AddComponent<NeedleBehaviour>();
        topLeft.duration = 0.1f;
        topLeft.amount = 10f;
        topLeft.movementType = NeedleBehaviour.Movement.RandomBounce;
        topLeft.randomInitialAngle = true;
        topLeft.initialAngleRange = new FloatRange(30, 80);

        NeedleBehaviour topRight = transform.Find("Dials/TopRight").gameObject.AddComponent<NeedleBehaviour>();
        topRight.duration = 1f;
        topRight.amount = 20f;
        topRight.movementType = NeedleBehaviour.Movement.RandomBounce;

        NeedleBehaviour bottomRight = transform.Find("Dials/BottomRight").gameObject.AddComponent<NeedleBehaviour>();
        bottomRight.duration = 0.1f;
        bottomRight.amount = 10f;
        bottomRight.movementType = NeedleBehaviour.Movement.RandomBounce;
        bottomRight.randomInitialAngle = true;
        bottomRight.initialAngleRange = new FloatRange(-30, -80);

        NeedleBehaviour bottomLeft = transform.Find("Dials/BottomLeft").gameObject.AddComponent<NeedleBehaviour>();
        bottomLeft.duration = 10f;
        bottomLeft.amount = 89f;
        bottomLeft.movementType = NeedleBehaviour.Movement.ConstantBounce;
    }

    public void EnableEverything()
    {
        text.text = Tasks.StartSubmersible_Status_Inactive;

        sliderBackground.gameObject.SetActive(true);
        weaponsSlider.enabled = true;
        shieldSlider.enabled = true;

        sliderBackground.gameObject.SetActive(false);

        Transform switches = transform.Find("Switches");

        for (int i = 0; i < 3; i++)
        {
            if (switches.GetChild(i).Find("Off/OffSwitch").TryGetComponent(out SpriteRenderer rend))
            {
                rend.material.SetFloat("_Outline", 1);
            }
        }
    }

    private void CheckKey()
    {
        if (_keyComplete) return;

        switch (_controller.CheckDrag(keyCollider))
        {
            case DragState.Dragging:
            {
                Vector2 vector = key.transform.position;
                float num = Vector2.SignedAngle(_controller.DragStartPosition - vector, _controller.DragPosition - vector);
                key.transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Clamp(num, -90f, 90f));

                return;
            }
            case DragState.Released:
            {
                float num2 = key.transform.localEulerAngles.z;

                if (num2 > 180f)
                {
                    num2 -= 360f;
                }

                num2 %= 360f;

                bool complete = Mathf.Abs(num2) > 80f;
                keyOnLight.gameObject.SetActive(complete);
                keyOffLight.gameObject.SetActive(!complete);

                if (complete)
                {
                    _keyComplete = true;
                    keyHole.material.SetFloat("_Outline", 0);
                }
                else
                {
                    key.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                }
            }

                break;
        }
    }

    #region Key

    public GameObject key;
    public GameObject keyOnLight;
    public GameObject keyOffLight;
    public SpriteRenderer keyHole;
    public Collider2D keyCollider;

    private bool _keyComplete;

    #endregion
}
