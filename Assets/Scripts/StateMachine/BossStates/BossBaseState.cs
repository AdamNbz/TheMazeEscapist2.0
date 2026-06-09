using UnityEngine;

namespace Platformer {
    public abstract class BossBaseState : IState {
        protected readonly BossController boss;
        protected readonly Animator animator;
        
        protected static readonly int IdleHash = Animator.StringToHash("IdleNormal");
        
        protected const float crossFadeDuration = 0.1f;

        protected BossBaseState(BossController boss, Animator animator) {
            this.boss = boss;
            this.animator = animator;
        }
        
        public virtual void OnEnter() {
            // noop
        }

        public virtual void Update() {
            // noop
        }

        public virtual void FixedUpdate() {
            // noop
        }

        public virtual void OnExit() {
            // noop
        }
    }
}