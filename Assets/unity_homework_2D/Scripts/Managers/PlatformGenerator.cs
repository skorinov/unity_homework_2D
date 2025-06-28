using Constants;
using Controllers.Platform;
using Pooling;
using UnityEngine;

namespace Managers
{
    public class PlatformGenerator : MonoBehaviour
    {
        [SerializeField] private float minVerticalDistance = 3f;
        [SerializeField] private float maxVerticalDistance = 5f;
        [SerializeField] private float maxHorizontalDistance = 2f;
        [SerializeField] private int platformsAhead = 8;
        [SerializeField] private float cleanupDistance = 20f;
        
        [Header("Multi-Platform Generation")]
        [SerializeField] private float minPlatformSpacing = 3f;
        [SerializeField, Range(0f, 100f)] private float multiPlatformChance = 80f;
        [SerializeField] private int maxPlatformsPerLevel = 3;

        private Camera _mainCamera;
        private float _screenHalfWidth;
        private float _lastCleanupY;
        private float _highestPlatformY;
        private float _lastPlatformX;

        private void Start()
        {
            _mainCamera = Camera.main;
            _screenHalfWidth = _mainCamera.orthographicSize * _mainCamera.aspect;
        }

        public void Initialize(Vector3 playerPosition)
        {
            PlatformPool.Instance?.ClearAllPlatforms();
            CoinPool.Instance?.ClearAllCoins();
            
            _lastCleanupY = playerPosition.y;
            _highestPlatformY = playerPosition.y + 0.5f;
            _lastPlatformX = playerPosition.x;
            
            // Generate first platform at player position
            CreatePlatformAt(playerPosition.x, _highestPlatformY);
            
            // Generate remaining platforms
            for (int i = 1; i < platformsAhead; i++)
            {
                GenerateNextLevel();
            }
        }

        public void UpdateGeneration(float playerY)
        {
            if (_highestPlatformY - playerY < platformsAhead * minVerticalDistance)
            {
                GenerateNextLevel();
            }

            if (playerY - _lastCleanupY > cleanupDistance)
            {
                PlatformPool.Instance?.ReturnPlatformsBelowHeight(playerY - cleanupDistance);
                _lastCleanupY = playerY;
            }
        }

        private void GenerateNextLevel()
        {
            _highestPlatformY += Random.Range(minVerticalDistance, maxVerticalDistance);
            
            // Generate main platform
            _lastPlatformX = GenerateValidX();
            var mainPlatform = CreatePlatformAt(_lastPlatformX, _highestPlatformY);
            
            // Try to generate additional platforms on the same level
            if (Random.Range(0f, 100f) < multiPlatformChance)
            {
                GenerateAdditionalPlatforms(_highestPlatformY, mainPlatform);
            }
        }

        private void GenerateAdditionalPlatforms(float levelY, BasePlatform mainPlatform)
        {
            var occupiedRanges = new System.Collections.Generic.List<(float min, float max)>();
            
            // Add main platform range
            if (mainPlatform)
            {
                float mainWidth = GetPlatformWidth(mainPlatform);
                occupiedRanges.Add((_lastPlatformX - mainWidth * 0.5f, _lastPlatformX + mainWidth * 0.5f));
            }

            int additionalCount = Random.Range(1, maxPlatformsPerLevel);
            
            for (int i = 0; i < additionalCount; i++)
            {
                float? validX = FindValidPosition(occupiedRanges);
                if (validX.HasValue)
                {
                    var platform = CreatePlatformAt(validX.Value, levelY);
                    if (platform)
                    {
                        float platformWidth = GetPlatformWidth(platform);
                        occupiedRanges.Add((validX.Value - platformWidth * 0.5f, validX.Value + platformWidth * 0.5f));
                    }
                }
                else
                {
                    break; // No more space available
                }
            }
        }

        private float? FindValidPosition(System.Collections.Generic.List<(float min, float max)> occupiedRanges)
        {
            float margin = GameConstants.PLATFORM_GENERATION_MARGIN;
            float minX = -_screenHalfWidth + margin;
            float maxX = _screenHalfWidth - margin;
            float platformWidth = GameConstants.DEFAULT_PLATFORM_WIDTH; // Approximate platform width
            
            // Try multiple random positions
            for (int attempt = 0; attempt < GameConstants.MAX_PLACEMENT_ATTEMPTS; attempt++)
            {
                float candidateX = Random.Range(minX + platformWidth * GameConstants.HALF_WIDTH_MULTIPLIER, maxX - platformWidth * GameConstants.HALF_WIDTH_MULTIPLIER);
                
                if (IsPositionValid(candidateX, platformWidth, occupiedRanges))
                {
                    return candidateX;
                }
            }
            
            return null;
        }

        private bool IsPositionValid(float x, float width, System.Collections.Generic.List<(float min, float max)> occupiedRanges)
        {
            float halfWidth = width * GameConstants.HALF_WIDTH_MULTIPLIER;
            float minBound = x - halfWidth - minPlatformSpacing;
            float maxBound = x + halfWidth + minPlatformSpacing;
            
            foreach (var range in occupiedRanges)
            {
                if (!(maxBound < range.min || minBound > range.max))
                {
                    return false; // Overlaps with existing platform
                }
            }
            
            return true;
        }

        private BasePlatform CreatePlatformAt(float x, float y)
        {
            var platform = PlatformPool.Instance?.GetPlatform();
            if (platform)
            {
                Vector3 position = new Vector3(x, y, 0f);
                platform.SetPosition(position);
            }
            return platform;
        }

        private float GetPlatformWidth(BasePlatform platform)
        {
            if (!platform) return GameConstants.DEFAULT_PLATFORM_WIDTH;
            
            var boxCollider = platform.GetComponent<BoxCollider2D>();
            return boxCollider ? boxCollider.size.x * platform.transform.localScale.x : GameConstants.DEFAULT_PLATFORM_WIDTH;
        }

        private float GenerateValidX()
        {
            float margin = GameConstants.PLATFORM_GENERATION_MARGIN;
            float minX = -_screenHalfWidth + margin;
            float maxX = _screenHalfWidth - margin;
            
            float minJumpX = Mathf.Max(minX, _lastPlatformX - maxHorizontalDistance);
            float maxJumpX = Mathf.Min(maxX, _lastPlatformX + maxHorizontalDistance);
            
            return Random.Range(minJumpX, maxJumpX);
        }
    }
}