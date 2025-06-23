using Constants;
using Pooling;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class PlatformGenerator : MonoBehaviour
    {
        [SerializeField] private float minDistance = 3f;
        [SerializeField] private float maxDistance = 4f;
        [SerializeField] private int platformsAhead = 8;
        [SerializeField] private float cleanupDistance = 20f;
        [SerializeField] private float minWidth = 3f;
        [SerializeField] private float maxWidth = 5f;
        [SerializeField] private float maxJumpX = 3f;
        [SerializeField] private float maxJumpY = 5f;

        private Camera _mainCamera;
        private float _screenHalfWidth;
        private float _lastCleanupY;
        private float _highestPlatformY;

        private void Start()
        {
            _mainCamera = Camera.main;
            _screenHalfWidth = _mainCamera.orthographicSize * _mainCamera.aspect;
        }

        public void Initialize(Vector3 playerPosition)
        {
            _lastCleanupY = playerPosition.y;
            _highestPlatformY = playerPosition.y;
            
            for (int i = 0; i < platformsAhead; i++)
            {
                _highestPlatformY += i == 0 ? 
                    minDistance * GameConstants.FIRST_PLATFORM_MULTIPLIER : 
                    Mathf.Min(Random.Range(minDistance, maxDistance), maxJumpY);
                GeneratePlatform(_highestPlatformY);
            }
        }

        public void UpdateGeneration(float playerY)
        {
            if (_highestPlatformY - playerY < platformsAhead * minDistance)
            {
                _highestPlatformY += Mathf.Min(Random.Range(minDistance, maxDistance), maxJumpY);
                GeneratePlatform(_highestPlatformY);
            }

            if (playerY - _lastCleanupY > cleanupDistance)
            {
                PlatformPool.Instance?.ReturnPlatformsBelowHeight(playerY - cleanupDistance);
                _lastCleanupY = playerY;
            }
        }

        public void ResetGeneration(Vector3 newPlayerPosition)
        {
            PlatformPool.Instance?.ClearAllPlatforms();
            Initialize(newPlayerPosition);
        }

        private void GeneratePlatform(float yPosition)
        {
            float width = Random.Range(minWidth, maxWidth);
            var bounds = CalculateBounds(width);
            var nearPlatforms = GetNearPlatforms(yPosition - maxDistance);
            
            if (nearPlatforms.Count > 0)
            {
                var refPos = nearPlatforms[Random.Range(0, nearPlatforms.Count)];
                bounds = ConstrainByJumpDistance(bounds, refPos.x);
            }
            
            for (int attempt = 0; attempt < GameConstants.MAX_PLACEMENT_ATTEMPTS; attempt++)
            {
                float x = Random.Range(bounds.min, bounds.max);
                Vector3 position = new Vector3(x, yPosition, 0f);
                
                if (IsValidPosition(position, width))
                {
                    PlatformPool.Instance?.GetPlatform(position)?.SetCustomWidth(width);
                    return;
                }
            }
        }

        private (float min, float max) CalculateBounds(float width)
        {
            float halfWidth = width * GameConstants.HALF_WIDTH_MULTIPLIER;
            float minX = -_screenHalfWidth + halfWidth + GameConstants.SCREEN_MARGIN;
            float maxX = _screenHalfWidth - halfWidth - GameConstants.SCREEN_MARGIN;
            return (minX, maxX);
        }

        private (float min, float max) ConstrainByJumpDistance((float min, float max) bounds, float refX)
        {
            float minX = Mathf.Max(bounds.min, refX - maxJumpX);
            float maxX = Mathf.Min(bounds.max, refX + maxJumpX);
            return (minX, maxX);
        }

        private List<Vector3> GetNearPlatforms(float yPosition)
        {
            var activePlatforms = PlatformPool.Instance?.GetActivePlatforms();
            if (activePlatforms == null) return new List<Vector3>();
            
            var nearPlatforms = new List<Vector3>();
            foreach (var platform in activePlatforms)
            {
                if (platform && Mathf.Abs(platform.transform.position.y - yPosition) < GameConstants.PLATFORM_SEARCH_RANGE)
                    nearPlatforms.Add(platform.transform.position);
            }
            
            return nearPlatforms;
        }

        private bool IsValidPosition(Vector3 position, float width)
        {
            var activePlatforms = PlatformPool.Instance?.GetActivePlatforms();
            if (activePlatforms == null) return true;

            float halfWidth = width * GameConstants.HALF_WIDTH_MULTIPLIER;
            
            foreach (var platform in activePlatforms)
            {
                if (!platform || Mathf.Abs(position.y - platform.transform.position.y) > GameConstants.VERTICAL_CHECK_RANGE) 
                    continue;
                
                float platformWidth = GetPlatformWidth(platform);
                float horizontalDistance = Mathf.Abs(position.x - platform.transform.position.x);
                
                if (horizontalDistance < halfWidth + platformWidth * GameConstants.HALF_WIDTH_MULTIPLIER + GameConstants.PLATFORM_SPACING) 
                    return false;
            }
            
            return true;
        }

        private float GetPlatformWidth(Controllers.Platform.BasePlatform platform)
        {
            var spriteRenderer = platform.GetComponent<SpriteRenderer>();
            return spriteRenderer ? spriteRenderer.size.x : GameConstants.DEFAULT_PLATFORM_WIDTH;
        }
    }
}