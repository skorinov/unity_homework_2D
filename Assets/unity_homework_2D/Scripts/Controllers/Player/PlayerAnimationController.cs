using UnityEngine;

namespace Controllers.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimationController : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float runThreshold = 0.1f;
        [SerializeField] private float directionChangeDelay = 0.15f;

        // Cached components
        private Animator _animator;
        private PlayerController _playerController;
        private Rigidbody2D _rb;
        private SpriteRenderer _spriteRenderer;

        // Cached animator hashes for better performance
        private static readonly int IsRunning = Animator.StringToHash("IsRunning");
        private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
        private static readonly int IsJumping = Animator.StringToHash("IsJumping");

        // State tracking to avoid unnecessary animator calls
        private bool _facingRight = true;
        private bool _lastGroundedState;
        private bool _lastRunningState;
        
        private float _lastDirectionChange;
        private float _lastSignificantVelocityX;

        // Update frequency optimization
        private int _frameCount;
        private const int ANIMATION_UPDATE_INTERVAL = 3; // Update every 3 frames for smoother performance

        private void Awake()
        {
            CacheComponents();
        }

        private void CacheComponents()
        {
            _animator = GetComponent<Animator>();
            _playerController = GetComponent<PlayerController>();
            _rb = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (!_animator || !_rb || !_playerController) return;
            
            Vector2 velocity = _rb.linearVelocity;
            bool isGrounded = _playerController.IsGrounded;
            bool isRunning = isGrounded && Mathf.Abs(velocity.x) > runThreshold;

            // Only update animator when states actually change
            if (isRunning != _lastRunningState)
            {
                _animator.SetBool(IsRunning, isRunning);
                _lastRunningState = isRunning;
            }

            if (isGrounded != _lastGroundedState)
            {
                _animator.SetBool(IsGrounded, isGrounded);
                _lastGroundedState = isGrounded;
            }
            
            UpdateSpriteDirection(velocity.x);
        }

        private void UpdateSpriteDirection(float velocityX)
        {
            if (Mathf.Abs(velocityX) > runThreshold)
                _lastSignificantVelocityX = velocityX;

            bool shouldFaceRight = _lastSignificantVelocityX > 0;
            
            // Apply direction change with delay to prevent rapid flipping
            if (shouldFaceRight != _facingRight && 
                Time.time - _lastDirectionChange > directionChangeDelay)
            {
                _facingRight = shouldFaceRight;
                _spriteRenderer.flipX = !_facingRight;
                _lastDirectionChange = Time.time;
            }
        }
        
        /// Triggers jump animation
        public void TriggerJump() => _animator.SetTrigger(IsJumping);
    }
}