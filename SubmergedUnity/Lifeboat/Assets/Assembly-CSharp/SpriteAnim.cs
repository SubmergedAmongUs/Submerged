//-----------------------------------------
//          PowerSprite Animator
//  Copyright © 2017 Powerhoof Pty Ltd
//			  powerhoof.com
//----------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace PowerTools
{
[RequireComponent(typeof(Animator))]
	[DisallowMultipleComponent]
	public class SpriteAnim : SpriteAnimEventHandler
	{
		private static readonly string STATE_NAME = "a";

		private static readonly string CONTROLLER_PATH = "SpriteAnimController";

		[SerializeField]
		private AnimationClip m_defaultAnim;

		private static RuntimeAnimatorController m_sharedAnimatorController = null;

		private Animator m_animator;

		private AnimatorOverrideController m_controller;

		private SpriteAnimNodes m_nodes;

		private List<KeyValuePair<AnimationClip, AnimationClip>> m_clipPairList = new List<KeyValuePair<AnimationClip, AnimationClip>>(1);

		private AnimationClip m_currAnim;
		public AnimationClip Curr => m_currAnim;
		private float m_speed = 1f;

		public bool Playing
		{
			get
			{
				return this.IsPlaying();
			}
		}

		public bool Paused
		{
			get
			{
				return this.IsPaused();
			}
			set
			{
				if (value)
				{
					this.Pause();
					return;
				}
				this.Resume();
			}
		}

		public float Speed
		{
			get
			{
				return this.m_speed;
			}
			set
			{
				this.SetSpeed(value);
			}
		}

		public float Time
		{
			get
			{
				return this.GetTime();
			}
			set
			{
				this.SetTime(value);
			}
		}

		public float NormalizedTime
		{
			get
			{
				return this.GetNormalisedTime();
			}
			set
			{
				this.SetNormalizedTime(value);
			}
		}

		public AnimationClip Clip
		{
			get
			{
				return this.m_currAnim;
			}
		}

		public string ClipName
		{
			get
			{
				if (!(this.m_currAnim != null))
				{
					return string.Empty;
				}
				return this.m_currAnim.name;
			}
		}

		public void Play(AnimationClip anim = null, float speed = 1f)
		{
			if (anim == null)
			{
				anim = (this.m_currAnim ? this.m_currAnim : this.m_defaultAnim);
				if (anim == null)
				{
					return;
				}
			}
			if (!this.m_animator.enabled)
			{
				this.m_animator.enabled = true;
			}
			if (this.m_nodes != null)
			{
				this.m_nodes.Reset();
			}
			this.m_clipPairList[0] = new KeyValuePair<AnimationClip, AnimationClip>(this.m_clipPairList[0].Key, anim);
			this.m_controller.ApplyOverrides(this.m_clipPairList);
			this.m_animator.Update(0f);
			this.m_animator.Play(SpriteAnim.STATE_NAME, 0, 0f);
			this.m_speed = Mathf.Max(0f, speed);
			this.m_animator.speed = this.m_speed;
			this.m_currAnim = anim;
			this.m_animator.Update(0f);
		}

		public void Stop()
		{
			this.m_animator.enabled = false;
		}

		public void Pause()
		{
			this.m_animator.speed = 0f;
		}

		public void Resume()
		{
			this.m_animator.speed = this.m_speed;
		}

		public AnimationClip GetCurrentAnimation()
		{
			return this.m_currAnim;
		}

		public bool IsPlaying(AnimationClip clip = null)
		{
			return (clip == null || this.m_currAnim == clip) && this.m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f;
		}

		public bool IsPlaying(string animName)
		{
			return !(this.m_currAnim == null) && this.m_currAnim.name == animName && this.m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f;
		}

		public bool IsPaused()
		{
			return !(this.m_currAnim == null) && this.m_animator.speed == 0f;
		}

		public void SetSpeed(float speed)
		{
			this.m_speed = Mathf.Max(0f, speed);
			this.m_animator.speed = this.m_speed;
		}

		public float GetSpeed()
		{
			return this.m_speed;
		}

		public float GetTime()
		{
			if (this.m_currAnim != null)
			{
				return this.m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime * this.m_currAnim.length;
			}
			return 0f;
		}

		public void SetTime(float time)
		{
			if (this.m_currAnim == null || this.m_currAnim.length <= 0f)
			{
				return;
			}
			this.SetNormalizedTime(time / this.m_currAnim.length);
		}

		public float GetNormalisedTime()
		{
			if (this.m_currAnim != null)
			{
				return this.m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
			}
			return 0f;
		}

		public void SetNormalizedTime(float ratio)
		{
			if (this.m_currAnim == null)
			{
				return;
			}
			this.m_animator.Play(SpriteAnim.STATE_NAME, 0, this.m_currAnim.isLooping ? Mathf.Repeat(ratio, 1f) : Mathf.Clamp01(ratio));
		}

		private void Awake()
		{
			this.m_controller = new AnimatorOverrideController();
			if (SpriteAnim.m_sharedAnimatorController == null)
			{
				SpriteAnim.m_sharedAnimatorController = Resources.Load<RuntimeAnimatorController>(SpriteAnim.CONTROLLER_PATH);
			}
			this.m_controller.runtimeAnimatorController = SpriteAnim.m_sharedAnimatorController;
			this.m_animator = base.GetComponent<Animator>();
			this.m_animator.runtimeAnimatorController = this.m_controller;
			this.m_controller.GetOverrides(this.m_clipPairList);
			this.Play(this.m_defaultAnim, 1f);
			this.m_nodes = base.GetComponent<SpriteAnimNodes>();
		}

		private void Reset()
		{
			if (base.GetComponent<RectTransform>() == null)
			{
				if (base.GetComponent<Sprite>() == null)
				{
					base.gameObject.AddComponent<SpriteRenderer>();
					return;
				}
			}
			else if (base.GetComponent<Image>() == null)
			{
				base.gameObject.AddComponent<Image>();
			}
		}
	}
}