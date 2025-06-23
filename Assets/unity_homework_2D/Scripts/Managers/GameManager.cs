using Controllers.Player;
using Data;
using Pooling;
using UnityEngine;
using Utilities;

namespace Managers
{
    public enum GameState
    {
        Stopped,
        Playing,
        Paused
    }

    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private Transform player;
        [SerializeField] private PlatformGenerator platformGenerator;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float deathDistance = 15f;
        
        public System.Action<float> OnHeightChanged;
        
        private float _highestPlayerY;
        private GameState _gameState = GameState.Stopped;
        private bool _isDead;
        
        public bool IsGameActive => _gameState == GameState.Playing;
        public bool IsGameInProgress => _gameState != GameState.Stopped;
        
        private void Update()
        {
            if (!IsGameActive || !player) return;
            
            UpdatePlayerHeight();
            CheckDeathConditions();
            platformGenerator?.UpdateGeneration(player.position.y);
        }
        
        public void StartGame()
        {
            if (!player) return;
            
            _gameState = GameState.Playing;
            _isDead = false;
            DataManager.Instance?.StartNewSession();
            
            InitializeGame();
        }
        
        public void PauseGame() 
        {
            _gameState = GameState.Paused;
            Time.timeScale = 0f;
        }
        
        public void ResumeGame() 
        {
            _gameState = GameState.Playing;
            Time.timeScale = 1f;
        }
        
        public void StopGame() 
        {
            _gameState = GameState.Stopped;
            Time.timeScale = 0f;
        }
        
        public void RestartGame()
        {
            if (!player) return;
            
            _isDead = false;
            ResetAllSystems();
            
            _highestPlayerY = player.position.y;
            OnHeightChanged?.Invoke(_highestPlayerY);
            platformGenerator?.ResetGeneration(player.position);
            
            _gameState = GameState.Playing;
            Time.timeScale = 1f;
            AudioManager.Instance?.PlayBackgroundMusic();
        }
        
        private void InitializeGame()
        {
            if (!player) return;
            
            _highestPlayerY = player.position.y;
            OnHeightChanged?.Invoke(_highestPlayerY);
            platformGenerator?.Initialize(player.position);
        }
        
        private void UpdatePlayerHeight()
        {
            float currentHeight = player.position.y;
            if (currentHeight > _highestPlayerY)
            {
                _highestPlayerY = currentHeight;
                OnHeightChanged?.Invoke(_highestPlayerY);
                DataManager.Instance?.UpdateHeight(_highestPlayerY);
            }
        }
        
        private void CheckDeathConditions()
        {
            if (_isDead || !mainCamera) return;

            float deathY = mainCamera.transform.position.y - mainCamera.orthographicSize - deathDistance;
            if (player.position.y < deathY)
            {
                TriggerGameOver();
            }
        }
        
        public void TriggerGameOver()
        {
            if (_isDead) return;

            _isDead = true;
            _gameState = GameState.Stopped;
            Time.timeScale = 0f;
            
            AudioManager.Instance?.StopBackgroundMusic();
            AudioManager.Instance?.PlayGameOverSound();
            UIManager.Instance?.ShowGameOver();
        }
        
        private void ResetAllSystems()
        {
            ResetPlayer();
            ResetCamera();
            ClearPools();
        }
        
        private void ResetPlayer()
        {
            if (!player) return;

            var playerRb = player.GetComponent<Rigidbody2D>();
            var playerController = player.GetComponent<PlayerController>();
            
            if (playerRb)
            {
                playerRb.linearVelocity = Vector2.zero;
                playerRb.angularVelocity = 0f;
            }
            
            playerController?.ResetMultipliers();
        }

        private void ResetCamera()
        {
            if (mainCamera)
                mainCamera.transform.position = new Vector3(0, 0, -10);
        }

        private void ClearPools()
        {
            PlatformPool.Instance?.ClearAllPlatforms();
            CoinPool.Instance?.ClearAllCoins();
        }
        
        public float GetHighestPlayerY() => _highestPlayerY;
        public Transform GetPlayer() => player;
    }
}