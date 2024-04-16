using System;
using System.Collections;
using System.Linq;
using Hazel;
using InnerNet;
using PowerTools;
using UnityEngine;

public class PlayerPhysics : InnerNetObject
{
	private byte lastClimbLadderSid;

	public float Speed = 4.5f;

	public float GhostSpeed = 3f;

	[HideInInspector]
	private Rigidbody2D body;

	[HideInInspector]
	private PlayerControl myPlayer;

	[SerializeField]
	private SpriteAnim Animator;

	[SerializeField]
	private SpriteAnim GlowAnimator;

	[SerializeField]
	private SpriteRenderer rend;

	public AnimationClip RunAnim;

	public AnimationClip IdleAnim;

	public AnimationClip GhostIdleAnim;

	public AnimationClip EnterVentAnim;

	public AnimationClip ExitVentAnim;

	public AnimationClip SpawnAnim;

	public AnimationClip SpawnGlowAnim;

	public AnimationClip ClimbAnim;

	public AnimationClip ClimbDownAnim;

	public SkinLayer Skin;

	public AudioClip ImpostorDiscoveredSound;

	[NonSerialized]
	public SpecialInputHandler inputHandler;

	public void RpcClimbLadder(Ladder source)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			this.ClimbLadder(source, (byte) (this.lastClimbLadderSid + 1));
		}
		else
		{
			this.lastClimbLadderSid += 1;
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(this.NetId, 31, SendOption.Reliable);
		messageWriter.Write(source.Id);
		messageWriter.Write(this.lastClimbLadderSid);
		messageWriter.EndMessage();
	}

	public void RpcEnterVent(int id)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			base.StopAllCoroutines();
			base.StartCoroutine(this.CoEnterVent(id));
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(this.NetId, 19, SendOption.Reliable);
		messageWriter.WritePacked(id);
		messageWriter.EndMessage();
	}

	public void RpcExitVent(int id)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			base.StopAllCoroutines();
			base.StartCoroutine(this.CoExitVent(id));
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(this.NetId, 20, SendOption.Reliable);
		messageWriter.WritePacked(id);
		messageWriter.EndMessage();
	}

	public void RpcBootFromVent(int ventId)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			this.BootFromVent(ventId);
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(this.NetId, 34);
		messageWriter.WritePacked(ventId);
		messageWriter.EndMessage();
	}

	public override void HandleRpc(byte callId, MessageReader reader)
	{
		if (callId <= 20)
		{
			if (callId == 19)
			{
				int id = reader.ReadPackedInt32();
				base.StopAllCoroutines();
				base.StartCoroutine(this.CoEnterVent(id));
				return;
			}
			if (callId != 20)
			{
				return;
			}
			int id2 = reader.ReadPackedInt32();
			base.StopAllCoroutines();
			base.StartCoroutine(this.CoExitVent(id2));
			return;
		}
		else
		{
			if (callId == 31)
			{
				AirshipStatus airshipStatus = (AirshipStatus)ShipStatus.Instance;
				byte ladderId = reader.ReadByte();
				byte climbLadderSid = reader.ReadByte();
				this.ClimbLadder(airshipStatus.Ladders.First((Ladder f) => f.Id == ladderId), climbLadderSid);
				return;
			}
			if (callId != 34)
			{
				return;
			}
			int ventId = reader.ReadPackedInt32();
			this.BootFromVent(ventId);
			return;
		}
	}

	public float TrueSpeed
	{
		get
		{
			return this.Speed * PlayerControl.GameOptions.PlayerSpeedMod;
		}
	}

	public float TrueGhostSpeed
	{
		get
		{
			return this.GhostSpeed * PlayerControl.GameOptions.PlayerSpeedMod;
		}
	}

	public void Awake()
	{
		this.body = base.GetComponent<Rigidbody2D>();
		this.myPlayer = base.GetComponent<PlayerControl>();
		if (this.inputHandler == null)
		{
			this.inputHandler = base.gameObject.AddComponent<SpecialInputHandler>();
			this.inputHandler.disableVirtualCursor = true;
			this.inputHandler.enabled = false;
		}
	}

	public void EnableInterpolation()
	{
		this.body.interpolation = RigidbodyInterpolation2D.Interpolate;
	}

	private void FixedUpdate()
	{	
		GameData.PlayerInfo data = this.myPlayer.Data;
		bool flag = data != null && data.IsDead;
		this.HandleAnimation(flag);
		if (base.AmOwner && this.myPlayer.CanMove && GameData.Instance)
		{
			this.body.velocity = DestroyableSingleton<HudManager>.Instance.joystick.Delta * (flag ? this.TrueGhostSpeed : this.TrueSpeed);
		}
	}

	private void LateUpdate()
	{
		Vector3 position = base.transform.position;
		position.z = position.y / 1000f;
		base.transform.position = position;
	}

	public Vector3 Vec2ToPosition(Vector2 pos)
	{
		return new Vector3(pos.x, pos.y, pos.y / 1000f);
	}

	public void SetSkin(uint skinId)
	{
		this.Skin.SetSkin(skinId, this.rend.flipX);
		if (this.Animator.IsPlaying(this.SpawnAnim))
		{
			this.Skin.SetSpawn(this.rend.flipX, this.Animator.Time);
		}
	}

	private void StartClimb(bool down)
	{
		this.rend.flipX = false;
		this.Skin.Flipped = false;
		this.Animator.Play(down ? this.ClimbDownAnim : this.ClimbAnim, 1f);
		this.Animator.Time = 0f;
		this.Skin.SetClimb(down);
		this.myPlayer.HatRenderer.SetClimbAnim();
		this.myPlayer.CurrentPet.Visible = false;
	}

	private void ClimbLadder(Ladder source, byte climbLadderSid)
	{
		if (!NetHelpers.SidGreaterThan(climbLadderSid, this.lastClimbLadderSid))
		{
			return;
		}
		this.lastClimbLadderSid = climbLadderSid;
		this.ResetMoveState(true);
		base.StartCoroutine(this.CoClimbLadder(source, climbLadderSid));
	}

	private IEnumerator CoClimbLadder(Ladder source, byte climbLadderSid)
	{
		this.myPlayer.Collider.enabled = false;
		this.myPlayer.moveable = false;
		this.myPlayer.NetTransform.enabled = false;
		if (this.myPlayer.AmOwner)
		{
			this.myPlayer.MyPhysics.inputHandler.enabled = true;
		}
		yield return this.WalkPlayerTo(source.transform.position, 0.001f, 1f);
		yield return Effects.Wait(0.1f);
		this.StartClimb(source.IsTop);
		if (Constants.ShouldPlaySfx() && PlayerControl.LocalPlayer == this.myPlayer)
		{
			this.myPlayer.FootSteps.clip = source.UseSound;
			this.myPlayer.FootSteps.loop = true;
			this.myPlayer.FootSteps.Play();
		}
		yield return this.WalkPlayerTo(source.Destination.transform.position, 0.001f, (float)(source.IsTop ? 2 : 1));
		this.myPlayer.CurrentPet.transform.position = this.myPlayer.transform.position;
		this.ResetAnimState();
		yield return Effects.Wait(0.1f);
		this.myPlayer.Collider.enabled = true;
		this.myPlayer.moveable = true;
		this.myPlayer.NetTransform.enabled = true;
		yield break;
	}

	public void ResetMoveState(bool stopCoroutines = true)
	{
		if (stopCoroutines)
		{
			this.myPlayer.StopAllCoroutines();
			base.StopAllCoroutines();
			if (this.inputHandler && this.inputHandler.enabled)
			{
				this.inputHandler.enabled = false;
			}
		}
		base.enabled = true;
		this.myPlayer.inVent = false;
		this.myPlayer.Visible = true;
		GameData.PlayerInfo data = this.myPlayer.Data;
		this.myPlayer.Collider.enabled = (data == null || !data.IsDead);
		this.ResetAnimState();
	}

	public void ResetAnimState()
	{
		this.myPlayer.FootSteps.Stop();
		this.myPlayer.FootSteps.loop = false;
		this.myPlayer.HatRenderer.SetIdleAnim();
		GameData.PlayerInfo data = this.myPlayer.Data;
		if (data != null)
		{
			this.myPlayer.HatRenderer.SetColor(this.myPlayer.Data.ColorId);
		}
		if (data == null || !data.IsDead)
		{
			this.Skin.SetIdle(this.rend.flipX);
			this.Animator.Play(this.IdleAnim, 1f);
			this.myPlayer.Visible = true;
			this.myPlayer.SetHatAlpha(1f);
			return;
		}
		this.Skin.SetGhost();
		this.Animator.Play(this.GhostIdleAnim, 1f);
		this.myPlayer.SetHatAlpha(0.5f);
	}

	private void HandleAnimation(bool amDead)
	{
		if (this.Animator.IsPlaying(this.SpawnAnim))
		{
			return;
		}
		if (!GameData.Instance)
		{
			return;
		}
		Vector2 velocity = this.body.velocity;
		AnimationClip currentAnimation = this.Animator.GetCurrentAnimation();
		if (currentAnimation == this.ClimbAnim || currentAnimation == this.ClimbDownAnim)
		{
			return;
		}
		if (!amDead)
		{
			if (velocity.sqrMagnitude >= 0.05f)
			{
				bool flipX = this.rend.flipX;
				if (velocity.x < -0.01f)
				{
					this.rend.flipX = true;
				}
				else if (velocity.x > 0.01f)
				{
					this.rend.flipX = false;
				}
				if (currentAnimation != this.RunAnim || flipX != this.rend.flipX)
				{
					this.Animator.Play(this.RunAnim, 1f);
					this.Animator.Time = 0.45833334f;
					this.Skin.SetRun(this.rend.flipX);
				}
			}
			else if (currentAnimation == this.RunAnim || currentAnimation == this.SpawnAnim || !currentAnimation)
			{
				this.Skin.SetIdle(this.rend.flipX);
				this.Animator.Play(this.IdleAnim, 1f);
				this.myPlayer.SetHatAlpha(1f);
			}
		}
		else
		{
			this.Skin.SetGhost();
			if (currentAnimation != this.GhostIdleAnim)
			{
				this.Animator.Play(this.GhostIdleAnim, 1f);
				this.myPlayer.SetHatAlpha(0.5f);
			}
			if (velocity.x < -0.01f)
			{
				this.rend.flipX = true;
			}
			else if (velocity.x > 0.01f)
			{
				this.rend.flipX = false;
			}
		}
		this.Skin.Flipped = this.rend.flipX;
	}

	public IEnumerator CoSpawnPlayer(LobbyBehaviour lobby)
	{
		if (!lobby)
		{
			yield break;
		}
		if (this.myPlayer.AmOwner)
		{
			this.inputHandler.enabled = true;
		}
		int spawnSeatId = (int)this.myPlayer.PlayerId % lobby.SpawnPositions.Length;
		Vector3 spawnPos = this.Vec2ToPosition(lobby.SpawnPositions[spawnSeatId]);
		this.myPlayer.nameText.gameObject.SetActive(false);
		this.myPlayer.Collider.enabled = false;
		KillAnimation.SetMovement(this.myPlayer, false);
		yield return new WaitForFixedUpdate();
		bool amFlipped = spawnSeatId > 4;
		this.myPlayer.MyRend.flipX = amFlipped;
		this.myPlayer.transform.position = spawnPos;
		SoundManager.Instance.PlaySound(lobby.SpawnSound, false, 1f).volume = 0.75f;
		this.Skin.SetSpawn(this.rend.flipX, 0f);
		this.Skin.Flipped = amFlipped;
		this.GlowAnimator.GetComponent<SpriteRenderer>().flipX = this.rend.flipX;
		this.GlowAnimator.Play(this.SpawnGlowAnim, 1f);
		yield return new WaitForAnimationFinish(this.Animator, this.SpawnAnim);
		base.transform.position = spawnPos + new Vector3(amFlipped ? -0.3f : 0.3f, -0.24f);
		this.ResetMoveState(false);
		Vector2 vector = (-spawnPos).normalized;
		yield return this.WalkPlayerTo((Vector2) spawnPos + vector, 0.01f, 1f);
		this.myPlayer.Collider.enabled = true;
		KillAnimation.SetMovement(this.myPlayer, true);
		this.myPlayer.nameText.gameObject.SetActive(true);
		if (this.myPlayer.AmOwner)
		{
			this.inputHandler.enabled = false;
		}
		yield break;
	}

	public void ExitAllVents()
	{
		ConsoleJoystick.SetMode_Gameplay();
		Vent.currentVent = null;
		this.ResetMoveState(true);
		this.myPlayer.moveable = true;
		Vent[] allVents = ShipStatus.Instance.AllVents;
		for (int i = 0; i < allVents.Length; i++)
		{
			allVents[i].SetButtons(false);
		}
	}

	private IEnumerator CoEnterVent(int id)
	{
		Vent vent = ShipStatus.Instance.AllVents.First((Vent v) => v.Id == id);
		if (this.myPlayer.AmOwner)
		{
			this.inputHandler.enabled = true;
		}
		this.myPlayer.moveable = false;
		yield return this.WalkPlayerTo(vent.transform.position + vent.Offset, 0.01f, 1f);
		vent.EnterVent(this.myPlayer);
		this.Skin.SetEnterVent(this.rend.flipX);
		yield return new WaitForAnimationFinish(this.Animator, this.EnterVentAnim);
		this.Skin.SetIdle(this.rend.flipX);
		this.Animator.Play(this.IdleAnim, 1f);
		this.myPlayer.Visible = false;
		this.myPlayer.inVent = true;
		if (this.myPlayer.AmOwner)
		{
			VentilationSystem.Update(VentilationSystem.Operation.Enter, id);
			this.inputHandler.enabled = false;
		}
		yield break;
	}

	private IEnumerator CoExitVent(int id)
	{
		Vent vent = ShipStatus.Instance.AllVents.First((Vent v) => v.Id == id);
		if (this.myPlayer.AmOwner)
		{
			this.inputHandler.enabled = true;
			VentilationSystem.Update(VentilationSystem.Operation.Exit, id);
		}
		this.myPlayer.Visible = true;
		this.myPlayer.inVent = false;
		vent.ExitVent(this.myPlayer);
		this.Skin.SetExitVent(this.rend.flipX);
		yield return new WaitForAnimationFinish(this.Animator, this.ExitVentAnim);
		this.Skin.SetIdle(this.rend.flipX);
		this.Animator.Play(this.IdleAnim, 1f);
		this.myPlayer.moveable = true;
		if (this.myPlayer.AmOwner)
		{
			this.inputHandler.enabled = false;
		}
		yield break;
	}

	public IEnumerator WalkPlayerTo(Vector2 worldPos, float tolerance = 0.01f, float speedMul = 1f)
	{
		worldPos -= this.myPlayer.Collider.offset;
		Rigidbody2D body = this.body;
		Vector2 del = worldPos - (Vector2) base.transform.position;
		while (del.sqrMagnitude > tolerance)
		{
			float num = Mathf.Clamp(del.magnitude * 2f, 0.05f, 1f);
			body.velocity = del.normalized * this.Speed * num * speedMul;
			yield return null;
			if (body.velocity.magnitude < 0.005f && (double)del.sqrMagnitude < 0.1)
			{
				break;
			}
			del = worldPos - (Vector2) base.transform.position;
		}
		del = default(Vector2);
		body.velocity = Vector2.zero;
		yield break;
	}

	public override bool Serialize(MessageWriter writer, bool initialState)
	{
		return false;
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
	}

	private void BootFromVent(int ventId)
	{
		if (base.AmOwner)
		{
			ShipStatus.Instance.AllVents.ForEach(delegate(Vent v)
			{
				v.SetButtons(false);
			});
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(this.ImpostorDiscoveredSound, false, 0.8f);
			}
		}
		base.StopAllCoroutines();
		base.StartCoroutine(this.CoExitVent(ventId));
	}
}
