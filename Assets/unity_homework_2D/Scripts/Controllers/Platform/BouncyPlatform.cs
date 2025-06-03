using Controllers.Player;
using Managers;
using UnityEngine;

namespace Controllers.Platform
{
    public class BouncyPlatform : BasePlatform
    {
        [Header("Bouncy Settings")]
        [SerializeField] private float jumpForceMultiplier = 1.5f;
        
        private const float BounceScale = 1.1f;
        private const float BounceDuration = 0.1f;
        
        private Vector3 _originalScale;
        private float _bounceTimer;
        private bool _isBouncing;
        
        public override PlatformType GetPlatformType() => PlatformType.Bouncy;
        
        protected override void Awake()
        {
            base.Awake();
            _originalScale = transform.localScale;
        }
        
        protected override void OnPlayerLanded(PlayerController player)
        {
            player.Jump(player.BaseJumpForce * jumpForceMultiplier);
            AudioManager.Instance?.PlayBounceSound();
            StartBounceEffect();
        }
        
        private void Update()
        {
            if (!_isBouncing) return;
            
            _bounceTimer -= Time.deltaTime;
            if (_bounceTimer <= 0f)
            {
                transform.localScale = _originalScale;
                _isBouncing = false;
            }
        }
        
        private void StartBounceEffect()
        {
            transform.localScale = _originalScale * BounceScale;
            _bounceTimer = BounceDuration;
            _isBouncing = true;
        }
        
        public override void ResetPlatform()
        {
            base.ResetPlatform();
            transform.localScale = _originalScale;
            _isBouncing = false;
            _bounceTimer = 0f;
        }
    }
}