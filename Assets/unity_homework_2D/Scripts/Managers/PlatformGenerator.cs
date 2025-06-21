using Pooling;
using UnityEngine;

namespace Managers
{
    public class PlatformGenerator : MonoBehaviour
    {
        [Header("Generation Settings")]
        [SerializeField] private float minDistance = 3f;
        [SerializeField] private float maxDistance = 4f;
        [SerializeField] private int platformsAhead = 8;
        [SerializeField] private float cleanupDistance = 20f;

        [Header("Platform Size")]
        [SerializeField] private float minWidth = 3f;
        [SerializeField] private float maxWidth = 5f;
        [SerializeField] private float maxJumpX = 3f;
        [SerializeField] private float maxJumpY = 5f;

        private const float SCREEN_MARGIN = 0.3f;
        private const float PLATFORM_SEARCH_RANGE = 2f;
        private const float PLATFORM_SPACING = 0.5f;
        private const float VERTICAL_CHECK_RANGE = 1f;
        private const float DEFAULT_PLATFORM_WIDTH = 2f;
        private const float FIRST_PLATFORM_MULTIPLIER = 0.5f;
        private const float HALF_WIDTH_MULTIPLIER = 0.5f;
        private const int MAX_PLACEMENT_ATTEMPTS = 10;

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
            
            // Generate platforms one by one using same logic as UpdateGeneration
            for (int i = 0; i < platformsAhead; i++)
            {
                _highestPlatformY += i == 0 ? minDistance * FIRST_PLATFORM_MULTIPLIER : 
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
            float halfWidth = width * HALF_WIDTH_MULTIPLIER;
            float minX = -_screenHalfWidth + halfWidth + SCREEN_MARGIN;
            float maxX = _screenHalfWidth - halfWidth - SCREEN_MARGIN;
            
            var nearPlatforms = GetNearPlatforms(yPosition - maxDistance);
            if (nearPlatforms.Length > 0)
            {
                var refPos = nearPlatforms[Random.Range(0, nearPlatforms.Length)];
                minX = Mathf.Max(minX, refPos.x - maxJumpX);
                maxX = Mathf.Min(maxX, refPos.x + maxJumpX);
            }
            
            for (int attempt = 0; attempt < MAX_PLACEMENT_ATTEMPTS; attempt++)
            {
                float x = Random.Range(minX, maxX);
                Vector3 position = new Vector3(x, yPosition, 0f);
                
                if (IsValidPosition(position, width))
                {
                    PlatformPool.Instance?.GetPlatform(position)?.SetCustomWidth(width);
                    return;
                }
            }
        }

        private Vector3[] GetNearPlatforms(float yPosition)
        {
            var activePlatforms = PlatformPool.Instance?.GetActivePlatforms();
            if (activePlatforms == null) return new Vector3[0];
            
            var nearPlatforms = new System.Collections.Generic.List<Vector3>();
            foreach (var platform in activePlatforms)
            {
                if (platform && Mathf.Abs(platform.transform.position.y - yPosition) < PLATFORM_SEARCH_RANGE)
                    nearPlatforms.Add(platform.transform.position);
            }
            
            return nearPlatforms.ToArray();
        }

        private bool IsValidPosition(Vector3 position, float width)
        {
            var activePlatforms = PlatformPool.Instance?.GetActivePlatforms();
            if (activePlatforms == null) return true;

            float halfWidth = width * HALF_WIDTH_MULTIPLIER;
            foreach (var platform in activePlatforms)
            {
                if (!platform || Mathf.Abs(position.y - platform.transform.position.y) > VERTICAL_CHECK_RANGE) continue;
                
                float platformWidth = platform.GetComponent<SpriteRenderer>()?.size.x ?? DEFAULT_PLATFORM_WIDTH;
                float horizontalDistance = Mathf.Abs(position.x - platform.transform.position.x);
                
                if (horizontalDistance < halfWidth + platformWidth * HALF_WIDTH_MULTIPLIER + PLATFORM_SPACING) return false;
            }
            
            return true;
        }
    }
}