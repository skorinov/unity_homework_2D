using UnityEngine;
using UnityEngine.UI;
using UI;
using Controllers.Player;
using Controllers.Platform;

namespace Managers
{
    public class GameOverManager : MonoBehaviour
    {
        [Header("Settings")] 
        [SerializeField] private float deathDistance = 15f;

        [Header("Game Components")] 
        [SerializeField] private Transform player;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private GameManager gameManager;

        [Header("UI")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Button restartButton;
        [SerializeField] private HeightUI heightUI;

        private bool _isDead;
        
        // Cached components for performance
        private Rigidbody2D _playerRb;
        private PlayerController _playerController;

        private void Start()
        {
            if (!player || !mainCamera) return;

            // Cache player components once
            _playerRb = player.GetComponent<Rigidbody2D>();
            _playerController = player.GetComponent<PlayerController>();

            // Initialize UI
            gameOverPanel.SetActive(false);
            restartButton.onClick.AddListener(RestartGame);
        }

        private void Update()
        {
            // Skip death check if already dead
            if (_isDead) return;

            // Calculate death boundary and check player position
            float deathY = mainCamera.transform.position.y - mainCamera.orthographicSize - deathDistance;
            if (player.position.y < deathY)
            {
                TriggerGameOver();
            }
        }

        public void TriggerGameOver()
        {
            // Prevent multiple triggers
            if (_isDead) return;

            _isDead = true;
            Time.timeScale = 0f;
            
            // Audio feedback
            AudioManager.Instance?.StopBackgroundMusic();
            AudioManager.Instance?.PlayGameOverSound();
            
            gameOverPanel.SetActive(true);
        }

        private void RestartGame()
        {
            // Reset game state
            Time.timeScale = 1f;
            gameOverPanel.SetActive(false);
            _isDead = false;

            // Reset all game components
            ResetPlayer();
            ResetCamera();
            ClearAllPlatforms();
            
            // Reset managers and UI
            gameManager?.ResetGame();
            heightUI?.ResetHeight();
            
            // Restart audio
            AudioManager.Instance?.PlayBackgroundMusic();
        }

        private void ResetPlayer()
        {
            if (!player) return;
            
            // Reset transform
            player.position = new Vector3(0, -2, 0);

            // Reset physics using cached components
            if (_playerRb)
            {
                _playerRb.linearVelocity = Vector2.zero;
                _playerRb.angularVelocity = 0f;
            }

            // Reset player state
            _playerController?.ResetMultipliers();
        }

        private void ResetCamera()
        {
            // Reset camera to initial position
            if (mainCamera)
            {
                mainCamera.transform.position = new Vector3(0, 0, -10);
            }
        }

        private void ClearAllPlatforms()
        {
            PlatformPool.Instance?.ClearAllPlatforms();
        }
    }
}