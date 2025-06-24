using Controllers.Collectibles;
using Pooling;
using System.Collections.Generic;
using UnityEngine;

namespace Controllers.Platform.Actions
{
    [CreateAssetMenu(fileName = "CoinSpawnAction", menuName = "Platform Actions/Coin Spawn Action")]
    public class CoinSpawnAction : PlatformAction
    {
        [SerializeField, Range(0f, 100f)] private float spawnChance = 30f;
        [SerializeField] private float spawnHeight = 1f;

        [System.NonSerialized]
        private Dictionary<BasePlatform, Coin> _spawnedCoins;

        public override void OnPlatformReady(BasePlatform platform, Vector3 worldPosition)
        {
            if (Random.Range(0f, 100f) <= spawnChance)
                SpawnCoin(platform, worldPosition);
        }

        public override void OnReset(BasePlatform platform)
        {
            if (_spawnedCoins.TryGetValue(platform, out var coin) && coin && coin.gameObject.activeInHierarchy)
            {
                CoinPool.Instance?.ReturnCoin(coin);
            }
            _spawnedCoins.Remove(platform);
        }

        public override void Initialize(BasePlatform platform)
        {
            // Initialize dictionary if needed
            if (_spawnedCoins == null)
                _spawnedCoins = new Dictionary<BasePlatform, Coin>();
                
            // Clear any leftover data for this platform
            _spawnedCoins.Remove(platform);
        }

        private void SpawnCoin(BasePlatform platform, Vector3 platformPosition)
        {
            if (!CoinPool.Instance) return;
            if (_spawnedCoins == null) _spawnedCoins = new Dictionary<BasePlatform, Coin>();

            var coin = CoinPool.Instance.GetCoin();
            if (coin)
            {
                Vector3 coinPosition = platformPosition;
                coinPosition.y += spawnHeight;
                coinPosition.z = 0f;
                coin.SetPosition(coinPosition);
                _spawnedCoins[platform] = coin;
            }
        }

        private void OnDestroy() 
        {
            if (_spawnedCoins == null) return;
            
            // Return all coins managed by this action
            foreach (var coin in _spawnedCoins.Values)
            {
                if (coin && coin.gameObject.activeInHierarchy)
                    CoinPool.Instance?.ReturnCoin(coin);
            }
            _spawnedCoins.Clear();
        }

        private void OnDisable()
        {
            // Also clear when ScriptableObject is disabled
            OnDestroy();
        }
    }
}