using Controllers.Platform;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Pooling
{
    [System.Serializable]
    public class PlatformPrefabData
    {
        public GameObject prefab;
        [Range(0f, 100f)] public float spawnChance = 25f;
        public int poolSize = 5;
        
        [System.NonSerialized]
        public ObjectPool<BasePlatform> pool;
    }

    public class PlatformPool : Singleton<PlatformPool>
    {
        [SerializeField] private PlatformPrefabData[] platformPrefabs;

        private readonly List<BasePlatform> _activePlatforms = new();
        private readonly Dictionary<BasePlatform, int> _platformToPoolIndex = new();
        private float _totalSpawnWeight;

        public int ActiveCount => _activePlatforms.Count;
        public List<BasePlatform> GetActivePlatforms() => _activePlatforms;

        protected override void OnSingletonAwake()
        {
            InitializePools();
            CalculateTotalWeight();
        }

        public BasePlatform GetPlatform()
        {
            var selectedIndex = SelectRandomPrefabIndex();
            if (selectedIndex == -1 || platformPrefabs[selectedIndex].pool == null) return null;

            var platform = platformPrefabs[selectedIndex].pool.Get();
            if (!platform) return null;

            platform.transform.position = Vector3.zero;
            _activePlatforms.Add(platform);
            _platformToPoolIndex[platform] = selectedIndex;

            return platform;
        }

        public void ReturnPlatform(BasePlatform platform)
        {
            if (!platform || !_platformToPoolIndex.TryGetValue(platform, out int poolIndex)) return;

            _activePlatforms.Remove(platform);
            _platformToPoolIndex.Remove(platform);

            if (poolIndex >= 0 && poolIndex < platformPrefabs.Length && platformPrefabs[poolIndex].pool != null)
            {
                platformPrefabs[poolIndex].pool.Return(platform);
            }
        }

        public void ReturnPlatformsBelowHeight(float height)
        {
            for (int i = _activePlatforms.Count - 1; i >= 0; i--)
            {
                if (_activePlatforms[i] && _activePlatforms[i].transform.position.y < height)
                {
                    ReturnPlatform(_activePlatforms[i]);
                }
            }
        }

        public void ClearAllPlatforms()
        {
            var platformsToReturn = new List<BasePlatform>(_activePlatforms);
            foreach (var platform in platformsToReturn)
            {
                if (platform) ReturnPlatform(platform);
            }
            
            _activePlatforms.Clear();
            _platformToPoolIndex.Clear();
        }

        private void InitializePools()
        {
            for (int i = 0; i < platformPrefabs.Length; i++)
            {
                var prefabData = platformPrefabs[i];
                if (!prefabData.prefab?.GetComponent<BasePlatform>()) continue;

                prefabData.pool = new ObjectPool<BasePlatform>(
                    prefabData.prefab, 
                    transform, 
                    prefabData.poolSize
                );
            }
        }

        private void CalculateTotalWeight()
        {
            _totalSpawnWeight = 0f;
            foreach (var prefabData in platformPrefabs)
            {
                if (prefabData.pool != null)
                    _totalSpawnWeight += prefabData.spawnChance;
            }
        }

        private int SelectRandomPrefabIndex()
        {
            if (_totalSpawnWeight <= 0f) return 0;

            float random = Random.Range(0f, _totalSpawnWeight);
            
            for (int i = 0; i < platformPrefabs.Length; i++)
            {
                var prefabData = platformPrefabs[i];
                if (prefabData.pool == null) continue;
                
                if (random < prefabData.spawnChance)
                    return i;
                    
                random -= prefabData.spawnChance;
            }

            return 0;
        }
    }
}