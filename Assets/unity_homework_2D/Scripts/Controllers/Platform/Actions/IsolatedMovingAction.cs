using Constants;
using System.Collections.Generic;
using UnityEngine;

namespace Controllers.Platform.Actions
{
    [CreateAssetMenu(fileName = "IsolatedMovingAction", menuName = "Platform Actions/Isolated Moving Action")]
    public class IsolatedMovingAction : PlatformAction
    {
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float moveRange = 6f;
        [SerializeField] private bool startMovingRight = true;

        private readonly Dictionary<BasePlatform, MovingData> _movingData = new();
        private Camera _mainCamera;

        private struct MovingData
        {
            public float leftBound;
            public float rightBound;
            public int direction;
            public Rigidbody2D rigidbody;
            public bool wasMoving;
        }

        public override void Initialize(BasePlatform platform)
        {
            if (!_mainCamera) _mainCamera = Camera.main;
            
            _movingData[platform] = new MovingData
            {
                wasMoving = false
            };
        }

        public override void OnUpdate(BasePlatform platform)
        {
            bool shouldMove = IsAloneOnHorizontalLevel(platform);
            
            if (!_movingData.TryGetValue(platform, out var data)) return;
            
            if (shouldMove && !data.wasMoving)
            {
                StartMoving(platform);
                data = _movingData[platform]; // Get updated data from StartMoving
                data.wasMoving = true;
                _movingData[platform] = data;
            }
            else if (!shouldMove && data.wasMoving)
            {
                StopMoving(platform);
                data.wasMoving = false;
                _movingData[platform] = data;
            }
            else if (shouldMove && data.rigidbody)
            {
                UpdateMovement(platform, data);
            }
            else if (data.wasMoving && !data.rigidbody)
            {
                // Rigidbody was destroyed, reset state
                data.wasMoving = false;
                _movingData[platform] = data;
            }
        }

        private bool IsAloneOnHorizontalLevel(BasePlatform platform)
        {
            var activePlatforms = Pooling.PlatformPool.Instance?.GetActivePlatforms();
            if (activePlatforms == null) return true;
            
            Vector3 platformPos = platform.transform.position;
            
            foreach (var otherPlatform in activePlatforms)
            {
                if (otherPlatform == platform) continue;
                
                float yDifference = Mathf.Abs(platformPos.y - otherPlatform.transform.position.y);
                if (yDifference < GameConstants.SAME_HORIZONTAL_LEVEL_TOLERANCE) // Same horizontal level
                {
                    return false;
                }
            }
            
            return true;
        }

        private void StartMoving(BasePlatform platform)
        {
            if (!_movingData.TryGetValue(platform, out var data)) return;
            
            var rb = platform.GetComponent<Rigidbody2D>();
            if (!rb)
            {
                rb = platform.gameObject.AddComponent<Rigidbody2D>();
            }
            
            SetupRigidbody(rb);
            var bounds = CalculateBounds(platform);
            
            data.leftBound = bounds.min;
            data.rightBound = bounds.max;
            data.direction = startMovingRight ? GameConstants.DIRECTION_RIGHT : GameConstants.DIRECTION_LEFT;
            data.rigidbody = rb;
            
            _movingData[platform] = data;
        }

        private void StopMoving(BasePlatform platform)
        {
            if (!_movingData.TryGetValue(platform, out var data)) return;
            
            if (data.rigidbody && data.rigidbody.bodyType == RigidbodyType2D.Kinematic)
            {
                data.rigidbody.bodyType = RigidbodyType2D.Static;
            }
            
            data.rigidbody = null;
            _movingData[platform] = data;
        }

        private void UpdateMovement(BasePlatform platform, MovingData data)
        {
            Vector2 currentPos = data.rigidbody.position;
            float targetX = currentPos.x + data.direction * moveSpeed * Time.fixedDeltaTime;

            if (targetX >= data.rightBound)
            {
                data.direction = GameConstants.DIRECTION_LEFT;
                targetX = data.rightBound;
            }
            else if (targetX <= data.leftBound)
            {
                data.direction = GameConstants.DIRECTION_RIGHT;
                targetX = data.leftBound;
            }

            // Use velocity instead of MovePosition
            Vector2 velocity = new Vector2((targetX - currentPos.x) / Time.fixedDeltaTime, 0);
            data.rigidbody.linearVelocity = velocity;
    
            _movingData[platform] = data;
        }

        public override void OnReset(BasePlatform platform)
        {
            if (!_movingData.TryGetValue(platform, out var data)) return;

            if (data.wasMoving)
            {
                StopMoving(platform);
            }
            // Reset data but keep the entry for reuse
            _movingData[platform] = new MovingData { wasMoving = false };
        }

        private void SetupRigidbody(Rigidbody2D rb)
        {
            if (!rb) return;
            
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        private (float min, float max) CalculateBounds(BasePlatform platform)
        {
            float screenHalfWidth = _mainCamera.orthographicSize * _mainCamera.aspect;
            var boxCollider = platform.GetComponent<BoxCollider2D>();
            float platformHalfWidth = boxCollider ? 
                boxCollider.size.x * platform.transform.localScale.x * GameConstants.HALF_WIDTH_MULTIPLIER : 
                GameConstants.DEFAULT_PLATFORM_HALF_WIDTH_FALLBACK;
    
            float currentX = platform.transform.position.x;
            float halfRange = moveRange * GameConstants.HALF_WIDTH_MULTIPLIER;
    
            float screenLeft = -screenHalfWidth + GameConstants.SCREEN_MARGIN + platformHalfWidth;
            float screenRight = screenHalfWidth - GameConstants.SCREEN_MARGIN - platformHalfWidth;
    
            float leftBound = Mathf.Max(currentX - halfRange, screenLeft);
            float rightBound = Mathf.Min(currentX + halfRange, screenRight);
    
            if (rightBound - leftBound < GameConstants.MIN_MOVEMENT_RANGE)
            {
                float center = (leftBound + rightBound) * GameConstants.HALF_WIDTH_MULTIPLIER;
                leftBound = center - GameConstants.MIN_MOVEMENT_HALF_RANGE;
                rightBound = center + GameConstants.MIN_MOVEMENT_HALF_RANGE;
            }

            return (leftBound, rightBound);
        }

        private void OnDestroy() => _movingData.Clear();
        
        public override void OnPlayerStaying(Player.PlayerController player, BasePlatform platform)
        {
            if (_movingData.TryGetValue(platform, out var data) && data.wasMoving && data.rigidbody)
            {
                // Move player with platform using the same velocity
                var playerRb = player.GetComponent<Rigidbody2D>();
                if (playerRb)
                {
                    Vector2 newVelocity = playerRb.linearVelocity;
                    newVelocity.x = data.rigidbody.linearVelocity.x;
                    playerRb.linearVelocity = newVelocity;
                }
            }
        }
    }
}