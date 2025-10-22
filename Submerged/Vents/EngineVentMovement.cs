using BepInEx.Unity.IL2CPP.Utils;
using PowerTools;
using Reactor.Networking.Attributes;
using Submerged.Enums;
using Submerged.Extensions;
using Submerged.Floors;
using UnityEngine;
using static Submerged.Vents.VentPatchData;

namespace Submerged.Vents;

public static class EngineVentMovement
{
    public static IEnumerator HandleMove(Vector2 targetPos)
    {
        FollowerCamera cam = HudManager.Instance.PlayerCam;
        Transform camTransform = cam.transform;
        FloorHandler floorHandler = FloorHandler.LocalPlayer;

        InTransition = true;

        Vector2 camPos = targetPos + new Vector2(0, 0.70886f);
        targetPos += new Vector2(0, -0.4f);

        cam.centerPosition = camTransform.position = camPos;

        floorHandler.RegisterFloorOverride(false);

        PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(targetPos);
        floorHandler.RpcRequestChangeFloor(false);

        RpcEngineVent(PlayerControl.LocalPlayer);

        yield return PublicHandleMove(PlayerControl.LocalPlayer);

        InTransition = false;
    }

    public static IEnumerator PublicHandleMove(PlayerControl player)
    {
        PlayerPhysics physics = player.MyPhysics;

        while (FloorHandler.GetFloorHandler(player).onUpper)
        {
            yield return null;
        }

        player.Visible = true;
        player.cosmetics.currentPet.Visible = false;

        CosmeticsLayer cosmetics = player.cosmetics;
        SkinLayer skin = cosmetics.skin;

        skin.SetExitVent(cosmetics.currentBodySprite.BodySprite.flipX);

        yield return new WaitForAnimationFinish(physics.Animations.Animator, physics.Animations.group.ExitVentAnim);
        skin.SetIdle(cosmetics.currentBodySprite.BodySprite.flipX);
        physics.Animations.Animator.Play(physics.Animations.group.IdleAnim);
    }

    [MethodRpc(CustomRpcCalls.EngineVent)]
    public static void RpcEngineVent(PlayerControl player)
    {
        if (!ShipStatus.Instance.IsSubmerged()) return;
        player.StartCoroutine(PublicHandleMove(player));
    }
}
