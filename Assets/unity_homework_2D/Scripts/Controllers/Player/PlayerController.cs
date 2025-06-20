using Managers;
using UnityEngine;
using Controllers.Platform;

namespace Controllers.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private InputManager inputManager;

        [Header("Movement")]
        [SerializeField] private float baseMoveSpeed = 4f;
        [SerializeField] private float maxHorizontalVelocity = 4f;

        [Header("Jump")]
        [SerializeField] private float baseJumpForce = 12f;
        [SerializeField] private float doubleJumpForce = 8f;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayerMask = 1;

        private Rigidbody2D _rb;
        private PlayerAnimationController _animationController;

        private const float CoyoteTimeWindow = 0.2f;

        private bool _isGrounded;
        private bool _hasDoubleJumped;
        private float _coyoteTimeCounter;

        private float _moveSpeedMultiplier = 1f;
        private float _jumpForceMultiplier = 1f;

        public bool IsGrounded => _isGrounded;
        public float BaseJumpForce => baseJumpForce;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _animationController = GetComponent<PlayerAnimationController>();

            if (inputManager)
            {
                inputManager.OnHorizontalInput += HandleHorizontalInput;
                inputManager.OnJumpInput += TryJump;
                inputManager.OnDropInput += TryDropThrough;
            }
        }

        private void Update()
        {
            if (!_isGrounded)
                _coyoteTimeCounter -= Time.deltaTime;
        }

        private void FixedUpdate()
        {
            CheckGroundStatus();
            ClampHorizontalVelocity();
        }

        private void HandleHorizontalInput(float input) =>
            _rb.AddForce(new Vector2(input * baseMoveSpeed * _moveSpeedMultiplier, 0f), ForceMode2D.Force);

        private void TryDropThrough()
        {
            if (!_isGrounded) return;

            var hit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);
            var platform = hit?.GetComponent<BasePlatform>();

            if (platform)
            {
                platform.GetComponent<BoxCollider2D>().enabled = false;
                platform.DisableCollision();
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, -5f);
            }
        }

        private void TryJump()
        {
            bool canJump = _isGrounded || _coyoteTimeCounter > 0;

            if (canJump)
            {
                Jump(baseJumpForce * _jumpForceMultiplier);
                AudioManager.Instance?.PlayJumpSound();
                _animationController?.TriggerJump();

                _hasDoubleJumped = false;
                _coyoteTimeCounter = 0;
            }
            else if (!_hasDoubleJumped)
            {
                Jump(doubleJumpForce * _jumpForceMultiplier);
                AudioManager.Instance?.PlayJumpSound();

                _hasDoubleJumped = true;
            }
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
                _coyoteTimeCounter = CoyoteTimeWindow;
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
    }
}