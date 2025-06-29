using Constants;
using UnityEngine;

namespace Controllers.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimationController : MonoBehaviour
    {
        [SerializeField] private float runThreshold = 0.1f;
        [SerializeField] private float directionChangeDelay = 0.15f;

        private Animator _animator;
        private PlayerController _playerController;
        private Rigidbody2D _rb;
        private SpriteRenderer _spriteRenderer;

        private static readonly int IsRunning = Animator.StringToHash("IsRunning");
        private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
        private static readonly int IsJumping = Animator.StringToHash("IsJumping");

        private bool _facingRight = true;
        private bool _lastGroundedState;
        private bool _lastRunningState;
        
        private float _lastDirectionChange;
        private float _lastSignificantVelocityX;

        private int _frameCount;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _playerController = GetComponent<PlayerController>();
            _rb = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (!_animator || !_rb || !_playerController) return;
            
            _frameCount++;
            if (_frameCount % GameConstants.ANIMATION_UPDATE_INTERVAL != 0) return;

            Vector2 velocity = _rb.linearVelocity;
            bool isGrounded = _playerController.IsGrounded;
            bool isRunning = isGrounded && Mathf.Abs(velocity.x) > runThreshold;

            UpdateAnimatorStates(isRunning, isGrounded);
            UpdateSpriteDirection(velocity.x);
        }

        private void UpdateAnimatorStates(bool isRunning, bool isGrounded)
        {
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
        }

        private void UpdateSpriteDirection(float velocityX)
        {
            if (Mathf.Abs(velocityX) > runThreshold)
                _lastSignificantVelocityX = velocityX;

            bool shouldFaceRight = _lastSignificantVelocityX > 0;
            
            if (shouldFaceRight != _facingRight && 
                Time.time - _lastDirectionChange > directionChangeDelay)
            {
                _facingRight = shouldFaceRight;
                _spriteRenderer.flipX = !_facingRight;
                _lastDirectionChange = Time.time;
            }
        }
        
        public void TriggerJump() => _animator.SetTrigger(IsJumping);
    }
}