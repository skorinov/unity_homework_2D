using Constants;
using Controllers.Player;
using Data;
using Pooling;
using UnityEngine;
using Utilities;

namespace Managers
{
    public enum GameState
    {
        Menu,
        Playing,
        Paused
    }

    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private Transform player;
        [SerializeField] private PlatformGenerator platformGenerator;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float cameraTransitionSpeed = 8f;
        [SerializeField] private float deathDistance = 15f;

        public System.Action<float> OnHeightChanged;

        private float _highestPlayerY;
        private float _lastGameCameraY;
        private GameState _gameState = GameState.Menu;
        private bool _isDead;
        private bool _isCameraTransitioning;
        private bool _hasGameEverStarted;

        private CameraFollow _cameraFollow;
        private ScreenWrappingSystem _screenWrapping;

        public bool IsGameActive => _gameState == GameState.Playing;
        public bool IsGameInProgress => _gameState != GameState.Menu;
        public bool HasGameEverStarted => _hasGameEverStarted;
        public bool IsCameraTransitioning => _isCameraTransitioning;

        protected override void OnSingletonAwake()
        {
            InitializeComponents();
            SetupMenuCamera();
        }

        private void Update()
        {
            if (_gameState == GameState.Playing && !_isDead && player)
            {
                UpdatePlayerHeight();
                CheckDeathConditions();
                platformGenerator?.UpdateGeneration(player.position.y);
            }
        }

        private void InitializeComponents()
        {
            if (!mainCamera) mainCamera = Camera.main;

            _cameraFollow = mainCamera.GetComponent<CameraFollow>();
            if (!_cameraFollow) _cameraFollow = mainCamera.gameObject.AddComponent<CameraFollow>();

            var playerObj = player ? player.gameObject : GameObject.FindWithTag(GameConstants.PLAYER_TAG);
            if (playerObj)
            {
                _screenWrapping = playerObj.GetComponent<ScreenWrappingSystem>();
                if (!_screenWrapping) _screenWrapping = playerObj.AddComponent<ScreenWrappingSystem>();
            }
        }

        private void SetupMenuCamera()
        {
            if (mainCamera)
            {
                Vector3 pos = mainCamera.transform.position;
                pos.x = GameConstants.MENU_AREA_X;
                pos.y = 0f;
                mainCamera.transform.position = pos;
                mainCamera.orthographicSize = GameConstants.CAMERA_SIZE;
            }

            DisableGameComponents();
        }

        public void StartGame()
        {
            if (!player || _isCameraTransitioning) return;
            
            // Clear only for NEW game
            ClearAllGameObjects();
            // Always reset game state for new game
            _hasGameEverStarted = true;
            StartCoroutine(TransitionToGame(true)); // Force new game
        }

        public void PauseGame()
        {
            if (_gameState != GameState.Playing || _isCameraTransitioning) return;
            _gameState = GameState.Paused;
            StartCoroutine(TransitionToMenu());
        }

        public void ResumeGame()
        {
            if (_gameState != GameState.Paused || _isCameraTransitioning) return;
            StartCoroutine(TransitionToGame(false)); // Resume existing game
        }

        public void RestartGame()
        {
            if (!player || _isCameraTransitioning) return;
            
            ClearAllGameObjects();

            _isDead = false;
            ResetGameSystems();
            _highestPlayerY = player.position.y;
            OnHeightChanged?.Invoke(_highestPlayerY);

            StartCoroutine(TransitionToGame(true)); // Force new game
        }

        private System.Collections.IEnumerator TransitionToGame(bool isNewGame = false)
        {
            _isCameraTransitioning = true;

            DisableGameComponents();
            Time.timeScale = 0f;

            // Setup game state
            if (_gameState == GameState.Menu || isNewGame)
            {
                _gameState = GameState.Playing;
                _isDead = false;
                _hasGameEverStarted = true;
                DataManager.Instance?.StartNewSession();
                
                yield return StartCoroutine(MoveCameraToPosition(GameConstants.GAME_AREA_X, GameConstants.CAMERA_SIZE, 0f));
                
                if (player) player.position = Vector3.zero;
                _highestPlayerY = 0f;
                InitializeGame();
            }
            else if (_gameState == GameState.Paused)
            {
                _gameState = GameState.Playing;
                
                // Resume from saved camera position
                yield return StartCoroutine(MoveCameraToPosition(GameConstants.GAME_AREA_X, GameConstants.CAMERA_SIZE, _lastGameCameraY));
            }

            EnableGameComponents();
            Time.timeScale = 1f;
            _isCameraTransitioning = false;
        }

        private System.Collections.IEnumerator TransitionToMenu()
        {
            _isCameraTransitioning = true;

            // Save current camera Y position only if we're pausing (not dying)
            if (_gameState == GameState.Paused && mainCamera)
                _lastGameCameraY = mainCamera.transform.position.y;

            DisableGameComponents();
            Time.timeScale = 0f;

            yield return StartCoroutine(MoveCameraToPosition(GameConstants.MENU_AREA_X, GameConstants.CAMERA_SIZE, 0));

            _isCameraTransitioning = false;
        }

        private System.Collections.IEnumerator MoveCameraToPosition(float targetX, float targetSize, float targetY)
        {
            if (!mainCamera) yield break;

            Vector3 startPos = mainCamera.transform.position;
            Vector3 targetPos = new Vector3(targetX, targetY, startPos.z);
            float startSize = mainCamera.orthographicSize;
            float elapsed = 0f;
            float duration = GameConstants.CAMERA_TRANSITION_BASE_DURATION  / cameraTransitionSpeed;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

                mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
                mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, t);

                yield return null;
            }

            mainCamera.transform.position = targetPos;
            mainCamera.orthographicSize = targetSize;
        }

        private void EnableGameComponents()
        {
            if (_cameraFollow)
            {
                _cameraFollow.enabled = true;
                _cameraFollow.SetTarget(player);
            }

            if (_screenWrapping) _screenWrapping.enabled = true;
        }

        private void DisableGameComponents()
        {
            if (_cameraFollow) _cameraFollow.enabled = false;
            if (_screenWrapping) _screenWrapping.enabled = false;
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
            _gameState = GameState.Menu;
            _hasGameEverStarted = false; // Reset so buttons show correctly

            DisableGameComponents();
            Time.timeScale = 0f;
            
            AudioManager.Instance?.PlayGameOverSound();
            DataManager.Instance?.GameData.EndSession();
            UIManager.Instance?.ShowGameOver();
        }

        private void ResetGameSystems()
        {
            ResetPlayer();
            ResetCameraPosition();
        }

        private void ResetPlayer()
        {
            if (!player) return;

            player.position = Vector3.zero;

            var playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb)
            {
                playerRb.linearVelocity = Vector2.zero;
                playerRb.angularVelocity = 0f;
            }

            player.GetComponent<PlayerController>()?.ResetMultipliers();
        }

        private void ResetCameraPosition()
        {
            if (mainCamera)
            {
                Vector3 pos = mainCamera.transform.position;
                pos.y = 0f;
                mainCamera.transform.position = pos;
            }
        }

        private void ClearAllGameObjects()
        {
            PlatformPool.Instance?.ClearAllPlatforms();
            CoinPool.Instance?.ClearAllCoins();
            EnemyPool.Instance?.ClearAllEnemies();
        }
    }
}