using Constants;
using Controllers.Platform;
using Managers;
using UnityEngine;
using Utilities;

namespace Controllers.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : Singleton<PlayerController>
    {
        [SerializeField] private InputManager inputManager;
        [SerializeField] private float groundMoveSpeed = 8f;
        [SerializeField] private float airMoveSpeed = 6f;
        [SerializeField] private float maxHorizontalVelocity = 8f;
        [SerializeField] private float baseJumpForce = 12f;
        [SerializeField] private float doubleJumpForce = 8f;
        [SerializeField] private float acceleration = 25f;
        [SerializeField] private float airAcceleration = 15f;
        [SerializeField] private float friction = 20f;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayerMask = 1;

        private Rigidbody2D _rb;
        private PlayerAnimationController _animationController;

        private bool _isGrounded;
        private bool _hasDoubleJumped;
        private float _coyoteTimeCounter;
        private float _horizontalInput;

        private float _moveSpeedMultiplier = 1f;
        private float _jumpForceMultiplier = 1f;

        public bool IsGrounded => _isGrounded;
        public float BaseJumpForce => baseJumpForce;

        protected override void OnSingletonAwake()
        {
            InitializeComponents();
            SubscribeToInput();
        }

        private void Update()
        {
            if (!_isGrounded)
                _coyoteTimeCounter -= Time.deltaTime;
        }

        private void FixedUpdate()
        {
            CheckGroundStatus();
            HandleMovement();
            ClampHorizontalVelocity();
        }

        private void InitializeComponents()
        {
            _rb = GetComponent<Rigidbody2D>();
            _animationController = GetComponent<PlayerAnimationController>();
        }

        private void SubscribeToInput()
        {
            if (inputManager)
            {
                inputManager.OnHorizontalInput += HandleHorizontalInput;
                inputManager.OnJumpInput += TryJump;
                inputManager.OnDropInput += TryDropThrough;
            }
        }

        private void HandleHorizontalInput(float input) => _horizontalInput = input;

        private void HandleMovement()
        {
            float currentSpeed = _isGrounded ? groundMoveSpeed : airMoveSpeed;
            float currentAcceleration = _isGrounded ? acceleration : airAcceleration;

            float targetVelocity = _horizontalInput * currentSpeed * _moveSpeedMultiplier;
            float velocityDifference = targetVelocity - _rb.linearVelocity.x;

            // Apply acceleration or friction
            if (Mathf.Abs(_horizontalInput) > 0.1f)
            {
                // Player is giving input - accelerate toward target velocity
                float force = velocityDifference * currentAcceleration;
                _rb.AddForce(new Vector2(force, 0f), ForceMode2D.Force);
            }
            else if (_isGrounded)
            {
                // No input and grounded - apply friction
                float frictionForce = -_rb.linearVelocity.x * friction;
                _rb.AddForce(new Vector2(frictionForce, 0f), ForceMode2D.Force);
            }
        }

        private void TryDropThrough()
        {
            if (!_isGrounded) return;

            var hit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);
            var platform = hit?.GetComponent<BasePlatform>();

            if (platform)
            {
                DisablePlatformCollision(platform);
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, -5f);
            }
        }

        private void DisablePlatformCollision(BasePlatform platform)
        {
            platform.GetComponent<BoxCollider2D>().enabled = false;
            platform.DisableCollision();
        }

        private void TryJump()
        {
            bool canJump = _isGrounded || _coyoteTimeCounter > 0;

            if (canJump)
            {
                PerformJump(baseJumpForce * _jumpForceMultiplier);
                _hasDoubleJumped = false;
                _coyoteTimeCounter = 0;
            }
            else if (!_hasDoubleJumped)
            {
                PerformJump(doubleJumpForce * _jumpForceMultiplier);
                _hasDoubleJumped = true;
            }
        }

        private void PerformJump(float force)
        {
            Jump(force);
            AudioManager.Instance?.PlayJumpSound();
            _animationController?.TriggerJump();
        }

        public void Jump(float force)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f);
            _rb.AddForce(new Vector2(0f, force), ForceMode2D.Impulse);
        }

        private void CheckGroundStatus()
        {
            bool wasGrounded = _isGrounded;
            _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);

            if (_isGrounded && !wasGrounded)
            {
                _hasDoubleJumped = false;
                ResetMultipliers();
            }
            else if (!_isGrounded && wasGrounded)
            {
                _coyoteTimeCounter = GameConstants.COYOTE_TIME_WINDOW;
            }
        }

        private void ClampHorizontalVelocity()
        {
            var velocity = _rb.linearVelocity;
            if (Mathf.Abs(velocity.x) > maxHorizontalVelocity)
            {
                velocity.x = Mathf.Sign(velocity.x) * maxHorizontalVelocity;
                _rb.linearVelocity = velocity;
            }
        }

        public void SetHorizontalSpeedMultiplier(float multiplier) => _moveSpeedMultiplier = multiplier;
        public void SetJumpForceMultiplier(float multiplier) => _jumpForceMultiplier = multiplier;
        public void ResetMultipliers() => _moveSpeedMultiplier = _jumpForceMultiplier = 1f;

        protected override void OnSingletonDestroy()
        {
            UnsubscribeFromInput();
        }

        private void UnsubscribeFromInput()
        {
            if (inputManager)
            {
                inputManager.OnHorizontalInput -= HandleHorizontalInput;
                inputManager.OnJumpInput -= TryJump;
                inputManager.OnDropInput -= TryDropThrough;
            }
        }

        public void ForceEnable()
        {
            gameObject.SetActive(true);
        }
    }
}