using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using Reactor.Utilities.Attributes;
using Submerged.Elevators;
using Submerged.Extensions;
using Submerged.Map;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Submerged.Systems.Elevator;

[RegisterInIl2Cpp]
public sealed class SubmarineElevator(nint ptr) : MonoBehaviour(ptr)
{
    private static readonly int _mainTex = Shader.PropertyToID("_MainTex");
    private static readonly int _alpha = Shader.PropertyToID("_Alpha");
    private static readonly int _colorPropertyID = Shader.PropertyToID("_Color");

    private static readonly float[] _individualTimings =
    [
        // These are the timings for each stage of the elevator movement animation
        // All times are in seconds

        0.40f, // DoorsClosing
        0.50f, // FadeToBlack
        1.25f, // ElevatorMovingOut
        0.25f, // Wait
        1.25f, // ElevatorMovingIn
        0.50f, // FadingToClear
        0.20f // DoorsOpening

        // Total - 5.6s
    ];

    private int _colorCount;
    private AudioSource _dingSound;
    private Material _lowerBlackoutMat;

    private Mesh _lowerMesh;

    private AudioSource _movingSound;
    private Il2CppStructArray<Vector2> _newUVs;
    private Vector2[] _originalUVs;

    private Material _upperBlackoutMat;
    private Mesh _upperMesh;

    private int _uvCount;
    private Il2CppStructArray<Color> _vertexColors;

    public SubmarineElevatorSystem system;

    public bool LocalPlayerInElevator => GetInElevator(PlayerControl.LocalPlayer);

    // public FollowerCamera FollowerCamera;

    public float FollowerCameraShakeAmount { get; set; }
    public float FollowerCameraShakePeriod { get; set; }

    public void Start()
    {
        _lowerBlackoutMat = lowerBlackout.Value.material;
        _upperBlackoutMat = upperBlackout.Value.material;

        _lowerMesh = lowerElevatorFilter.Value.mesh;
        _upperMesh = upperElevatorFilter.Value.mesh;

        _originalUVs = _lowerMesh.uv;
        _newUVs = _originalUVs;
        _vertexColors = _lowerMesh.colors;
        _uvCount = _originalUVs.Length;
        _colorCount = _vertexColors.Length;

        _movingSound = SoundManager.Instance.PlayDynamicSound($"moving_{name}_{Pointer}",
            SubmarineStatus.instance.minigameProperties.audioClips[4],
            true,
            new Action<AudioSource, float>(DynamicMoveSound),
            SoundManager.Instance.SfxChannel);
    }

    private void Update()
    {
        if (!system.moving)
        {
            lowerElevator.Value.material.SetTexture(_mainTex, system.upperDeckIsTargetFloor ? elevatorWithoutDoorL1 : elevatorWithoutDoorL2);
            upperElevator.Value.material.SetTexture(_mainTex, system.upperDeckIsTargetFloor ? elevatorWithoutDoorL1 : elevatorWithoutDoorL2);

            lowerOuterDoor.Value.SetDoorway(!system.upperDeckIsTargetFloor);
            lowerInnerDoor.Value.SetDoorway(!system.upperDeckIsTargetFloor);

            upperInnerDoor.Value.SetDoorway(system.upperDeckIsTargetFloor);
            upperOuterDoor.Value.SetDoorway(system.upperDeckIsTargetFloor);

            _lowerBlackoutMat.SetFloat(_alpha, 0);
            _upperBlackoutMat.SetFloat(_alpha, 0);

            lowerInnerRend.Value.enabled = !system.upperDeckIsTargetFloor;
            upperInnerRend.Value.enabled = system.upperDeckIsTargetFloor;

            lowerLight.Value.enabled = !system.upperDeckIsTargetFloor;
            upperLight.Value.enabled = system.upperDeckIsTargetFloor;

            // Elevator is on the floor so 0 for current floor
            SetMeshMovement(lowerElevator, _lowerMesh, system.upperDeckIsTargetFloor ? -1 : 0);
            SetMeshMovement(upperElevator, _upperMesh, !system.upperDeckIsTargetFloor ? 1 : 0);

            _movingSound.Stop();
        }
        else
        {
            lowerLight.Value.enabled = system.upperDeckIsTargetFloor;
            upperLight.Value.enabled = !system.upperDeckIsTargetFloor;

            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.AmOwner) continue;

                if (GetInElevator(player) && !CheckInElevator(player.transform.position))
                {
                    player.transform.position = GetTargetSyncPosition(player.NetTransform.incomingPosQueue);
                }
            }

