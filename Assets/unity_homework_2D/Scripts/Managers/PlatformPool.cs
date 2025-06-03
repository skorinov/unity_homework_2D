using System.Collections.Generic;
using UnityEngine;
using Controllers.Platform;

namespace Managers
{
    /// Object pool system for platform management using Singleton pattern
    /// Provides efficient platform creation, reuse, and cleanup
    public class PlatformPool : MonoBehaviour
    {
        [Header("Pool Settings")]
        [SerializeField] private int poolSize = 30;
        [SerializeField] private PlatformData[] platformTypes;

        // Singleton instance
        public static PlatformPool Instance { get; private set; }

        // Pool collections for object management
        private readonly Queue<GameObject> _platformPool = new Queue<GameObject>();
        private readonly List<GameObject> _activePlatforms = new List<GameObject>();
        
        // Component caching for performance optimization
        private readonly Dictionary<GameObject, CachedComponents> _componentCache = new Dictionary<GameObject, CachedComponents>();
        private readonly Dictionary<GameObject, PlatformData> _platformDataCache = new Dictionary<GameObject, PlatformData>();
        
        // Cached total spawn chance for performance
        private float _totalSpawnChance;
        
        /// Cached components structure to reduce GetComponent calls
        private struct CachedComponents
        {
            public SpriteRenderer spriteRenderer;
            public BoxCollider2D boxCollider;
            public BasePlatform platformComponent;
        }
        
        /// Platform configuration data for different platform types
        [System.Serializable]
        public class PlatformData
        {
            public string platformName;
            public GameObject prefab;
            public float minWidth = 1f;
            public float maxWidth = 3f;
            [Range(0f, 100f)] public float spawnChance = 25f;
        }

        private void Awake()
        {
            // Singleton setup with scene persistence
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start() => InitializePool();
        
        /// Initialize the object pool with pre-created platforms
        private void InitializePool()
        {
            CalculateTotalSpawnChance();
            
            // Pre-populate pool with inactive platforms
            for (int i = 0; i < poolSize; i++)
            {
                GameObject platform = CreatePlatform();
                if (platform)
                {
                    platform.SetActive(false);
                    _platformPool.Enqueue(platform);
                }
            }
        }
        
        /// Calculate total spawn chance for weighted random selection
        private void CalculateTotalSpawnChance()
        {
            _totalSpawnChance = 0f;
            foreach (var data in platformTypes)
            {
                _totalSpawnChance += data.spawnChance;
            }
        }
        
        /// Create new platform instance with cached components
        private GameObject CreatePlatform()
        {
            var platformData = GetRandomPlatformType();
            if (!platformData?.prefab) return null;

            GameObject platform = Instantiate(platformData.prefab, transform);
            
            CacheComponents(platform, platformData);
            SetPlatformSize(platform, platformData);
            
            return platform;
        }
        
        /// Cache all necessary components to minimize GetComponent calls
        private void CacheComponents(GameObject platform, PlatformData data)
        {
            var cachedComponents = new CachedComponents
            {
                spriteRenderer = platform.GetComponent<SpriteRenderer>(),
                boxCollider = platform.GetComponent<BoxCollider2D>(),
                platformComponent = platform.GetComponent<BasePlatform>()
            };
            
            _componentCache[platform] = cachedComponents;
            _platformDataCache[platform] = data;
        }
        
        /// Select random platform type based on spawn chances (weighted random)
        private PlatformData GetRandomPlatformType()
        {
            if (_totalSpawnChance <= 0f)
            {
                return platformTypes.Length > 0 ? platformTypes[0] : null;
            }

            float random = Random.Range(0f, _totalSpawnChance);
            foreach (var data in platformTypes)
            {
                if (random < data.spawnChance) return data;
                random -= data.spawnChance;
            }
            return platformTypes[0]; // Fallback
        }
        
        /// Set platform size based on configuration using cached components
        private void SetPlatformSize(GameObject platform, PlatformData data)
        {
            if (!platform || !_componentCache.TryGetValue(platform, out var cached)) return;

            float randomWidth = Random.Range(data.minWidth, data.maxWidth);
            float originalWidth = cached.spriteRenderer?.sprite ? 
                cached.spriteRenderer.sprite.bounds.size.x : 1f;

            float scaleX = randomWidth / originalWidth;
            var scale = platform.transform.localScale;
            platform.transform.localScale = new Vector3(scaleX, scale.y, scale.z);

            // Adjust collider offset using cached component
            if (cached.boxCollider)
            {
                cached.boxCollider.offset = new Vector2(0f, cached.boxCollider.offset.y);
            }
        }
        
        /// Get platform from pool or create new one if pool is empty
        public GameObject GetPlatform(Vector3 position)
        {
            GameObject platform = _platformPool.Count == 0 ? 
                CreatePlatform() : 
                _platformPool.Dequeue();
                
            if (!platform) return null;

            // Reset platform if reused from pool
            if (_platformPool.Count > 0)
            {
                RegeneratePlatform(platform);
            }

            // Setup platform for use
            platform.transform.position = position;
            platform.SetActive(true);
            platform.tag = "Platform";
            _activePlatforms.Add(platform);
            
            return platform;
        }
        
        /// Return platform to pool for reuse
        public void ReturnPlatform(GameObject platform)
        {
            if (!platform) return;

            _activePlatforms.Remove(platform);
            platform.SetActive(false);
            _platformPool.Enqueue(platform);
        }
        
        /// Cleanup platforms below specified height for memory optimization
        public void ReturnPlatformsBelowHeight(float height)
        {
            // Iterate backwards to safely remove items while iterating
            for (int i = _activePlatforms.Count - 1; i >= 0; i--)
            {
                if (!_activePlatforms[i])
                {
                    _activePlatforms.RemoveAt(i);
                    continue;
                }

                if (_activePlatforms[i].transform.position.y < height)
                {
                    ReturnPlatform(_activePlatforms[i]);
                }
            }
        }
        
        /// Clear all active platforms
        public void ClearAllPlatforms()
        {
            while (_activePlatforms.Count > 0)
            {
                var platform = _activePlatforms[0];
                if (platform)
                {
                    ReturnPlatform(platform);
                }
                else
                {
                    _activePlatforms.RemoveAt(0);
                }
            }
        }
        
        /// Reset and resize platform when reusing from pool
        private void RegeneratePlatform(GameObject platform)
        {
            // Use cached BasePlatform component for reset
            if (_componentCache.TryGetValue(platform, out var cached))
            {
                cached.platformComponent?.ResetPlatform();
            }
            
            // Use cached PlatformData or get new random type for variety
            var data = _platformDataCache.TryGetValue(platform, out var cachedData) ? 
                cachedData : GetRandomPlatformType();
            SetPlatformSize(platform, data);
        }
        
        /// Clear all cached data to prevent memory leaks
        private void OnDestroy()
        {
            _componentCache.Clear();
            _platformDataCache.Clear();

            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}