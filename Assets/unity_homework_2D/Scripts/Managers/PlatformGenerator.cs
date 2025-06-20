using Pooling;
using UnityEngine;

namespace Managers
{
    public class PlatformGenerator : MonoBehaviour
    {
        [Header("Ground Reference")]
        [SerializeField] private Transform groundObject;
        [SerializeField] private float groundMargin = 2f;

        [Header("Spawn Settings")]
        [SerializeField] private float minPlatformDistance = 3f;
        [SerializeField] private float maxPlatformDistance = 5f;
        [SerializeField] private int platformsAhead = 6;
        [SerializeField] private float cleanupDistance = 20f;

        [Header("Platform Size")]
        [SerializeField] private float minPlatformWidth = 3f;
        [SerializeField] private float maxPlatformWidth = 5f;
        [SerializeField] private float platformHeight = 0.5f;

        [Header("Jump Reachability")]
        [SerializeField] private float maxJumpDistanceX = 3f; // Max horizontal jump distance
        [SerializeField] private float maxJumpDistanceY = 5f; // Max vertical jump distance

        [Header("Multi-Platform Settings")]
        [SerializeField] private float sameLevelChance = 0.3f;
        [SerializeField] private int maxPlatformsPerLevel = 3;
        [SerializeField] private float minDistanceBetweenPlatforms = 2f;

        [Header("Collision Detection")]
        [SerializeField] private float safetyMargin = 0.3f;

        private Camera _mainCamera;
        private float _screenLeftBound;
        private float _screenRightBound;
        private float _lastPlayerY;
        private float _lastCleanupY;
        private float _minGenerationY;
        private float _highestActivePlatformY = float.MinValue;
        private float _lowestActivePlatformY = float.MaxValue;

        private void Start()
        {
            _mainCamera = Camera.main;
            CalculateScreenBounds();
            _minGenerationY = groundObject ? groundObject.position.y + groundMargin : float.MinValue;
        }

        public void Initialize(Vector3 playerPosition)
        {
            _lastCleanupY = playerPosition.y;
            _lastPlayerY = playerPosition.y;
            _highestActivePlatformY = float.MinValue;
            _lowestActivePlatformY = float.MaxValue;
            
            GenerateInitialPlatforms(playerPosition.y);
        }

        public void UpdateGeneration(float playerY)
        {
            float avgDistance = (minPlatformDistance + maxPlatformDistance) * 0.5f;
            UpdateActivePlatformBoundaries();
            
            // Generate upward
            if (_highestActivePlatformY - playerY < platformsAhead * avgDistance)
                TryGeneratePlatformLevel(true);
            
            // Generate downward (only when moving down and above ground)
            if (_lastPlayerY - playerY > 0.1f && 
                playerY - _lowestActivePlatformY < platformsAhead * avgDistance && 
                _lowestActivePlatformY > _minGenerationY)
            {
                TryGeneratePlatformLevel(false);
            }
            
            // Cleanup distant platforms
            if (Mathf.Abs(playerY - _lastCleanupY) > cleanupDistance)
            {
                PlatformPool.Instance?.ReturnPlatformsOutsideRange(
                    playerY - cleanupDistance, 
                    playerY + cleanupDistance
                );
                _lastCleanupY = playerY;
            }
            
            _lastPlayerY = playerY;
        }

        public void ResetGeneration(Vector3 newPlayerPosition)
        {
            PlatformPool.Instance?.ClearAllPlatforms();
            CalculateScreenBounds();
            _minGenerationY = groundObject ? groundObject.position.y + groundMargin : float.MinValue;
            Initialize(newPlayerPosition);
        }

        private void CalculateScreenBounds()
        {
            if (!_mainCamera) return;
            
            float screenHalfWidth = _mainCamera.orthographicSize * _mainCamera.aspect;
            _screenLeftBound = -screenHalfWidth + safetyMargin;
            _screenRightBound = screenHalfWidth - safetyMargin;
        }

        private void UpdateActivePlatformBoundaries()
        {
            var activePlatforms = PlatformPool.Instance?.GetActivePlatforms();
            if (activePlatforms == null || activePlatforms.Count == 0)
            {
                _highestActivePlatformY = _lastPlayerY;
                _lowestActivePlatformY = Mathf.Max(_lastPlayerY - 10f, _minGenerationY);
                return;
            }

            _highestActivePlatformY = float.MinValue;
            _lowestActivePlatformY = float.MaxValue;

            foreach (var platform in activePlatforms)
            {
                if (!platform) continue;
                
                float platformY = platform.transform.position.y;
                if (platformY > _highestActivePlatformY) _highestActivePlatformY = platformY;
                if (platformY < _lowestActivePlatformY) _lowestActivePlatformY = platformY;
            }
        }

