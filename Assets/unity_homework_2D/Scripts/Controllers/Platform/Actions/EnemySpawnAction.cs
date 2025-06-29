using Controllers.Enemies;
using Pooling;
using System.Collections.Generic;
using UnityEngine;

namespace Controllers.Platform.Actions
{
    [CreateAssetMenu(fileName = "EnemySpawnAction", menuName = "Platform Actions/Enemy Spawn Action")]
    public class EnemySpawnAction : PlatformAction
    {
        [SerializeField, Range(0f, 100f)] private float spawnChance = 30f;
        [SerializeField] private float spawnHeight = 0.3f;
        
        [System.NonSerialized]
        private Dictionary<BasePlatform, BaseEnemy> _spawnedEnemies;
        
        public override void SetChance(float chance) => spawnChance = Mathf.Clamp(chance, 0f, 100f);
        public override bool HasChance() => true;
        
        public override void OnPlatformReady(BasePlatform platform, Vector3 worldPosition)
        {
            if (Random.Range(0f, 100f) <= spawnChance)
                SpawnEnemy(platform, worldPosition);
        }
        
        public override void OnReset(BasePlatform platform)
        {
            if (_spawnedEnemies?.TryGetValue(platform, out var enemy) == true && 
                enemy && enemy.gameObject.activeInHierarchy)
            {
                // Detach from platform before returning to pool
                enemy.transform.SetParent(null);
                EnemyPool.Instance?.ReturnEnemy(enemy);
            }
            _spawnedEnemies?.Remove(platform);
        }
        
        public override void Initialize(BasePlatform platform)
        {
            // Initialize dictionary if needed
            if (_spawnedEnemies == null)
                _spawnedEnemies = new Dictionary<BasePlatform, BaseEnemy>();
                
            // Clear any leftover data for this platform
            _spawnedEnemies.Remove(platform);
        }
        
        private void SpawnEnemy(BasePlatform platform, Vector3 platformPosition)
        {
            if (!EnemyPool.Instance) return;
            if (_spawnedEnemies == null) _spawnedEnemies = new Dictionary<BasePlatform, BaseEnemy>();
            
            var enemy = EnemyPool.Instance.GetEnemy();
            if (enemy)
            {
                // Position enemy on platform
                Vector3 spawnPosition = platformPosition + Vector3.up * spawnHeight;
                
                // Setup enemy to patrol this platform
                enemy.SetupOnPlatform(platform.transform, spawnPosition);
                
                // Make enemy a child of platform so it follows platform movement
                enemy.transform.SetParent(platform.transform);
                
                _spawnedEnemies[platform] = enemy;
            }
        }
        
        private void OnDestroy() => ClearAllEnemies();
        private void OnDisable() => ClearAllEnemies();
        
        private void ClearAllEnemies()
        {
            if (_spawnedEnemies == null) return;
            
            // Also clear when ScriptableObject is disabled
            foreach (var enemy in _spawnedEnemies.Values)
            {
                if (enemy && enemy.gameObject.activeInHierarchy)
                {
                    enemy.transform.SetParent(null);
                    EnemyPool.Instance?.ReturnEnemy(enemy);
                }
            }
            _spawnedEnemies.Clear();
        }
    }
}