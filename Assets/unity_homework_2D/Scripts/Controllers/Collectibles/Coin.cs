using Managers;
using Pooling;
using UnityEngine;

namespace Controllers.Collectibles
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class Coin : MonoBehaviour, IPoolable
    {
        [Header("Settings")]
        [SerializeField] private int coinValue = 1;
        
        private CircleCollider2D _collider;
        private bool _isCollected;
        
        private const string PLAYER_TAG = "Player";
        
        private void Awake()
        {
            _collider = GetComponent<CircleCollider2D>();
            _collider.isTrigger = true;
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(PLAYER_TAG) && !_isCollected)
            {
                _isCollected = true;
                
                // Add coins to game (implement in GameManager)
                // GameManager.Instance?.AddCoins(coinValue);
                
                AudioManager.Instance?.PlayCoinSound();
                CoinPool.Instance?.ReturnCoin(this);
            }
        }
        
        public void OnGetFromPool()
        {
            _isCollected = false;
            _collider.enabled = true;
            gameObject.SetActive(true);
        }
        
        public void OnReturnToPool()
        {
            gameObject.SetActive(false);
        }
        
        public void OnCreatedInPool() { }
        public bool CanReturnToPool() => _isCollected;
    }
}