using Submerged.Floors;
using UnityEngine;
using static Submerged.Vents.VentPatchData;

namespace Submerged.Vents;

public static class CrossFloorVentMovement
{
    public static IEnumerator HandleCentralMove(Vector2 targetPos)
    {
        Vector2 startPos = PlayerControl.LocalPlayer.transform.position;
        bool movingDown = startPos.y > targetPos.y;

        if (movingDown)
        {
            yield return MovementAnimation(targetPos + new Vector2(0, -0.5f), true, 3, 6.1963f, -0.7163f);
        }
        else
        {
            yield return MovementAnimation(targetPos + new Vector2(0, 0.25f), false, -3, 6.1827f);
        }
    }

    public static IEnumerator HandleMove(Vector2 targetPos)
    {
        Vector2 startPos = PlayerControl.LocalPlayer.transform.position;
        bool movingDown = startPos.y > targetPos.y;

        if (movingDown)
        {
            yield return MovementAnimation(targetPos + new Vector2(0, -0.5f), true, 3, 6.1963f);
        }
        else
        {
            yield return MovementAnimation(targetPos + new Vector2(0, 0.25f), false, -3, 6.1827f);
        }
    }

    private static IEnumerator MovementAnimation(Vector2 targetPos, bool originalFloor, float cameraSpeedY, float cameraTargetX, float movingDownRpcSnapToYOffset = 0)
    {
        FollowerCamera cam = HudManager.Instance.PlayerCam;
        Transform camTransform = cam.transform;
        FloorHandler floorHandler = FloorHandler.LocalPlayer;

        cam.Locked = InTransition = true;

        const float DURATION = 0.15f;

        // Camera animation
        for (float t = 0; t <= DURATION; t += Time.deltaTime)
        {
            Vector3 pos2 = camTransform.position;
            pos2.y -= cameraSpeedY * Time.deltaTime;
            camTransform.position = pos2;

            yield return null;
        }

        // Camera snaps to other floor
        targetPos.y += cameraSpeedY * DURATION;
        cam.transform.position = targetPos;

        // Teleport player to other floor
        floorHandler.RegisterCamXOverride(cameraTargetX);
        floorHandler.RegisterFloorOverride(!originalFloor);
        floorHandler.RpcRequestChangeFloor(!originalFloor);

        // Teleport player to same position despite floor X difference
        if (!originalFloor)
        {
            PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(targetPos - new Vector2(0, FloorHandler.MAP_OFFSET + movingDownRpcSnapToYOffset));
        }
        else
        {
            PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(targetPos + new Vector2(0, FloorHandler.MAP_OFFSET));
        }

        // Camera finish animation
        for (float t = 0; t <= DURATION; t += Time.deltaTime)
        {
            Vector3 pos2 = camTransform.position;
            pos2.x = cameraTargetX;
            pos2.y -= cameraSpeedY * Time.deltaTime;
            camTransform.position = pos2;

            yield return null;
        }

        cam.Locked = InTransition = false;
        cam.centerPosition = cam.transform.position;
    }
}
