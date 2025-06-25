using Constants;
using System.Collections.Generic;
using UnityEngine;

namespace Controllers.Platform.Actions
{
    [CreateAssetMenu(fileName = "IsolatedMovingAction", menuName = "Platform Actions/Isolated Moving Action")]
    public class IsolatedMovingAction : ConditionalPlatformAction
    {
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float moveRange = 3f;
        [SerializeField] private bool startMovingRight = true;

        private readonly Dictionary<BasePlatform, MovingData> _movingData = new();
        private Camera _mainCamera;

        private struct MovingData
        {
            public float leftBound;
            public float rightBound;
            public int direction;
            public Rigidbody2D rigidbody;
        }

        public override bool ShouldActivate(PlatformContext context)
        {
            // Activate movement only if platform is horizontally isolated
            return context.IsHorizontallyIsolated;
        }

        protected override void OnActivated(BasePlatform platform)
        {
            if (!_mainCamera) _mainCamera = Camera.main;

            var rb = platform.GetComponent<Rigidbody2D>();
            if (!rb)
            {
                rb = platform.gameObject.AddComponent<Rigidbody2D>();
            }
            
            SetupRigidbody(rb);

            var bounds = CalculateBounds(platform);
            
            _movingData[platform] = new MovingData
            {
                leftBound = bounds.min,
                rightBound = bounds.max,
                direction = startMovingRight ? 1 : -1,
                rigidbody = rb
            };
        }

        protected override void OnDeactivated(BasePlatform platform)
        {
            if (_movingData.ContainsKey(platform))
            {
                var data = _movingData[platform];
                if (data.rigidbody && data.rigidbody.bodyType == RigidbodyType2D.Kinematic)
                {
                    // Reset rigidbody to static if it was kinematic
                    data.rigidbody.bodyType = RigidbodyType2D.Static;
                }
                _movingData.Remove(platform);
            }
        }

        protected override void OnConditionalUpdate(BasePlatform platform)
        {
            if (!_movingData.TryGetValue(platform, out var data) || !data.rigidbody) return;

            Vector2 currentPos = data.rigidbody.position;
            float newX = currentPos.x + data.direction * moveSpeed * Time.fixedDeltaTime;

            if (newX >= data.rightBound)
            {
                data.direction = -1;
                newX = data.rightBound;
            }
            else if (newX <= data.leftBound)
            {
                data.direction = 1;
                newX = data.leftBound;
            }

            _movingData[platform] = data;
            data.rigidbody.MovePosition(new Vector2(newX, currentPos.y));
        }

        public override void OnReset(BasePlatform platform)
        {
            OnDeactivated(platform);
            base.OnReset(platform);
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
            float platformHalfWidth = boxCollider ? boxCollider.size.x * platform.transform.localScale.x * 0.5f : 0.5f;
            
            float currentX = platform.transform.position.x;
            float halfRange = moveRange * 0.5f;
            
            float screenLeft = -screenHalfWidth + GameConstants.SCREEN_MARGIN + platformHalfWidth;
            float screenRight = screenHalfWidth - GameConstants.SCREEN_MARGIN - platformHalfWidth;
            
            float leftBound = Mathf.Max(currentX - halfRange, screenLeft);
            float rightBound = Mathf.Min(currentX + halfRange, screenRight);
            
            if (rightBound - leftBound < 1f)
            {
                float center = (leftBound + rightBound) * 0.5f;
                leftBound = center - 0.5f;
                rightBound = center + 0.5f;
            }

            return (leftBound, rightBound);
        }

        private void OnDestroy() => _movingData.Clear();
    }
}