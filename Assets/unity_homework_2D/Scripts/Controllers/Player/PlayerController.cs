using Managers;
using UnityEngine;
using Controllers.Platform;

namespace Controllers.Player
{
    /// Main player movement controller with jump mechanics, ground detection, and platform interactions
    /// Handles horizontal movement, single/double jump, coyote time, and platform effects
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private InputManager inputManager;

        [Header("Movement")] 
        [SerializeField] private float baseMoveSpeed = 4f;
        // Maximum speed limit to prevent infinite acceleration
        [SerializeField] private float maxHorizontalVelocity = 4f;

        [Header("Jump")] 
        [SerializeField] private float baseJumpForce = 12f;
        [SerializeField] private float doubleJumpForce = 8f;

        [Header("Ground Check")]
        // Empty GameObject positioned at player's feet
        [SerializeField] private Transform groundCheck;  
        [SerializeField] private float groundCheckRadius = 0.2f;
        // Which layers count as "ground"
        [SerializeField] private LayerMask groundLayerMask = 1;

        // Cached components for performance
        private Rigidbody2D _rb;
        private PlayerAnimationController _animationController;

        // Grace period after leaving platform
        private const float CoyoteTimeWindow = 0.2f;

        // Player state tracking
        private bool _isGrounded;
        private bool _hasDoubleJumped;
        // Remaining coyote time in seconds
        private float _coyoteTimeCounter;
        
        // Platform effect multipliers (modified by special platforms)
        private float _moveSpeedMultiplier = 1f;
        private float _jumpForceMultiplier = 1f;

        // Public properties for external access
        public bool IsGrounded => _isGrounded;
        public float BaseJumpForce => baseJumpForce;
        
        /// Initialize components and subscribe to input events
        private void Awake()
        {
            // Cache required components
            _rb = GetComponent<Rigidbody2D>();
            _animationController = GetComponent<PlayerAnimationController>();

            // Connect to input system events
            if (inputManager)
            {
                inputManager.OnHorizontalInput += HandleHorizontalInput;
                inputManager.OnJumpInput += TryJump;
                inputManager.OnDropInput += TryDropThrough;
            }
        }
        
        /// Handle coyote time countdown (grace period for jumping after leaving ground)
        private void Update()
        {
            // Decrease coyote time when airborne
            if (!_isGrounded)
            {
                _coyoteTimeCounter -= Time.deltaTime;
            }
        }
        
        /// Physics update: ground detection and velocity limiting
        private void FixedUpdate()
        {
            CheckGroundStatus();

            // Prevent infinite horizontal acceleration
            var velocity = _rb.linearVelocity;
            if (Mathf.Abs(velocity.x) > maxHorizontalVelocity)
            {
                // Clamp velocity while preserving direction
                velocity.x = Mathf.Sign(velocity.x) * maxHorizontalVelocity;
                _rb.linearVelocity = velocity;
            }
        }
        
        /// Apply horizontal movement force based on input
        private void HandleHorizontalInput(float input) =>
            _rb.AddForce(new Vector2(input * baseMoveSpeed * _moveSpeedMultiplier, 0f), ForceMode2D.Force);
        
        /// Drop through one-way platforms (down key while on platform)
        /// Temporarily disables platform collision and gives downward velocity
        private void TryDropThrough()
        {
            // Only works when standing on ground
            if (!_isGrounded) return;

            // Check what we're standing on
            var hit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);
            var platform = hit?.GetComponent<BasePlatform>();

            if (platform)
            {
                // Disable platform collision temporarily
                platform.GetComponent<BoxCollider2D>().enabled = false;
                platform.DisableCollision();
                
                // Push player downward to ensure they fall through
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, -5f);
            }
        }
        
        /// Main jumping logic with coyote time and double jump support
        /// Handles both ground jumps and air jumps with different forces
        private void TryJump()
        {
            // Can jump if grounded OR within coyote time window
            bool canJump = _isGrounded || _coyoteTimeCounter > 0;

            if (canJump)
            {
                // Primary jump from ground or coyote time
                Jump(baseJumpForce * _jumpForceMultiplier);
                AudioManager.Instance?.PlayJumpSound();
                _animationController?.TriggerJump();
                
                // Reset double jump availability and coyote time
                _hasDoubleJumped = false;
                _coyoteTimeCounter = 0;
            }
            else if (!_hasDoubleJumped)
            {
                // Secondary jump in mid-air
                Jump(doubleJumpForce * _jumpForceMultiplier);
                AudioManager.Instance?.PlayJumpSound();
                _animationController?.TriggerDoubleJump();
                
                // Mark double jump as used
                _hasDoubleJumped = true;
            }
        }
        
        /// Execute jump with specified force
        public void Jump(float force)
        {
            // Reset Y velocity for consistent jump height (prevents variable jumps when falling)
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f);
            
            // Apply upward impulse (immediate velocity change)
            _rb.AddForce(new Vector2(0f, force), ForceMode2D.Impulse);
        }
        
        /// Ground detection using Physics2D.OverlapCircle
        /// Handles state transitions and coyote time initialization
        private void CheckGroundStatus()
        {
            bool wasGrounded = _isGrounded;
            
            // Check for ground collision at player's feet
            _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);

            // Just landed: reset abilities and platform effects
            if (_isGrounded && !wasGrounded)
            {
                _hasDoubleJumped = false;
                ResetMultipliers();
            }
            // Just left ground: start coyote time
            else if (!_isGrounded && wasGrounded)
            {
                // Grace period for jumping
                _coyoteTimeCounter = CoyoteTimeWindow;  
            }
        }

        #region Platform Effect System
        
        /// Modify horizontal movement speed (used by sticky platforms)
        public void SetHorizontalSpeedMultiplier(float multiplier) => 
            _moveSpeedMultiplier = multiplier;
        
        /// Modify jump force (used by bouncy platforms, sticky platforms)
        public void SetJumpForceMultiplier(float multiplier) => 
            _jumpForceMultiplier = multiplier;
        
        /// Reset all platform effects to default values
        public void ResetMultipliers() => 
            _moveSpeedMultiplier = _jumpForceMultiplier = 1f;
            
        #endregion
    }
}