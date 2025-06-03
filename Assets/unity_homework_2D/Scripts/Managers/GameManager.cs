using UnityEngine;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        [Header("Game Settings")]
        [SerializeField] private Transform player;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float cameraFollowSpeed = 2f;
        [SerializeField] private float cameraOffsetY = 0f;
        [SerializeField] private float minCameraY = 0f;

        [Header("Platform Generation")]
        [SerializeField] private float platformSpawnDistance = 3f;
        [SerializeField] private float platformSpawnRangeX = 4f;
        [SerializeField] private int initialPlatformCount = 10;

        [Header("Platform Placement")]
        [SerializeField] private float minHorizontalDistance = 4f;
        [SerializeField] private float maxJumpDistance = 5f;
        [SerializeField] private int maxConsecutiveSameSide = 2;

        [Header("Optimization")]
        [SerializeField] private float platformCleanupOffset = 15f;

        // Height tracking event
        public System.Action<float> OnHeightChanged;

        // Core game state
        private float _highestPlayerY;
        private float _nextPlatformSpawnHeight;
        private float _lastPlatformX;
        private int _lastSide; // -1 left, 1 right, 0 uninitialized
        private int _consecutiveSameSide;
        
        // Cached screen boundaries for performance
        private float _screenHalfWidth;
        private float _effectiveMaxX;
        
        // Performance optimization variables
        private float _lastPlayerY;
        private float _platformCheckTimer;
        private const float PlatformCheckInterval = 0.1f;
        private const int MaxPlatformsPerFrame = 3;

        private void Start() => InitializeGame();

        private void InitializeGame()
        {
            if (!player)
            {
                Debug.LogError("Player Transform is not assigned in GameManager!");
                return;
            }

            // Initialize game state
            _highestPlayerY = player.position.y;
            _nextPlatformSpawnHeight = player.position.y + 1f;
            _lastPlatformX = player.position.x;
            
            // Notify UI systems
            OnHeightChanged?.Invoke(_highestPlayerY);
            
            // Setup game world
            UpdateCachedValues();
            GenerateInitialPlatforms();
        }

        private void Update()
        {
            // Early exit if missing critical components
            if (!player || !mainCamera) return;
            
            UpdateCamera();
            CheckPlatformGenerationOptimized();
            CleanupOldPlatforms();
        }

        // Smooth camera following with vertical offset
        private void UpdateCamera()
        {
            Vector3 targetPos = new Vector3(
                mainCamera.transform.position.x,
                Mathf.Max(player.position.y + cameraOffsetY, minCameraY),
                mainCamera.transform.position.z
            );

            mainCamera.transform.position = Vector3.Lerp(
                mainCamera.transform.position,
                targetPos,
                cameraFollowSpeed * Time.deltaTime
            );
        }

        // Optimized platform generation check - runs periodically instead of every frame
        private void CheckPlatformGenerationOptimized()
        {
            _platformCheckTimer += Time.deltaTime;
            
            // Check only periodically or when player moves up significantly
            if (_platformCheckTimer >= PlatformCheckInterval || 
                player.position.y > _lastPlayerY + 1f)
            {
                CheckPlatformGeneration();
                _platformCheckTimer = 0f;
                _lastPlayerY = player.position.y;
            }
        }

        private void CheckPlatformGeneration()
        {
            // Update highest point reached and notify UI
            if (player.position.y > _highestPlayerY)
            {
                _highestPlayerY = player.position.y;
                OnHeightChanged?.Invoke(_highestPlayerY);
            }

            // Spawn platforms ahead of player, limiting spawns per frame
            int platformsSpawned = 0;
            while (_highestPlayerY + 10f > _nextPlatformSpawnHeight && 
                   platformsSpawned < MaxPlatformsPerFrame)
            {
                SpawnPlatform();
                platformsSpawned++;
            }
        }

        // Remove platforms that are far below camera view
        private void CleanupOldPlatforms()
        {
            float cleanupHeight = mainCamera.transform.position.y - 
                                 mainCamera.orthographicSize - platformCleanupOffset;
            PlatformPool.Instance?.ReturnPlatformsBelowHeight(cleanupHeight);
        }

        // Create initial set of platforms when game starts
        private void GenerateInitialPlatforms()
        {
            for (int i = 0; i < initialPlatformCount; i++)
            {
                SpawnPlatform();
            }
        }

        private void SpawnPlatform()
        {
            // Update screen bounds only when camera size changes
            if (Mathf.Abs(mainCamera.orthographicSize * mainCamera.aspect - _screenHalfWidth) > 0.01f)
            {
                UpdateCachedValues();
            }
            
            // Calculate optimal position and spawn platform
            float spawnX = CalculateOptimalPlatformX();
            Vector3 spawnPos = new Vector3(spawnX, _nextPlatformSpawnHeight, 0f);

            PlatformPool.Instance?.GetPlatform(spawnPos);
            UpdatePlatformTracking(spawnX);
            _nextPlatformSpawnHeight += platformSpawnDistance;
        }

        // Cache screen boundaries for performance
        private void UpdateCachedValues()
        {
            _screenHalfWidth = mainCamera.orthographicSize * mainCamera.aspect;
            _effectiveMaxX = Mathf.Min(_screenHalfWidth - 0.5f, platformSpawnRangeX);
        }

        // Smart platform positioning algorithm
        private float CalculateOptimalPlatformX()
        {
            // First platform or random placement
            if (_lastSide == 0) return GetRandomSidePosition();

            // Force side switch if too many consecutive platforms on same side
            bool forceSideSwitch = _consecutiveSameSide >= maxConsecutiveSameSide;
            float targetX = forceSideSwitch || Random.value < 0.7f 
                ? GetOppositeSidePosition() 
                : GetSameSidePosition();

            return ClampToJumpableDistance(targetX);
        }

        // Generate random position on either side
        private float GetRandomSidePosition() =>
            Random.value < 0.5f 
                ? Random.Range(-_effectiveMaxX, -0.1f) 
                : Random.Range(0.1f, _effectiveMaxX);

        // Generate position on opposite side from last platform
        private float GetOppositeSidePosition() =>
            _lastSide == -1 
                ? Random.Range(minHorizontalDistance, _effectiveMaxX)
                : Random.Range(-_effectiveMaxX, -minHorizontalDistance);

        // Generate position on same side as last platform
        private float GetSameSidePosition()
        {
            if (_lastSide == -1)
            {
                float minX = Mathf.Min(_lastPlatformX - minHorizontalDistance, -0.1f);
                return Random.Range(-_effectiveMaxX, minX);
            }
            
            float maxX = Mathf.Max(_lastPlatformX + minHorizontalDistance, 0.1f);
            return Random.Range(maxX, _effectiveMaxX);
        }

        // Ensure platform is within jumpable distance
        private float ClampToJumpableDistance(float targetX)
        {
            float distance = Mathf.Abs(targetX - _lastPlatformX);
            if (distance <= maxJumpDistance) return targetX;

            // Clamp to maximum jump distance
            float direction = Mathf.Sign(targetX - _lastPlatformX);
            targetX = _lastPlatformX + direction * maxJumpDistance;
            targetX = Mathf.Clamp(targetX, -_effectiveMaxX, _effectiveMaxX);

            // Avoid platforms too close to center
            return Mathf.Abs(targetX) < 0.1f ? (targetX >= 0 ? 0.1f : -0.1f) : targetX;
        }

        // Track platform placement for side alternation logic
        private void UpdatePlatformTracking(float platformX)
        {
            int newSide = platformX < 0 ? -1 : 1;
            _consecutiveSameSide = newSide == _lastSide ? _consecutiveSameSide + 1 : 1;
            _lastSide = newSide;
            _lastPlatformX = platformX;
        }
        
        public float GetHighestPlayerY() => _highestPlayerY;
        public Transform GetPlayer() => player;
        public float GetLastPlatformX() => _lastPlatformX;
        public int GetConsecutiveSameSide() => _consecutiveSameSide;
        
        // Reset game state for restart
        public void ResetGame()
        {
            if (!player) return;
            
            // Reset tracking variables
            _highestPlayerY = player.position.y;
            _nextPlatformSpawnHeight = player.position.y + 1f;
            _lastPlatformX = player.position.x;
            _lastSide = 0;
            _consecutiveSameSide = 0;
            _platformCheckTimer = 0f;
            
            // Notify systems and regenerate platforms
            OnHeightChanged?.Invoke(_highestPlayerY);
            UpdateCachedValues();
            GenerateInitialPlatforms();
        }
    }
}