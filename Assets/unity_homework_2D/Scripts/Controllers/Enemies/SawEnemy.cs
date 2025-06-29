using Constants;
using UnityEngine;

namespace Controllers.Enemies
{
    public class SawEnemy : BaseEnemy
    {
        [SerializeField] private float moveSpeed = 2f;
        
        private float _leftBound;
        private float _rightBound;
        private int _direction = GameConstants.DIRECTION_RIGHT; // right direction
        private float _rotationSpeed = 360f;
        
        protected override void UpdateBehavior()
        {
            RotateSaw();
            PatrolPlatform();
            CheckBounds();
        }
        
        private void RotateSaw()
        {
            transform.Rotate(0f, 0f, _rotationSpeed * Time.deltaTime);
        }
        
        private void PatrolPlatform()
        {
            Vector3 position = transform.position;
            position.x += _direction * moveSpeed * Time.deltaTime;
            transform.position = position;
        }
        
        private void CheckBounds()
        {
            float currentX = transform.position.x;
            
            if (_direction > 0 && currentX >= _rightBound)
            {
                _direction = -1;
                SetPositionX(_rightBound);
            }
            else if (_direction < 0 && currentX <= _leftBound)
            {
                _direction = 1;
                SetPositionX(_leftBound);
            }
        }
        
        private void SetPositionX(float boundX)
        {
            Vector3 pos = transform.position;
            pos.x = boundX;
            transform.position = pos;
        }
        
        public override void SetupOnPlatform(Transform platform, Vector3 spawnPosition)
        {
            if (!platform) return;
            
            _currentPlatform = platform;
            transform.position = spawnPosition;
            
            // Calculate platform bounds
            var platformCollider = platform.GetComponent<BoxCollider2D>();
            if (platformCollider)
            {
                float platformWidth = platformCollider.size.x * platform.localScale.x;
                float platformCenter = platform.position.x;
                float enemyRadius = (_collider as CircleCollider2D)?.radius * transform.localScale.x ?? GameConstants.DEFAULT_ENEMY_RADIUS;
                
                _leftBound = platformCenter - (platformWidth * GameConstants.HALF_WIDTH_MULTIPLIER) + enemyRadius;
                _rightBound = platformCenter + (platformWidth * GameConstants.HALF_WIDTH_MULTIPLIER) - enemyRadius;
            }
            else
            {
                // Fallback bounds
                _leftBound = platform.position.x - GameConstants.DEFAULT_PLATFORM_HALF_WIDTH;
                _rightBound = platform.position.x + GameConstants.DEFAULT_PLATFORM_HALF_WIDTH;
            }
            
            // Random starting direction
            _direction = Random.Range(0, 2) == 0 ? GameConstants.DIRECTION_LEFT : GameConstants.DIRECTION_RIGHT;
            _isActive = true;
        }
    }
}