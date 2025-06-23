using Controllers.Collectibles;
using Pooling;
using System.Collections.Generic;
using UnityEngine;

namespace Controllers.Platform.Actions
{
    [CreateAssetMenu(fileName = "CoinSpawnAction", menuName = "Platform Actions/Coin Spawn Action")]
    public class CoinSpawnAction : PlatformAction
    {
        [SerializeField, Range(0f, 100f)] private float spawnChance = 50f;
        [SerializeField] private int coinCount = 1;
        [SerializeField] private float spawnHeight = 1f;
        [SerializeField] private float coinSpacing = 0.8f;

        private readonly Dictionary<BasePlatform, List<Coin>> _spawnedCoins = new();

        public override void Initialize(BasePlatform platform)
        {
            _spawnedCoins[platform] = new List<Coin>();
        }

        public override void OnPlatformReady(BasePlatform platform)
        {
            if (Random.Range(0f, 100f) <= spawnChance)
                SpawnCoins(platform);
        }

        private void SpawnCoins(BasePlatform platform)
        {
            if (!CoinPool.Instance) return;

            var coins = new List<Coin>();
            Vector3 pos = platform.transform.position;
            pos.y += spawnHeight;
            pos.z = 0f;

            if (coinCount == 1)
            {
                var coin = CoinPool.Instance.SpawnCoin(pos);
                if (coin) coins.Add(coin);
            }
            else
            {
                var collider = platform.GetComponent<BoxCollider2D>();
                float platformWidth = collider ? collider.size.x : 2f;
                float totalWidth = (coinCount - 1) * coinSpacing;
                float startX = pos.x - totalWidth * 0.5f;
                
                float maxOffset = platformWidth * 0.4f;
                startX = Mathf.Clamp(startX, pos.x - maxOffset, pos.x + maxOffset - totalWidth);

                for (int i = 0; i < coinCount; i++)
                {
                    Vector3 coinPos = new Vector3(startX + i * coinSpacing, pos.y, 0f);
                    var coin = CoinPool.Instance.SpawnCoin(coinPos);
                    if (coin) coins.Add(coin);
                }
            }

            if (coins.Count > 0)
                _spawnedCoins[platform] = coins;
        }

        public override void OnReset(BasePlatform platform)
        {
            if (_spawnedCoins.TryGetValue(platform, out var coins))
            {
                foreach (var coin in coins)
                {
                    if (coin) CoinPool.Instance?.ReturnCoin(coin);
                }
                _spawnedCoins.Remove(platform);
            }
        }

        private void OnDestroy() => _spawnedCoins.Clear();
    }
}