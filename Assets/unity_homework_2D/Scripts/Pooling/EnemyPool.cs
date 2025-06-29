using Controllers.Enemies;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Pooling
{
    [System.Serializable]
    public class EnemyPrefabData
    {
        public GameObject prefab;
        [Range(0f, 100f)] public float spawnChance = 25f;
        public int poolSize = 5;
        
        [System.NonSerialized]
        public ObjectPool<BaseEnemy> pool;
    }

    public class EnemyPool : Singleton<EnemyPool>
    {
        [SerializeField] private EnemyPrefabData[] enemyPrefabs;

        private readonly List<BaseEnemy> _activeEnemies = new();
        private readonly Dictionary<BaseEnemy, int> _enemyToPoolIndex = new();
        private float _totalSpawnWeight;

        public int ActiveCount => _activeEnemies.Count;

        protected override void OnSingletonAwake()
        {
            InitializePools();
            CalculateTotalWeight();
        }

        public BaseEnemy GetEnemy()
        {
            var selectedIndex = SelectRandomPrefabIndex();
            if (selectedIndex == -1 || enemyPrefabs[selectedIndex].pool == null) return null;

            var enemy = enemyPrefabs[selectedIndex].pool.Get();
            if (!enemy) return null;

            _activeEnemies.Add(enemy);
            _enemyToPoolIndex[enemy] = selectedIndex;

            return enemy;
        }

        public void ReturnEnemy(BaseEnemy enemy)
        {
            if (!enemy || !_enemyToPoolIndex.TryGetValue(enemy, out int poolIndex)) return;

            _activeEnemies.Remove(enemy);
            _enemyToPoolIndex.Remove(enemy);

            if (poolIndex >= 0 && poolIndex < enemyPrefabs.Length && enemyPrefabs[poolIndex].pool != null)
            {
                enemyPrefabs[poolIndex].pool.Return(enemy);
            }
        }

        public void ClearAllEnemies()
        {
            var enemiesToReturn = new List<BaseEnemy>(_activeEnemies);
            foreach (var enemy in enemiesToReturn)
            {
                if (enemy) ReturnEnemy(enemy);
            }
            
            _activeEnemies.Clear();
            _enemyToPoolIndex.Clear();
        }

        private void InitializePools()
        {
            for (int i = 0; i < enemyPrefabs.Length; i++)
            {
                var prefabData = enemyPrefabs[i];
                if (!prefabData.prefab?.GetComponent<BaseEnemy>()) continue;

                prefabData.pool = new ObjectPool<BaseEnemy>(
                    prefabData.prefab, 
                    transform, 
                    prefabData.poolSize
                );
            }
        }

        private void CalculateTotalWeight()
        {
            _totalSpawnWeight = 0f;
            foreach (var prefabData in enemyPrefabs)
            {
                if (prefabData.pool != null)
                    _totalSpawnWeight += prefabData.spawnChance;
            }
        }

        private int SelectRandomPrefabIndex()
        {
            if (_totalSpawnWeight <= 0f) return 0;

            float random = Random.Range(0f, _totalSpawnWeight);
            
            for (int i = 0; i < enemyPrefabs.Length; i++)
            {
                var prefabData = enemyPrefabs[i];
                if (prefabData.pool == null) continue;
                
                if (random < prefabData.spawnChance)
                    return i;
                    
                random -= prefabData.spawnChance;
            }

            return 0;
        }
    }
}