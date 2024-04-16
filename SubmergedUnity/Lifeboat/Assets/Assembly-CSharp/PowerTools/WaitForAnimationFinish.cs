using System;
using System.Collections;
using UnityEngine;

namespace PowerTools
{
    public class WaitForAnimationFinish : IEnumerator
    {
        private SpriteAnim animator;

        private AnimationClip clip;

        private bool first = true;

        public WaitForAnimationFinish(SpriteAnim animator, AnimationClip clip)
        {
            this.animator = animator;
            this.clip = clip;
            this.animator.Play(this.clip, 1f);
            this.animator.Time = 0f;
        }

        public object Current
        {
            get
            {
                return null;
            }
        }

        public bool MoveNext()
        {
            if (this.first)
            {
                this.first = false;
                return true;
            }
            bool result;
            try
            {
                result = this.animator.IsPlaying(this.clip);
            }
            catch
            {
                result = false;
            }
            return result;
        }

        public void Reset()
        {
            this.first = true;
            this.animator.Play(this.clip, 1f);
        }
    }
}