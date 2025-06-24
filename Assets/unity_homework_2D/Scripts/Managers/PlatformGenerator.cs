using Pooling;
using UnityEngine;

namespace Managers
{
    public class PlatformGenerator : MonoBehaviour
    {
        [SerializeField] private float minVerticalDistance = 2f;
        [SerializeField] private float maxVerticalDistance = 4f;
        [SerializeField] private float maxHorizontalDistance = 2f;
        [SerializeField] private int platformsAhead = 8;
        [SerializeField] private float cleanupDistance = 20f;

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
            var firstPlatform = PlatformPool.Instance?.GetPlatform();
            if (firstPlatform)
            {
                Vector3 position = new Vector3(playerPosition.x, _highestPlatformY, 0f);
                firstPlatform.SetPosition(position);
            }
            
            // Generate remaining platforms
            for (int i = 1; i < platformsAhead; i++)
            {
                GenerateNextPlatform();
            }
        }

        public void UpdateGeneration(float playerY)
        {
            if (_highestPlatformY - playerY < platformsAhead * minVerticalDistance)
            {
                GenerateNextPlatform();
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
            CoinPool.Instance?.ClearAllCoins();
            Initialize(newPlayerPosition);
        }

        private void GenerateNextPlatform()
        {
            _highestPlatformY += Random.Range(minVerticalDistance, maxVerticalDistance);
            _lastPlatformX = GenerateValidX();
            
            var platform = PlatformPool.Instance?.GetPlatform();
            if (platform)
            {
                Vector3 position = new Vector3(_lastPlatformX, _highestPlatformY, 0f);
                platform.SetPosition(position);
            }
        }

        private float GenerateValidX()
        {
            float margin = 1f;
            float minX = -_screenHalfWidth + margin;
            float maxX = _screenHalfWidth - margin;
            
            float minJumpX = Mathf.Max(minX, _lastPlatformX - maxHorizontalDistance);
            float maxJumpX = Mathf.Min(maxX, _lastPlatformX + maxHorizontalDistance);
            
            return Random.Range(minJumpX, maxJumpX);
        }
    }
}