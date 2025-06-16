using Controllers.Player;
using Managers;
using UnityEngine;

namespace Controllers.Platform
{
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(PlatformEffector2D))]
    public abstract class BasePlatform : MonoBehaviour
    {
        [Header("Base Platform Settings")] 
        [SerializeField] protected Color platformColor = Color.white;

        // Core components - cached for performance
        protected SpriteRenderer SpriteRenderer;
        protected BoxCollider2D BoxCollider;
        protected PlatformEffector2D PlatformEffector;
        protected PlayerController PlayerOnPlatform;

        // Original values for reset functionality
        protected Vector2 OriginalSpriteSize;
        protected Vector2 OriginalColliderSize;
        protected Vector2 OriginalColliderOffset;

        public abstract PlatformType GetPlatformType();

        public enum PlatformType
        {
            Normal, Bouncy, Fragile, Moving, Crumbling, Sticky
        }

        protected virtual void Awake()
        {
            // Cache components and setup platform properties
            SpriteRenderer = GetComponent<SpriteRenderer>();
            BoxCollider = GetComponent<BoxCollider2D>();
            PlatformEffector = GetComponent<PlatformEffector2D>();
            CacheOriginalValues();
            SetupOneWayCollision();
        }

        protected virtual void OnEnable() => ResetPlatform();
        
        /// Stores original component values for reset functionality
        private void CacheOriginalValues()
        {
            if (SpriteRenderer?.sprite)
                OriginalSpriteSize = SpriteRenderer.sprite.bounds.size;

            if (BoxCollider)
            {
                OriginalColliderSize = BoxCollider.size;
                OriginalColliderOffset = BoxCollider.offset;
            }
        }
        
        /// Configures platform as one-way collision for jump-through behavior
        private void SetupOneWayCollision()
        {
            if (!PlatformEffector) return;

            PlatformEffector.useOneWay = true;
            PlatformEffector.surfaceArc = 180f;
            BoxCollider.usedByEffector = true;
        }
        
        /// Resets platform to initial state when returned to pool
        public virtual void ResetPlatform()
        {
            // Reset sprite appearance
            if (SpriteRenderer)
            {
                SpriteRenderer.color = platformColor;
                SpriteRenderer.size = OriginalSpriteSize;
            }

            // Reset collider properties
            if (BoxCollider)
            {
                BoxCollider.enabled = true;
                BoxCollider.size = OriginalColliderSize;
                BoxCollider.offset = OriginalColliderOffset;
                BoxCollider.usedByEffector = true;
            }

            PlayerOnPlatform = null;
        }
        
        /// Temporarily disables collision for drop-through functionality
        public void DisableCollision() => Invoke(nameof(ResetCollision), 0.2f);
        
        private void ResetCollision()
        {
            if (BoxCollider) BoxCollider.enabled = true;
        }

        #region Collision Events - Optimized player detection

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (IsPlayerLanding(other))
                OnPlayerLanded(other.gameObject.GetComponent<PlayerController>());
        }

        private void OnCollisionStay2D(Collision2D other)
        {
            if (IsPlayerLanding(other))
                OnPlayerStaying(other.gameObject.GetComponent<PlayerController>());
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Player"))
                OnPlayerLeft(other.gameObject.GetComponent<PlayerController>());
        }
        
        /// Checks if collision is valid player landing (downward movement)
        private bool IsPlayerLanding(Collision2D collision) =>
            collision.gameObject.CompareTag("Player") && collision.rigidbody.linearVelocity.y <= 0.1f;

        #endregion

        #region Virtual Methods - Override in derived classes

        protected virtual void OnPlayerLanded(PlayerController player) { }
        protected virtual void OnPlayerStaying(PlayerController player) { }
        protected virtual void OnPlayerLeft(PlayerController player) { }

        #endregion

        #region Platform Lifecycle

        /// Breaks platform and removes it from scene
        protected virtual void BreakPlatform()
        {
            if (SpriteRenderer) SpriteRenderer.color = Color.clear;
            if (BoxCollider) BoxCollider.enabled = false;
            ReturnToPool();
        }
        
        /// Returns platform to object pool for reuse
        protected virtual void ReturnToPool()
        {
            ResetPlatform();
            PlatformPool.Instance?.ReturnPlatform(gameObject);
        }

        #endregion
    }
}