using System;
using UnityEngine;

public class DeadBody : MonoBehaviour
{
	public bool Reported;

	public byte ParentId;

	public Collider2D myCollider;

	public SpriteRenderer bloodSplatter;

	public SpriteRenderer bodyRenderer;

	public Vector2 TruePosition
	{
		get
		{
			return base.transform.TransformPoint(this.myCollider.offset);
		}
	}

	public void OnClick()
	{
		if (this.Reported)
		{
			return;
		}
		Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
		Vector2 truePosition2 = this.TruePosition;
		if (Vector2.Distance(truePosition2, truePosition) <= PlayerControl.LocalPlayer.MaxReportDistance && PlayerControl.LocalPlayer.CanMove && !PhysicsHelpers.AnythingBetween(truePosition, truePosition2, Constants.ShipAndObjectsMask, false))
		{
			this.Reported = true;
			GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById(this.ParentId);
			PlayerControl.LocalPlayer.CmdReportDeadBody(playerById);
		}
	}
}
