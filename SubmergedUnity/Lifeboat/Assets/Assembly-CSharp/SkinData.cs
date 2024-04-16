using System;
using UnityEngine;

[CreateAssetMenu]
public class SkinData : ScriptableObject, IBuyable
{
	public Sprite IdleFrame;

	public AnimationClip IdleAnim;

	public AnimationClip RunAnim;

	public AnimationClip EnterVentAnim;

	public AnimationClip ExitVentAnim;

	public AnimationClip ClimbAnim;

	public AnimationClip ClimbDownAnim;

	public AnimationClip KillTongueImpostor;

	public AnimationClip KillTongueVictim;

	public AnimationClip KillShootImpostor;

	public AnimationClip KillShootVictim;

	public AnimationClip KillNeckImpostor;

	public AnimationClip KillStabImpostor;

	public AnimationClip KillStabVictim;

	public AnimationClip KillNeckVictim;

	public AnimationClip KillRHMVictim;

	public Sprite EjectFrame;

	public AnimationClip SpawnAnim;

	public AnimationClip IdleLeftAnim;

	public AnimationClip RunLeftAnim;

	public AnimationClip EnterLeftVentAnim;

	public AnimationClip ExitLeftVentAnim;

	public AnimationClip SpawnLeftAnim;

	public OverlayKillAnimation[] KillAnims;

	public bool NotInStore;

	public bool Free;

	public HatBehaviour RelatedHat;

	public string StoreName;

	public string ProductId;

	public int Order;

	public string ProdId
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(this.ProductId))
			{
				return this.ProductId;
			}
			if (this.RelatedHat)
			{
				return this.RelatedHat.ProdId;
			}
			return null;
		}
	}
}
