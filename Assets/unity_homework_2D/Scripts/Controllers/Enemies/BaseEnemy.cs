using Constants;
using Controllers.Player;
using Pooling;
using UnityEngine;

namespace Controllers.Enemies
{
    [RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
    public abstract class BaseEnemy : MonoBehaviour, IPoolable
    {
        [SerializeField] protected float pushForce = 8f;
        [SerializeField] protected float upwardForce = 3f;
        
        protected Collider2D _collider;
        protected SpriteRenderer _spriteRenderer;
        protected Transform _currentPlatform;
        protected bool _isActive;
        
        protected virtual void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _collider.isTrigger = true;
        }
        
        protected virtual void Update()
        {
            if (!_isActive || !_currentPlatform) return;
            UpdateBehavior();
        }
        
        // Abstract methods for different enemy types
        protected abstract void UpdateBehavior();
        public abstract void SetupOnPlatform(Transform platform, Vector3 spawnPosition);
        
        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(GameConstants.PLAYER_TAG)) return;

            var player = other.GetComponent<PlayerController>();
            var damageSystem = other.GetComponent<PlayerDamageSystem>();
                
            if (player && damageSystem && !damageSystem.IsInvulnerable)
            {
                Vector2 pushDirection = CalculatePushDirection(player.transform.position);
                damageSystem.TakeDamage(pushDirection, pushForce, upwardForce);
                OnPlayerHit(player);
            }
        }
        
        protected virtual Vector2 CalculatePushDirection(Vector3 playerPosition)
        {
            Vector2 direction = (playerPosition - transform.position).normalized;
            
            // Ensure some upward force
            if (direction.y < GameConstants.MIN_PUSH_UPWARD_FORCE)
                direction.y = GameConstants.MIN_PUSH_UPWARD_FORCE;
                
            return direction.normalized;
        }
        
        // Virtual method for additional behavior on player hit
        protected virtual void OnPlayerHit(PlayerController player) { }
        
        public virtual void OnGetFromPool()
        {
            _isActive = false;
            _currentPlatform = null;
            gameObject.SetActive(true);
        }
        
        public virtual void OnReturnToPool()
        {
            _isActive = false;
            _currentPlatform = null;
            gameObject.SetActive(false);
        }
        
        public virtual void OnCreatedInPool() { }
        public virtual bool CanReturnToPool() => true;
    }
}