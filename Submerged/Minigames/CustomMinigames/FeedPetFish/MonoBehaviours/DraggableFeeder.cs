using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.FeedPetFish.MonoBehaviours;

[RegisterInIl2Cpp]
public sealed class DraggableFeeder(nint ptr) : MonoBehaviour(ptr)
{
    private const float SNAP_BACK_DURATION = 0.2f;

    public FeedFishMinigame owner;

    public Transform rotationTarget;
    public ParticleSystem fishFood;
    // public Transform fishFoodParent;
    public BoxCollider2D activatedArea;

    // public float shakeDuration = 3f;
    private int _correctFoodIndex;
    private float _counter;
    private bool _isCorrectFood;

    private bool _isNearDropZone;

    private Vector3 _lastLocation;
    private Camera _mainCamera;
    private Vector3 _mouseOffset;
    private Transform _myLid;
    private float _recordedMovement;
    private float _stepDuration;

    private void Start()
    {
        _mainCamera = Camera.main;
        _myLid = transform.GetChild(0);
        fishFood.Stop();
        _counter = 0f;
        _stepDuration = 0f;

        //pick 2 fishgroups randomly
    }

    private void Update()
    {
        if (_isNearDropZone && _isCorrectFood)
        {
            Vector3 difference = rotationTarget.transform.position - transform.position;
            float angle = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);

            fishFood.transform.position = _myLid.position;

            _counter += Time.deltaTime;

            if (_counter > 0.5f)
            {
                _counter -= 0.5f;

                if (_recordedMovement > 1f)
                {
                    fishFood.Play();
                    _stepDuration += 0.5f;

                    if (_stepDuration > 3f)
                    {
                        //Complete this fish food
                        fishFood.Stop();
                        _isCorrectFood = false;
                        this.StartCoroutine(CoRotate());
                        //OnMouseUp();
                        // BoxCollider2D myTouchCollider = GetComponents<BoxCollider2D>().First(col => !col.isTrigger);
                        // myTouchCollider.enabled = false;

                        owner.UpdateCompletedStep(_correctFoodIndex);
                    }
                }
                else
                {
                    fishFood.Stop();
                }

                _recordedMovement = 0f;
            }
        }
    }

    private void OnMouseDown()
    {
        _mouseOffset = gameObject.transform.position - _mainCamera.ScreenToWorldPoint(Input.mousePosition);
    }

    private void OnMouseDrag()
    {
        Vector3 position = _mainCamera.ScreenToWorldPoint(Input.mousePosition) + _mouseOffset;
        position.z = gameObject.transform.position.z;

        gameObject.transform.position = position;

        if (_isNearDropZone)
        {
            _recordedMovement += Mathf.Abs((position - _lastLocation).sqrMagnitude) * 1000;
            _lastLocation = position;
        }
    }

    private void OnMouseUp()
    {
        //CoRoutine to move back to original location
        this.StartCoroutine(CoReturnToShelf());
        _recordedMovement = 0f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == activatedArea)
        {
            if (!_isNearDropZone)
            {
                _isNearDropZone = true;
                //FishFood.Play();
                fishFood.transform.position = _myLid.position;
                _lastLocation = transform.position;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision == activatedArea)
        {
            if (_isNearDropZone)
            {
                _isNearDropZone = false;
                fishFood.Stop();
                fishFood.transform.localPosition = Vector3.zero;
                this.StartCoroutine(CoRotate());
            }
        }
    }

    public void SetCorrectFoodStatus(bool isCorrect, int index = -1)
    {
        _isCorrectFood = isCorrect;

        if (isCorrect)
        {
            _correctFoodIndex = index;
        }
    }

    [HideFromIl2Cpp]
    private IEnumerator CoRotate()
    {
        Quaternion targetRotation = Quaternion.identity;
        Quaternion currentRotation = transform.localRotation;

        for (float t = 0f; t <= SNAP_BACK_DURATION; t += Time.deltaTime)
        {
            float t2 = t / SNAP_BACK_DURATION;
            transform.localRotation = Quaternion.Slerp(currentRotation, targetRotation, t2);

            yield return null;
        }

        transform.localRotation = targetRotation;
    }

    [HideFromIl2Cpp]
    public IEnumerator CoReturnToShelf()
    {
        Vector3 currentPosition = transform.localPosition;
        Vector3 targetPosition = Vector3.zero;

        Quaternion targetRotation = Quaternion.identity;
        Quaternion currentRotation = transform.localRotation;

        targetPosition.z = transform.localPosition.z;

        for (float t = 0f; t <= SNAP_BACK_DURATION; t += Time.deltaTime)
        {
            float t2 = t / SNAP_BACK_DURATION;

            transform.localPosition = Vector3.Lerp(currentPosition, targetPosition, t2);
            transform.localRotation = Quaternion.Slerp(currentRotation, targetRotation, t2);

            yield return null;
        }

        transform.localPosition = targetPosition;
        transform.localRotation = targetRotation;
    }
}
