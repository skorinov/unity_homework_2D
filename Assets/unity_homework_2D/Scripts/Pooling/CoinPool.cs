using Controllers.Collectibles;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Pooling
{
    public class CoinPool : Singleton<CoinPool>
    {
        [SerializeField] private GameObject coinPrefab;
        [SerializeField] private int initialPoolSize = 20;
        
        private ObjectPool<Coin> _coinPool;
        private readonly List<Coin> _activeCoins = new();
        
        public int ActiveCount => _activeCoins.Count;
        
        protected override void OnSingletonAwake()
        {
            if (coinPrefab?.GetComponent<Coin>())
            {
                _coinPool = new ObjectPool<Coin>(coinPrefab, transform, initialPoolSize);
            }
        }
        
        public Coin SpawnCoin(Vector3 position)
        {
            if (_coinPool == null) return null;
            
            var coin = _coinPool.Get();
            if (!coin) return null;
            
            coin.transform.position = position;
            _activeCoins.Add(coin);
            
            return coin;
        }
        
        public void ReturnCoin(Coin coin)
        {
            if (!coin || !_activeCoins.Contains(coin)) return;
            
            _activeCoins.Remove(coin);
            _coinPool?.Return(coin);
        }
        
        public void ReturnCoinsOutsideRange(float minY, float maxY)
        {
            for (int i = _activeCoins.Count - 1; i >= 0; i--)
            {
                if (!_activeCoins[i])
                {
                    _activeCoins.RemoveAt(i);
                    continue;
                }
                
                float coinY = _activeCoins[i].transform.position.y;
                if (coinY < minY || coinY > maxY)
                {
                    ReturnCoin(_activeCoins[i]);
                }
            }
        }
        
        public void ClearAllCoins()
        {
            var coinsToReturn = new List<Coin>(_activeCoins);
            foreach (var coin in coinsToReturn)
            {
                if (coin) ReturnCoin(coin);
            }
            
            _activeCoins.Clear();
        }
    }
}