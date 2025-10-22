using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Attributes;
using Submerged.Enums;
using Submerged.Extensions;
using Submerged.Map;
using UnityEngine;

namespace Submerged.Floors;

[RegisterInIl2Cpp]
public sealed class FloorHandler(nint ptr) : MonoBehaviour(ptr)
{
    public const float MAP_OFFSET = 48.119f;
    public const float FLOOR_CUTOFF = -6.19f;

    public static FloorHandler LocalPlayer => GetFloorHandler(PlayerControl.LocalPlayer);

    private static readonly Dictionary<int, FloorHandler> _hashCodeToFloorHandler = new();
    public int lastSid;

    public bool onUpper;
    public bool disable;

    private bool? _overrideOnUpper;
    private float? _overrideCameraX;

    private FollowerCamera _followerCam;
    private Transform _followerCamTransform;

    private PlayerControl _player;
    private Transform _transform;

    private PlayerControl Player
    {
        get => _player ? _player : _player = GetComponent<PlayerControl>();
        set => _player = value;
    }

    private void Awake()
    {
        Player = GetComponent<PlayerControl>();
        _transform = Player.transform;

        if (Player.GetComponent<DummyBehaviour>().enabled)
        {
            if (Player.transform.position.y > FLOOR_CUTOFF)
            {
                onUpper = true;
                SubmarinePlayerFloorSystem.Instance.ChangePlayerFloorState(Player.PlayerId, true);
            }
        }
    }

    private void Start()
    {
        CleanCache();

        _followerCam = HudManager.Instance.PlayerCam;
        _followerCamTransform = _followerCam.transform;
    }

    private void FixedUpdate()
    {
        LateUpdate();
    }

    public void LateUpdate()
    {
        if (!PlayerControl.LocalPlayer) return;
        if (!ShipStatus.Instance || !SubmarineStatus.instance) return;
        if (disable) return;

        UpdateFloor();
        Vector3 position = _transform.position;
        bool changed = false;
        Vector3 camOffset = _followerCamTransform.position - position;
        Vector3 camCenterOffset = _followerCam.centerPosition - (Vector2) position;

        if (position.y > FLOOR_CUTOFF)
        {
            if (!onUpper)
            {
                position.y -= MAP_OFFSET;
                changed = true;
            }
        }
        else
        {
            if (onUpper)
            {
                position.y += MAP_OFFSET;
                changed = true;
            }
        }

        position.z = position.y / 1000f;

        if (changed)
        {
            _transform.position = position;
            Player.NetTransform.SnapTo(position);
            // Player.NetTransform.targetSyncPosition = position;

            if (Player.lightSource)
            {
                Player.lightSource.Update();
            }

            if (Player.AmOwner)
            {
                if (!Player.inVent)
                {
                    _followerCam.centerPosition = position + camCenterOffset;
                    _followerCamTransform.position = position + camOffset;
                }
                else
                {
                    _followerCam.centerPosition = _followerCamTransform.position = position + camCenterOffset;

                    if (_overrideCameraX != null)
                    {
                        Vector3 modifiedPos = _followerCam.centerPosition;

                        _followerCam.centerPosition = _followerCamTransform.position
                            = new Vector3(_overrideCameraX.Value, modifiedPos.y, modifiedPos.z);

                        _overrideCameraX = null;
                    }
                }
            }
        }
    }

    private static FloorHandler GetFloorHandler(Component comp)
    {
        if (!comp) return null;

        int hashCode = comp.GetHashCode();

        if (_hashCodeToFloorHandler.TryGetValue(hashCode, out FloorHandler handler)) return handler;

        handler = comp.gameObject.EnsureComponent<FloorHandler>();
        _hashCodeToFloorHandler.Add(hashCode, handler);

        return handler;
    }

    public static FloorHandler GetFloorHandler(PlayerControl comp) => GetFloorHandler((Component) comp);

    public static FloorHandler GetFloorHandler(PlayerPhysics comp) => GetFloorHandler((Component) comp);

    public static FloorHandler GetFloorHandler(CustomNetworkTransform comp) => GetFloorHandler((Component) comp);

    public void RpcRequestChangeFloor(bool toUpper)
    {
        onUpper = toUpper;

        if (AmongUsClient.Instance.AmHost)
        {
            SubmarinePlayerFloorSystem.Instance.ChangePlayerFloorState(Player.PlayerId, toUpper);

            return;
        }

        RpcRequestChangeFloor(PlayerControl.LocalPlayer, toUpper, lastSid++);
    }

    public void UpdateFloor()
    {
        SubmarinePlayerFloorSystem.Instance.playerFloorStates.TryGetValue(Player.PlayerId, out onUpper);

        if (Player.PlayerId == PlayerControl.LocalPlayer.PlayerId)
        {
            if (_overrideOnUpper != null)
            {
                if (onUpper == _overrideOnUpper)
                    _overrideOnUpper = null;
                else
                    onUpper = _overrideOnUpper.Value;
            }
        }
    }

    public void RegisterFloorOverride(bool overrideFloor)
    {
        if (!_player.AmOwner) throw new InvalidOperationException("Only the local player can override their floor state.");
        _overrideOnUpper = overrideFloor;
    }

    public void RegisterCamXOverride(float overrideCamX)
    {
        if (!_player.AmOwner) throw new InvalidOperationException("Only the local player can override their camera position.");
        _overrideCameraX = overrideCamX;
    }

    public void ClearOverrides()
    {
        if (!_player.AmOwner) throw new InvalidOperationException("Only the local player can use floor handler overrides.");
        _overrideOnUpper = null;
        _overrideCameraX = null;
    }

    private static void CleanCache()
    {
        Span<int> hashCodesToRemove = stackalloc int[_hashCodeToFloorHandler.Count];
        int count = 0;

        foreach ((int hashCode, FloorHandler floorHandler) in _hashCodeToFloorHandler)
        {
            try
            {
                if (floorHandler) continue;
            }
            catch (ObjectCollectedException)
            {
                // ignore
            }

            hashCodesToRemove[count++] = hashCode;
        }

        for (int i = 0; i < count; i++)
        {
            _hashCodeToFloorHandler.Remove(hashCodesToRemove[i]);
        }
    }

    [MethodRpc(CustomRpcCalls.RequestChangeFloor)]
    public static void RpcRequestChangeFloor(PlayerControl player, bool toUpper, int sid)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        SubmarinePlayerFloorSystem floorSystem = SubmarinePlayerFloorSystem.Instance;

        if (!floorSystem.playerFloorSids.ContainsKey(player.PlayerId) || floorSystem.playerFloorSids[player.PlayerId] <= sid)
        {
            floorSystem.playerFloorSids[player.PlayerId] = sid;
            floorSystem!.ChangePlayerFloorState(player.PlayerId, toUpper);
        }
    }
}