            ElevatorMovementStage stage = GetMovementStageFromSystem();

            switch (stage)
            {
                case ElevatorMovementStage.DoorsClosing:
                {
                    FollowerCameraShakeAmount = 0f;
                    FollowerCameraShakePeriod = 0f;

                    lowerOuterDoor.Value.SetDoorway(false);
                    lowerInnerDoor.Value.SetDoorway(false);

                    upperInnerDoor.Value.SetDoorway(false);
                    upperOuterDoor.Value.SetDoorway(false);

                    // Real inner doors are moving so elevator doors are hidden
                    lowerInnerRend.Value.enabled = system.upperDeckIsTargetFloor;
                    upperInnerRend.Value.enabled = !system.upperDeckIsTargetFloor;

                    lowerElevator.Value.material.SetTexture(_mainTex, system.upperDeckIsTargetFloor ? elevatorWithoutDoorL2 : elevatorWithoutDoorL1);
                    upperElevator.Value.material.SetTexture(_mainTex, system.upperDeckIsTargetFloor ? elevatorWithoutDoorL2 : elevatorWithoutDoorL1);

                    _lowerBlackoutMat.SetFloat(_alpha, 0);
                    _upperBlackoutMat.SetFloat(_alpha, 0);

                    // Elevator is on the floor so 0 for current floor but target is opposite of current floor
                    SetMeshMovement(lowerElevator, _lowerMesh, !system.upperDeckIsTargetFloor ? -1 : 0);
                    SetMeshMovement(upperElevator, _upperMesh, system.upperDeckIsTargetFloor ? 1 : 0);

                    _movingSound.Stop();

                    break;
                }
                case ElevatorMovementStage.FadingToBlack:
                {
                    if (!_movingSound.isPlaying) _movingSound.Play();

                    FollowerCameraShakeAmount = 0f;
                    FollowerCameraShakePeriod = 0f;

                    // Doors done moving so elevator doors replace them
                    lowerInnerRend.Value.enabled = false;
                    upperInnerRend.Value.enabled = false;

                    lowerElevator.Value.material.SetTexture(_mainTex, system.upperDeckIsTargetFloor ? elevatorWithDoorL2 : elevatorWithDoorL1);
                    upperElevator.Value.material.SetTexture(_mainTex, system.upperDeckIsTargetFloor ? elevatorWithDoorL2 : elevatorWithDoorL1);

                    if (LocalPlayerInElevator || PlayerControl.LocalPlayer.Data.IsDead)
                    {
                        if (!PlayerControl.LocalPlayer.Data.IsDead)
                        {
                            _lowerBlackoutMat.SetFloat(_alpha, GetMovementLerp(stage));
                            _upperBlackoutMat.SetFloat(_alpha, GetMovementLerp(stage));

                            SetMeshMovement(lowerElevator, _lowerMesh, 0);
                            SetMeshMovement(upperElevator, _upperMesh, 0);
                        }
                        else
                        {
                            _lowerBlackoutMat.SetFloat(_alpha, 0);
                            _upperBlackoutMat.SetFloat(_alpha, 0);

                            SetMeshMovement(lowerElevator, _lowerMesh, !system.upperDeckIsTargetFloor ? -1 : 0);
                            SetMeshMovement(upperElevator, _upperMesh, system.upperDeckIsTargetFloor ? 1 : 0);
                        }
                    }
                    else
                    {
                        _lowerBlackoutMat.SetFloat(_alpha, 0);
                        _upperBlackoutMat.SetFloat(_alpha, 0);
                        // Elevator is on the floor so 0 for current floor but target is opposite of current floor
                        SetMeshMovement(lowerElevator, _lowerMesh, !system.upperDeckIsTargetFloor ? -1 : 0);
                        SetMeshMovement(upperElevator, _upperMesh, system.upperDeckIsTargetFloor ? 1 : 0);
                    }

                    break;
                }
                case ElevatorMovementStage.ElevatorMovingOut:
                {
                    if (!_movingSound.isPlaying) _movingSound.Play();

                    // Doors done moving so elevator doors replace them
                    lowerInnerRend.Value.enabled = false;
                    upperInnerRend.Value.enabled = false;

                    lowerElevator.Value.material.SetTexture(_mainTex, system.upperDeckIsTargetFloor ? elevatorWithDoorL2 : elevatorWithDoorL1);
                    upperElevator.Value.material.SetTexture(_mainTex, system.upperDeckIsTargetFloor ? elevatorWithDoorL2 : elevatorWithDoorL1);

                    if (LocalPlayerInElevator || PlayerControl.LocalPlayer.Data.IsDead)
                    {
                        if (!PlayerControl.LocalPlayer.Data.IsDead)
                        {
                            FollowerCameraShakeAmount = 0.03f;
                            FollowerCameraShakePeriod = 400f;

                            _lowerBlackoutMat.SetFloat(_alpha, 1);
                            _upperBlackoutMat.SetFloat(_alpha, 1);
                            SetMeshMovement(lowerElevator, _lowerMesh, 0);
                            SetMeshMovement(upperElevator, _upperMesh, 0);
                        }
                        else
                        {
                            FollowerCameraShakeAmount = 0f;
                            FollowerCameraShakePeriod = 0f;
                            _lowerBlackoutMat.SetFloat(_alpha, 0);
                            _upperBlackoutMat.SetFloat(_alpha, 0);

                            SetMeshMovement(lowerElevator, _lowerMesh, !system.upperDeckIsTargetFloor ? -1 : 0);
                            SetMeshMovement(upperElevator, _upperMesh, system.upperDeckIsTargetFloor ? 1 : 0);
                        }
                    }
                    else
                    {
                        FollowerCameraShakeAmount = 0f;
                        FollowerCameraShakePeriod = 0f;
                        _lowerBlackoutMat.SetFloat(_alpha, 0);
                        _upperBlackoutMat.SetFloat(_alpha, 0);
                        // Elevator on opposite to target floor is lerping movement, other one is invisible
                        SetMeshMovement(lowerElevator, _lowerMesh, !system.upperDeckIsTargetFloor ? -1 : -GetMovementLerp(stage));
                        SetMeshMovement(upperElevator, _upperMesh, system.upperDeckIsTargetFloor ? 1 : GetMovementLerp(stage));
                    }

                    break;
                }
                case ElevatorMovementStage.Wait:
                {
                    if (!_movingSound.isPlaying) _movingSound.Play();
                    // Doors done moving so elevator doors replace them
                    lowerInnerRend.Value.enabled = false;
                    upperInnerRend.Value.enabled = false;

                    lowerElevator.Value.material.SetTexture(_mainTex, system.upperDeckIsTargetFloor ? elevatorWithDoorL2 : elevatorWithDoorL1);
                    upperElevator.Value.material.SetTexture(_mainTex, system.upperDeckIsTargetFloor ? elevatorWithDoorL2 : elevatorWithDoorL1);

                    if (LocalPlayerInElevator || PlayerControl.LocalPlayer.Data.IsDead)
                    {
                        if (!PlayerControl.LocalPlayer.Data.IsDead)
                        {
                            FollowerCameraShakeAmount = 0.03f;
                            FollowerCameraShakePeriod = 400f;

                            _lowerBlackoutMat.SetFloat(_alpha, 1);
                            _upperBlackoutMat.SetFloat(_alpha, 1);

                            SetMeshMovement(lowerElevator, _lowerMesh, 0);
                            SetMeshMovement(upperElevator, _upperMesh, 0);
                        }
                        else
                        {
                            FollowerCameraShakeAmount = 0f;
                            FollowerCameraShakePeriod = 0f;
                            _lowerBlackoutMat.SetFloat(_alpha, 0);
                            _upperBlackoutMat.SetFloat(_alpha, 0);

                            SetMeshMovement(lowerElevator, _lowerMesh, system.upperDeckIsTargetFloor ? 0 : 1);
                            SetMeshMovement(upperElevator, _upperMesh, !system.upperDeckIsTargetFloor ? 0 : 1);
                        }
                    }
                    else
                    {
                        FollowerCameraShakeAmount = 0f;
                        FollowerCameraShakePeriod = 0f;
                        _lowerBlackoutMat.SetFloat(_alpha, 0);
                        _upperBlackoutMat.SetFloat(_alpha, 0);

                        // Elevator is invisible on both floors
                        SetMeshMovement(lowerElevator, _lowerMesh, -1);
                        SetMeshMovement(upperElevator, _upperMesh, 1);
                    }

                    break;
                }
                case ElevatorMovementStage.ElevatorMovingIn:
                {
                    if (!_movingSound.isPlaying) _movingSound.Play();

                    // Doors done moving so elevator doors replace them
                    lowerInnerRend.Value.enabled = false;
                    upperInnerRend.Value.enabled = false;

                    lowerElevator.Value.material.SetTexture(_mainTex, system.upperDeckIsTargetFloor ? elevatorWithDoorL2 : elevatorWithDoorL1);
                    upperElevator.Value.material.SetTexture(_mainTex, system.upperDeckIsTargetFloor ? elevatorWithDoorL2 : elevatorWithDoorL1);

                    if (LocalPlayerInElevator || PlayerControl.LocalPlayer.Data.IsDead)
                    {
                        if (!PlayerControl.LocalPlayer.Data.IsDead)
                        {
                            FollowerCameraShakeAmount = 0.03f;
                            FollowerCameraShakePeriod = 400f;

                            _lowerBlackoutMat.SetFloat(_alpha, 1);
                            _upperBlackoutMat.SetFloat(_alpha, 1);

                            SetMeshMovement(lowerElevator, _lowerMesh, 0);
                            SetMeshMovement(upperElevator, _upperMesh, 0);
                        }
                        else
                        {
                            FollowerCameraShakeAmount = 0f;
                            FollowerCameraShakePeriod = 0f;
                            _lowerBlackoutMat.SetFloat(_alpha, 0);
                            _upperBlackoutMat.SetFloat(_alpha, 0);

                            SetMeshMovement(lowerElevator, _lowerMesh, system.upperDeckIsTargetFloor ? 0 : 1);
                            SetMeshMovement(upperElevator, _upperMesh, !system.upperDeckIsTargetFloor ? 0 : 1);
                        }
                    }
                    else
                    {
                        FollowerCameraShakeAmount = 0f;
                        FollowerCameraShakePeriod = 0f;

                        _lowerBlackoutMat.SetFloat(_alpha, 0);
                        _upperBlackoutMat.SetFloat(_alpha, 0);

                        // Elevator on target floor is lerping movement, other one is invisible
                        SetMeshMovement(lowerElevator, _lowerMesh, system.upperDeckIsTargetFloor ? -1 : -1 + GetMovementLerp(stage));
                        SetMeshMovement(upperElevator, _upperMesh, !system.upperDeckIsTargetFloor ? 1 : 1 - GetMovementLerp(stage));
                    }

                    break;
                }
                case ElevatorMovementStage.FadingToClear:
                {
                    if (!_movingSound.isPlaying) _movingSound.Play();

                    if (_dingSound == null)
                    {
                        _dingSound = SoundManager.Instance.PlayDynamicSound($"ding_{name}_{GetInstanceID()}",
                            SubmarineStatus.instance.minigameProperties.audioClips[3],
                            false,
                            new Action<AudioSource, float>(DynamicDingSound),
                            SoundManager.Instance.SfxChannel);
                    }

                    FollowerCameraShakeAmount = 0f;
                    FollowerCameraShakePeriod = 0f;

                    // Doors done moving so elevator doors replace them
                    lowerInnerRend.Value.enabled = false;
                    upperInnerRend.Value.enabled = false;

                    lowerElevator.Value.material.SetTexture(_mainTex, system.upperDeckIsTargetFloor ? elevatorWithDoorL2 : elevatorWithDoorL1);
                    upperElevator.Value.material.SetTexture(_mainTex, system.upperDeckIsTargetFloor ? elevatorWithDoorL2 : elevatorWithDoorL1);

                    if (LocalPlayerInElevator || PlayerControl.LocalPlayer.Data.IsDead)
                    {
                        if (!PlayerControl.LocalPlayer.Data.IsDead)
                        {
                            _lowerBlackoutMat.SetFloat(_alpha, LocalPlayerInElevator ? 1 - GetMovementLerp(stage) : 0);
                            _upperBlackoutMat.SetFloat(_alpha, LocalPlayerInElevator ? 1 - GetMovementLerp(stage) : 0);

                            SetMeshMovement(lowerElevator, _lowerMesh, 0);
                            SetMeshMovement(upperElevator, _upperMesh, 0);
                        }
                        else
                        {
                            _lowerBlackoutMat.SetFloat(_alpha, 0);
                            _upperBlackoutMat.SetFloat(_alpha, 0);

                            SetMeshMovement(lowerElevator, _lowerMesh, !system.upperDeckIsTargetFloor ? 0 : 1);
                            SetMeshMovement(upperElevator, _upperMesh, system.upperDeckIsTargetFloor ? 0 : 1);
                        }
                    }
                    else
                    {
                        _lowerBlackoutMat.SetFloat(_alpha, 0);
                        _upperBlackoutMat.SetFloat(_alpha, 0);
                        // Elevator is on the floor so 0 for current floor
                        SetMeshMovement(lowerElevator, _lowerMesh, system.upperDeckIsTargetFloor ? -1 : 0);
                        SetMeshMovement(upperElevator, _upperMesh, !system.upperDeckIsTargetFloor ? 1 : 0);
                    }

                    break;
                }
                case ElevatorMovementStage.DoorsOpening:
                {
                    _dingSound = null;
                    FollowerCameraShakeAmount = 0f;
                    FollowerCameraShakePeriod = 0f;

                    // Real inner doors are moving so elevator doors are hidden
                    lowerInnerRend.Value.enabled = !system.upperDeckIsTargetFloor;
                    upperInnerRend.Value.enabled = system.upperDeckIsTargetFloor;

                    lowerElevator.Value.material.SetTexture(_mainTex, !system.upperDeckIsTargetFloor ? elevatorWithoutDoorL2 : elevatorWithoutDoorL1);
                    upperElevator.Value.material.SetTexture(_mainTex, !system.upperDeckIsTargetFloor ? elevatorWithoutDoorL2 : elevatorWithoutDoorL1);

                    _lowerBlackoutMat.SetFloat(_alpha, 0);
                    _upperBlackoutMat.SetFloat(_alpha, 0);

                    // Elevator is on the floor so 0 for current floor
                    SetMeshMovement(lowerElevator, _lowerMesh, system.upperDeckIsTargetFloor ? -1 : 0);
                    SetMeshMovement(upperElevator, _upperMesh, !system.upperDeckIsTargetFloor ? 1 : 0);

                    lowerOuterDoor.Value.SetDoorway(!system.upperDeckIsTargetFloor);
                    lowerInnerDoor.Value.SetDoorway(!system.upperDeckIsTargetFloor);

                    upperInnerDoor.Value.SetDoorway(system.upperDeckIsTargetFloor);
                    upperOuterDoor.Value.SetDoorway(system.upperDeckIsTargetFloor);

                    lowerLight.Value.enabled = !system.upperDeckIsTargetFloor;
                    upperLight.Value.enabled = system.upperDeckIsTargetFloor;

                    system.moving = upperInnerDoor.Value.animator.Playing || lowerInnerDoor.Value.animator.IsPlaying();

                    break;
                }
            }
        }
    }

    private Vector2 GetTargetSyncPosition(ICG.Queue<Vector2> incomingPosQueue)
    {
        int index = incomingPosQueue._tail - 1;
        if (index == -1) index = incomingPosQueue._array.Length - 1;
        return incomingPosQueue._array[index];
    }

    public void OnDestroy()
    {
        SoundManager.Instance.StopNamedSound($"ding_{name}_{GetInstanceID()}");
    }

    [HideFromIl2Cpp]
    public bool GetInElevator(PlayerControl player)
    {
        Vector3 pos;

        if (player.AmOwner)
        {
            pos = player.transform.position;
        }
        else
        {
            pos = GetTargetSyncPosition(player.NetTransform.incomingPosQueue);
        }

        return lowerElevatorCollider.Value.OverlapPoint(pos) || upperElevatorCollider.Value.OverlapPoint(pos);
    }

    [HideFromIl2Cpp]
    public bool CheckInElevator(Vector2 position) => lowerElevatorCollider.Value.OverlapPoint(position) || upperElevatorCollider.Value.OverlapPoint(position);

    [HideFromIl2Cpp]
    private float GetMovementLerp(int stage)
    {
        try
        {
            return Mathf.Clamp01(system.lerpTimer / _individualTimings[stage]);
        }
        catch (Exception e)
        {
            Error(e);
            return 0;
        }
    }

    [HideFromIl2Cpp]
    private float GetMovementLerp(ElevatorMovementStage stage) => GetMovementLerp((int) stage);

    [HideFromIl2Cpp]
    public ElevatorMovementStage GetMovementStageFromSystem() => system.lastStage;

    [HideFromIl2Cpp]
    public ElevatorMovementStage GetMovementStageFromTime()
    {
        float sum = 0;

        for (int i = 0; i < _individualTimings.Length; i++)
        {
            sum += _individualTimings[i];

            if (system.totalTimer <= sum)
            {
                return (ElevatorMovementStage) i;
            }
        }

        return ElevatorMovementStage.DoorsOpening;
    }

    [HideFromIl2Cpp]
    public void GetPlayersInElevator(List<PlayerControl> players)
    {
        players.Clear();

        foreach (GameData.PlayerInfo playerInfo in GameData.Instance.AllPlayers.GetFastEnumerator())
        {
            if (playerInfo.IsDead || playerInfo.Disconnected) continue;
            PlayerControl player = playerInfo.Object;

            if (GetInElevator(player)) players.Add(player);
        }
    }

    public void SetMeshMovement(MeshRenderer rend, Mesh mesh, float amount)
    {
        float alpha = 1 - (Mathf.Clamp(Mathf.Abs(amount), 0.25f, 0.75f) - 0.25f) / 0.5f;
        Color alphaColor = new(1, 1, 1, alpha);

        for (int i = 0; i < _uvCount; i++)
        {
            Vector2 uv = _originalUVs[i];
            uv.y += amount;
            _newUVs[i] = uv;
        }

        for (int i = 0; i < _colorCount; i++)
        {
            _vertexColors[i] = alphaColor;
        }

        rend.material.SetColor(_colorPropertyID, new Color(1, 1, 1, alpha));
        mesh.SetUVs(0, _newUVs);
        mesh.SetColors(_vertexColors);
    }

    public void Use()
    {
        ShipStatus.Instance.RpcUpdateSystem(system.systemTypes, 2);
    }

    private void DynamicMoveSound(AudioSource audioSource, float dt)
    {
        if (!PlayerControl.LocalPlayer)
        {
            audioSource.volume = 0f;

            return;
        }

        const float FALL_OFF = 2.8f;

        Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
        float distanceLower = Vector2.Distance(truePosition, lowerElevator.Value.transform.position);
        float distanceUpper = Vector2.Distance(truePosition, upperElevator.Value.transform.position);

        float distance = Mathf.Min(distanceLower, distanceUpper);
        float volume = Mathf.Lerp(0, 1, 1 - distance / FALL_OFF);
        audioSource.volume = volume * 0.5f;
    }

    private void DynamicDingSound(AudioSource audioSource, float dt)
    {
        if (!PlayerControl.LocalPlayer)
        {
            audioSource.volume = 0f;

            return;
        }

        Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();

        float distance = Vector2.Distance(truePosition, system.upperDeckIsTargetFloor ? upperElevator.Value.transform.position : lowerElevator.Value.transform.position);

        float volume;

        switch (distance)
        {
            case <= 2.5f:
                volume = 1;

                break;
            case <= 4f:
                volume = Mathf.Lerp(1, 0, (distance - 2.5f) / 1.5f);

                break;
            default:
                volume = 0;

                break;
        }

        audioSource.volume = volume * 0.5f;

        if (CheckInElevator(truePosition))
        {
            audioSource.volume *= 0.5f;
        }
    }

    // ReSharper disable UnassignedField.Global
    public Il2CppReferenceField<MeshRenderer> lowerElevator;
    public Il2CppReferenceField<MeshFilter> lowerElevatorFilter;
    public Il2CppReferenceField<MeshRenderer> upperElevator;
    public Il2CppReferenceField<MeshFilter> upperElevatorFilter;

    public Il2CppReferenceField<BoxCollider2D> lowerElevatorCollider;
    public Il2CppReferenceField<BoxCollider2D> upperElevatorCollider;

    public Il2CppReferenceField<SpriteRenderer> lowerBlackout;
    public Il2CppReferenceField<SpriteRenderer> upperBlackout;

    public Il2CppReferenceField<Texture> elevatorWithoutDoorL1;
    public Il2CppReferenceField<Texture> elevatorWithDoorL1;

    public Il2CppReferenceField<Texture> elevatorWithoutDoorL2;
    public Il2CppReferenceField<Texture> elevatorWithDoorL2;

    public Il2CppReferenceField<PlainDoor> lowerOuterDoor;
    public Il2CppReferenceField<PlainDoor> lowerInnerDoor;
    public Il2CppReferenceField<SpriteRenderer> lowerInnerRend;

    public Il2CppReferenceField<PlainDoor> upperOuterDoor;
    public Il2CppReferenceField<PlainDoor> upperInnerDoor;
    public Il2CppReferenceField<SpriteRenderer> upperInnerRend;

    public Il2CppReferenceField<SpriteRenderer> lowerLight;

    public Il2CppReferenceField<SpriteRenderer> upperLight;
    // ReSharper restore UnassignedField.Global
}