        private void GenerateInitialPlatforms(float startY)
        {
            // Start much closer to player
            float currentY = startY;
            Vector3 lastPlatformPos = new Vector3(0f, startY, 0f); // Assume player starts at center
            
            for (int i = 0; i < platformsAhead; i++)
            {
                var platformPos = TryPlaceReachablePlatform(currentY, lastPlatformPos);
                if (platformPos.HasValue)
                {
                    lastPlatformPos = platformPos.Value;
                    currentY += Random.Range(minPlatformDistance, maxPlatformDistance);
                }
                else
                {
                    currentY += minPlatformDistance; // Skip this level if can't place
                }
            }
        }

        private void TryGeneratePlatformLevel(bool isUpward)
        {
            float distance = Random.Range(minPlatformDistance, maxPlatformDistance);
            float targetY = isUpward ? _highestActivePlatformY + distance : _lowestActivePlatformY - distance;
            
            // Check limits
            if (targetY < _minGenerationY) return;
            if (!HasVerticalSpace(targetY)) return;
            
            // Get platforms from previous level for reachability check
            var referencePlatforms = GetPlatformsAtLevel(isUpward ? _highestActivePlatformY : _lowestActivePlatformY);
            
            int platformCount = Random.value < sameLevelChance ? Random.Range(2, maxPlatformsPerLevel + 1) : 1;
            
            bool success = false;
            if (platformCount == 1)
            {
                success = TryPlaceReachablePlatformFromLevel(targetY, referencePlatforms);
            }
            else
            {
                success = TryPlaceMultipleReachablePlatforms(targetY, platformCount, referencePlatforms);
            }
            
            // Only update boundaries if successful
            if (success)
            {
                if (isUpward) _highestActivePlatformY = targetY;
                else _lowestActivePlatformY = targetY;
            }
        }

        private System.Collections.Generic.List<Vector3> GetPlatformsAtLevel(float yLevel)
        {
            var platforms = new System.Collections.Generic.List<Vector3>();
            var activePlatforms = PlatformPool.Instance?.GetActivePlatforms();
            
            if (activePlatforms == null) return platforms;
            
            foreach (var platform in activePlatforms)
            {
                if (platform && Mathf.Abs(platform.transform.position.y - yLevel) < 0.5f)
                {
                    platforms.Add(platform.transform.position);
                }
            }
            
            return platforms;
        }

        private bool IsReachableFrom(Vector3 fromPos, Vector3 toPos)
        {
            float horizontalDistance = Mathf.Abs(toPos.x - fromPos.x);
            float verticalDistance = toPos.y - fromPos.y; // Only upward jumps
            
            return horizontalDistance <= maxJumpDistanceX && 
                   verticalDistance > 0 && verticalDistance <= maxJumpDistanceY;
        }

        private bool HasVerticalSpace(float yPosition)
        {
            var activePlatforms = PlatformPool.Instance?.GetActivePlatforms();
            if (activePlatforms == null) return true;

            float checkDistance = platformHeight + safetyMargin;
            
            foreach (var platform in activePlatforms)
            {
                if (platform && Mathf.Abs(yPosition - platform.transform.position.y) < checkDistance)
                    return false;
            }
            return true;
        }

        private bool CanPlacePlatform(Vector3 position, float width)
        {
            float halfWidth = width * 0.5f;
            
            // Check screen bounds
            if (position.x - halfWidth < _screenLeftBound || position.x + halfWidth > _screenRightBound)
                return false;
            
            // Check collision with existing platforms
            var activePlatforms = PlatformPool.Instance?.GetActivePlatforms();
            if (activePlatforms == null) return true;

            float safeDistanceH = halfWidth + safetyMargin;
            float safeDistanceV = platformHeight * 0.5f + safetyMargin;
            
            foreach (var platform in activePlatforms)
            {
                if (!platform) continue;
                
                Vector3 platformPos = platform.transform.position;
                var spriteRenderer = platform.GetComponent<SpriteRenderer>();
                float platformWidth = spriteRenderer ? spriteRenderer.size.x : 2f;
                float platformHalfWidth = platformWidth * 0.5f + safetyMargin;
                
                float horizontalDistance = Mathf.Abs(position.x - platformPos.x);
                float verticalDistance = Mathf.Abs(position.y - platformPos.y);
                
                if (horizontalDistance < (safeDistanceH + platformHalfWidth) && 
                    verticalDistance < safeDistanceV * 2f)
                {
                    return false;
                }
            }
            
            return true;
        }

