using UnityEngine;

namespace Controllers.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimationController : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float runThreshold = 0.1f;
        [SerializeField] private float directionChangeDelay = 0.15f;
        
        // Core components
        private Animator _animator;
        private PlayerController _playerController;
        private Rigidbody2D _rb;
        private SpriteRenderer _spriteRenderer;
        
        // Cached animation parameter hashes
        private static readonly int IsRunning = Animator.StringToHash("IsRunning");
        private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
        private static readonly int VelocityY = Animator.StringToHash("VelocityY");
        private static readonly int IsJumping = Animator.StringToHash("IsJumping");
        private static readonly int IsDoubleJumping = Animator.StringToHash("IsDoubleJumping");
        
        // Direction state tracking
        private bool _facingRight = true;
        private float _lastDirectionChange;
        private float _lastSignificantVelocityX;
        
        private void Awake()
        {
            // Cache all required components once
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
            
            // Update animation states
            UpdateAnimationStates(velocity, isGrounded);
            UpdateSpriteDirection(velocity.x);
        }
        
        /// Updates all animation parameters
        private void UpdateAnimationStates(Vector2 velocity, bool isGrounded)
        {
            bool isRunning = isGrounded && Mathf.Abs(velocity.x) > runThreshold;
            
            _animator.SetBool(IsRunning, isRunning);
            _animator.SetBool(IsGrounded, isGrounded);
            _animator.SetFloat(VelocityY, velocity.y);
        }
        
        /// Handles sprite flipping with delay to prevent jittering
        private void UpdateSpriteDirection(float velocityX)
        {
            // Only track significant movement to avoid jitter
            if (Mathf.Abs(velocityX) > runThreshold)
            {
                _lastSignificantVelocityX = velocityX;
            }
            
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
        
        /// Triggers double jump animation
        public void TriggerDoubleJump() => _animator.SetTrigger(IsDoubleJumping);
    }
}