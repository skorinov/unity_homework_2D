using UnityEngine;
using UnityEngine.UI;
using UI;
using Controllers.Player;
using Pooling;

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
        private Rigidbody2D _playerRb;
        private PlayerController _playerController;

        private void Start()
        {
            if (!player || !mainCamera) return;

            _playerRb = player.GetComponent<Rigidbody2D>();
            _playerController = player.GetComponent<PlayerController>();

            gameOverPanel.SetActive(false);
            restartButton.onClick.AddListener(RestartGame);
        }

        private void Update()
        {
            if (_isDead) return;

            float deathY = mainCamera.transform.position.y - mainCamera.orthographicSize - deathDistance;
            if (player.position.y < deathY)
                TriggerGameOver();
        }

        public void TriggerGameOver()
        {
            if (_isDead) return;

            _isDead = true;
            Time.timeScale = 0f;

            AudioManager.Instance?.StopBackgroundMusic();
            AudioManager.Instance?.PlayGameOverSound();

            gameOverPanel.SetActive(true);
        }

        private void RestartGame()
        {
            Time.timeScale = 1f;
            gameOverPanel.SetActive(false);
            _isDead = false;

            ResetPlayer();
            ResetCamera();
            ClearAllPlatforms();

            gameManager?.ResetGame();
            heightUI?.ResetHeight();

            AudioManager.Instance?.PlayBackgroundMusic();
        }

        private void ResetPlayer()
        {
            if (!player) return;

            player.position = new Vector3(0, -2, 0);

            if (_playerRb)
            {
                _playerRb.linearVelocity = Vector2.zero;
                _playerRb.angularVelocity = 0f;
            }

            _playerController?.ResetMultipliers();
        }

        private void ResetCamera()
        {
            if (mainCamera)
                mainCamera.transform.position = new Vector3(0, 0, -10);
        }

        private void ClearAllPlatforms()
        {
            PlatformPool.Instance?.ClearAllPlatforms();
        }
    }
}