        private Vector3? TryPlaceReachablePlatform(float yPosition, Vector3 referencePos)
        {
            float width = Random.Range(minPlatformWidth, maxPlatformWidth);
            
            // Calculate reachable X range from reference position
            float minReachableX = referencePos.x - maxJumpDistanceX;
            float maxReachableX = referencePos.x + maxJumpDistanceX;
            
            // Constrain to screen bounds
            float halfWidth = width * 0.5f;
            float minX = Mathf.Max(_screenLeftBound + halfWidth, minReachableX);
            float maxX = Mathf.Min(_screenRightBound - halfWidth, maxReachableX);
            
            if (minX >= maxX) return null;
            
            // Try positions within reachable range
            for (int attempt = 0; attempt < 10; attempt++)
            {
                float x = Random.Range(minX, maxX);
                Vector3 position = new Vector3(x, yPosition, 0f);
                
                if (CanPlacePlatform(position, width) && IsReachableFrom(referencePos, position))
                {
                    CreatePlatform(position, width);
                    return position;
                }
            }
            
            return null;
        }

        private bool TryPlaceReachablePlatformFromLevel(float yPosition, System.Collections.Generic.List<Vector3> referencePlatforms)
        {
            if (referencePlatforms.Count == 0) 
                return TryPlaceReachablePlatform(yPosition, Vector3.zero).HasValue;
            
            // Try to place at least one reachable platform from any reference platform
            foreach (var refPos in referencePlatforms)
            {
                var result = TryPlaceReachablePlatform(yPosition, refPos);
                if (result.HasValue) return true;
            }
            
            return false;
        }

        private bool TryPlaceMultipleReachablePlatforms(float yPosition, int count, System.Collections.Generic.List<Vector3> referencePlatforms)
        {
            // First, ensure at least one platform is reachable
            bool hasReachablePlatform = TryPlaceReachablePlatformFromLevel(yPosition, referencePlatforms);
            if (!hasReachablePlatform) return false;
            
            // Try to place additional platforms
            var occupiedRanges = new System.Collections.Generic.List<(float min, float max)>();
            
            // Get occupied range from the already placed platform
            var currentPlatforms = GetPlatformsAtLevel(yPosition);
            foreach (var platformPos in currentPlatforms)
            {
                float width = 2f; // Approximate width, could be more precise
                float halfWidth = width * 0.5f;
                occupiedRanges.Add((platformPos.x - halfWidth - minDistanceBetweenPlatforms * 0.5f, 
                                   platformPos.x + halfWidth + minDistanceBetweenPlatforms * 0.5f));
            }
            
            // Try to place remaining platforms
            for (int i = 1; i < count; i++)
            {
                float width = Random.Range(minPlatformWidth, maxPlatformWidth);
                bool placed = false;
                
                for (int attempt = 0; attempt < 8; attempt++)
                {
                    float halfWidth = width * 0.5f;
                    float minX = _screenLeftBound + halfWidth;
                    float maxX = _screenRightBound - halfWidth;
                    
                    if (minX >= maxX) break;
                    
                    float x = Random.Range(minX, maxX);
                    Vector3 position = new Vector3(x, yPosition, 0f);
                    
                    if (!CanPlacePlatform(position, width)) continue;
                    
                    // Check conflicts with platforms on same level
                    float rangeMin = x - halfWidth - minDistanceBetweenPlatforms * 0.5f;
                    float rangeMax = x + halfWidth + minDistanceBetweenPlatforms * 0.5f;
                    
                    bool hasConflict = false;
                    foreach (var range in occupiedRanges)
                    {
                        if (!(rangeMax < range.min || rangeMin > range.max))
                        {
                            hasConflict = true;
                            break;
                        }
                    }
                    
                    if (!hasConflict)
                    {
                        CreatePlatform(position, width);
                        occupiedRanges.Add((rangeMin, rangeMax));
                        placed = true;
                        break;
                    }
                }
                
                if (!placed) break; // Stop trying if we can't place more
            }
            
            return true; // Success as long as we have at least one reachable platform
        }

        private void CreatePlatform(Vector3 position, float width)
        {
            var platform = PlatformPool.Instance?.GetPlatform(position);
            platform?.SetCustomWidth(width);
        }
    }
}