using System;
using System.Collections;
using PowerTools;
using UnityEngine;

public class KillAnimation : MonoBehaviour
{
	public AnimationClip BlurAnim;

	public DeadBody bodyPrefab;

	public Vector3 BodyOffset;

	public IEnumerator CoPerformKill(PlayerControl source, PlayerControl target)
	{
		FollowerCamera cam = Camera.main.GetComponent<FollowerCamera>();
		bool isParticipant = PlayerControl.LocalPlayer == source || PlayerControl.LocalPlayer == target;
		PlayerPhysics sourcePhys = source.MyPhysics;
		KillAnimation.SetMovement(source, false);
		KillAnimation.SetMovement(target, false);
		DeadBody deadBody = UnityEngine.Object.Instantiate<DeadBody>(this.bodyPrefab);
		deadBody.enabled = false;
		deadBody.ParentId = target.PlayerId;
		target.SetPlayerMaterialColors(deadBody.bodyRenderer);
		target.SetPlayerMaterialColors(deadBody.bloodSplatter);
		Vector3 vector = target.transform.position + this.BodyOffset;
		vector.z = vector.y / 1000f;
		deadBody.transform.position = vector;
		if (isParticipant)
		{
			cam.Locked = true;
			ConsoleJoystick.SetMode_Task();
			if (PlayerControl.LocalPlayer.AmOwner)
			{
				PlayerControl.LocalPlayer.MyPhysics.inputHandler.enabled = true;
			}
		}
		target.Die(DeathReason.Kill);
		SpriteAnim sourceAnim = source.MyAnim;
		yield return new WaitForAnimationFinish(sourceAnim, this.BlurAnim);
		source.NetTransform.SnapTo(target.transform.position);
		sourceAnim.Play(sourcePhys.IdleAnim, 1f);
		KillAnimation.SetMovement(source, true);
		KillAnimation.SetMovement(target, true);
		deadBody.enabled = true;
		if (isParticipant)
		{
			cam.Locked = false;
		}
		yield break;
	}

	public static void SetMovement(PlayerControl source, bool canMove)
	{
		source.moveable = canMove;
		source.MyPhysics.ResetMoveState(false);
		source.NetTransform.enabled = canMove;
		source.MyPhysics.enabled = canMove;
		source.NetTransform.Halt();
	}
}
