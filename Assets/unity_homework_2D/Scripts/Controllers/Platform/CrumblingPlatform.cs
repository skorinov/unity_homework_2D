using Controllers.Player;
using Managers;
using UnityEngine;

namespace Controllers.Platform
{
    public class CrumblingPlatform : BasePlatform
    {
        [Header("Crumbling Settings")]
        [SerializeField] private float crumbleDuration = 1f;
        
        private bool _isCrumbling;
        private float _crumbleTimer;
        private Color _originalColor;
        
        public override PlatformType GetPlatformType() => PlatformType.Crumbling;
        
        protected override void Awake()
        {
            base.Awake();
            if (SpriteRenderer)
            {
                _originalColor = SpriteRenderer.color;
            }
                
        }
        
        public override void ResetPlatform()
        {
            base.ResetPlatform();
            _isCrumbling = false;
            _crumbleTimer = 0f;

            if (SpriteRenderer)
            {
                SpriteRenderer.color = _originalColor;
            }
        }
        
        protected override void OnPlayerLanded(PlayerController player)
        {
            PlayerOnPlatform = player;

            if (_isCrumbling) return;

            _isCrumbling = true;
            _crumbleTimer = crumbleDuration;
        }
        
        protected override void OnPlayerStaying(PlayerController player)
        {
            PlayerOnPlatform = player;
        }
        
        protected override void OnPlayerLeft(PlayerController player)
        {
            PlayerOnPlatform = null;

            if (_isCrumbling)
            {
                StopCrumbling();
            }
                
        }
        
        private void Update()
        {
            if (!_isCrumbling) return;
            _crumbleTimer -= Time.deltaTime;
                
            if (_crumbleTimer <= 0f)
            {
                _isCrumbling = false;
                AudioManager.Instance?.PlayPlatformBreakSound(); // Play break sound
                BreakPlatform();
                return;
            }
                
            // Update visual fade
            float fadeAmount = _crumbleTimer / crumbleDuration;

            if (!SpriteRenderer) return;
            Color color = _originalColor;
            color.a = fadeAmount;
            SpriteRenderer.color = color;
        }
        
        private void StopCrumbling()
        {
            _isCrumbling = false;
            _crumbleTimer = 0f;

            if (SpriteRenderer)
            {
                SpriteRenderer.color = _originalColor;
            }
        }
    }
}