using Managers;
using UnityEngine;

namespace Controllers.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
    public class PlayerDamageSystem : MonoBehaviour
    {
        [SerializeField] private float invulnerabilityDuration = 1.5f;
        [SerializeField] private float blinkInterval = 0.1f;
        [SerializeField] private Color damageColor = new Color(1f, 0.5f, 0.5f, 1f);
        
        private Rigidbody2D _rb;
        private SpriteRenderer _spriteRenderer;
        private Color _originalColor;
        private bool _isInvulnerable;
        private float _invulnerabilityTimer;
        private float _blinkTimer;
        private bool _isBlinking;
        
        public bool IsInvulnerable => _isInvulnerable;
        
        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _originalColor = _spriteRenderer.color;
        }
        
        private void Update()
        {
            if (_isInvulnerable)
            {
                UpdateInvulnerability();
                UpdateBlinkEffect();
            }
        }
        
        private void UpdateInvulnerability()
        {
            _invulnerabilityTimer -= Time.deltaTime;
            
            if (_invulnerabilityTimer <= 0f)
            {
                EndInvulnerability();
            }
        }
        
        private void UpdateBlinkEffect()
        {
            _blinkTimer -= Time.deltaTime;
            
            if (_blinkTimer <= 0f)
            {
                _isBlinking = !_isBlinking;
                _blinkTimer = blinkInterval;
                
                // Toggle visibility
                Color color = _spriteRenderer.color;
                color.a = _isBlinking ? 0.3f : 1f;
                _spriteRenderer.color = color;
            }
        }
        
        public void TakeDamage(Vector2 pushDirection, float pushForce, float upwardForce)
        {
            if (_isInvulnerable) return;
            
            StartInvulnerability();
            ApplyPushForce(pushDirection, pushForce, upwardForce);
            PlayDamageEffects();
        }
        
        private void StartInvulnerability()
        {
            _isInvulnerable = true;
            _invulnerabilityTimer = invulnerabilityDuration;
            _blinkTimer = 0f;
            _isBlinking = false;
        }
        
        private void EndInvulnerability()
        {
            _isInvulnerable = false;
            _isBlinking = false;
            
            // Restore original appearance
            _spriteRenderer.color = _originalColor;
        }
        
        private void ApplyPushForce(Vector2 direction, float force, float upForce)
        {
            // Reset vertical velocity for consistent push
            Vector2 velocity = _rb.linearVelocity;
            velocity.y = 0f;
            _rb.linearVelocity = velocity;
            
            // Apply push force
            Vector2 pushVector = new Vector2(
                direction.x * force,
                Mathf.Max(direction.y * force, upForce)
            );
            
            _rb.AddForce(pushVector, ForceMode2D.Impulse);
        }
        
        private void PlayDamageEffects()
        {
            // Brief color flash
            StartCoroutine(DamageColorFlash());
            
            // Play damage sound if available
            // AudioManager.Instance?.PlayPlayerHurt();
        }
        
        private System.Collections.IEnumerator DamageColorFlash()
        {
            _spriteRenderer.color = damageColor;
            yield return new WaitForSeconds(0.1f);
            
            // Color will be handled by blinking system after this
            if (!_isInvulnerable)
                _spriteRenderer.color = _originalColor;
        }
        
        public void ResetDamageSystem()
        {
            EndInvulnerability();
            StopAllCoroutines();
        }
        
        // Public method to check if player can take damage (for other systems)
        public bool CanTakeDamage() => !_isInvulnerable;
        
        private void OnDisable()
        {
            // Ensure we clean up when player is disabled
            ResetDamageSystem();
        }
    }
}