using Constants;
using Data;
using Managers;
using Pooling;
using UnityEngine;

namespace Controllers.Collectibles
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class Coin : MonoBehaviour, IPoolable
    {
        private CircleCollider2D _collider;
        private bool _isCollected;
        
        private void Awake()
        {
            _collider = GetComponent<CircleCollider2D>();
            _collider.isTrigger = true;
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(GameConstants.PLAYER_TAG) && !_isCollected)
            {
                _isCollected = true;
                
                DataManager.Instance?.AddCoin();
                AudioManager.Instance?.PlayCoinSound();
                CoinPool.Instance?.ReturnCoin(this);
            }
        }
        
        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }
        
        public void OnGetFromPool()
        {
            _isCollected = false;
            _collider.enabled = true;
            gameObject.SetActive(true);
        }
        
        public void OnReturnToPool() => gameObject.SetActive(false);
        public void OnCreatedInPool() { }
        public bool CanReturnToPool() => true;
    }
}