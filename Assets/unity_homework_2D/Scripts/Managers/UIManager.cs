using UnityEngine;
using Utilities;
using UI.Screens;

namespace Managers
{
    public enum UIState
    {
        None,
        MainMenu,
        InGame,
        GameOver,
        Statistics
    }

    public class UIManager : Singleton<UIManager>
    {
        [SerializeField] private MainMenuUI mainMenuUI;
        [SerializeField] private GameUI gameUI;
        [SerializeField] private GameOverUI gameOverUI;
        [SerializeField] private StatisticsUI statisticsUI;
        [SerializeField] private InputManager inputManager;

        private UIState _currentState = UIState.None;
        private bool _hasInitialized;

        public bool IsGameInProgress => _currentState == UIState.InGame;

        protected override void OnSingletonAwake()
        {
            HideAllScreens();
            SubscribeToInput();
        }

        private void Start()
        {
            if (!_hasInitialized)
            {
                _hasInitialized = true;
                SetState(UIState.MainMenu);
            }
        }

        private void SubscribeToInput()
        {
            if (inputManager)
            {
                inputManager.OnUICancel += HandleEscapeInput;
            }
        }

        private void HandleEscapeInput()
        {
            switch (_currentState)
            {
                case UIState.InGame:
                    ReturnToMainMenu();
                    break;
                case UIState.Statistics:
                    SetState(UIState.MainMenu);
                    break;
            }
        }

        public void SetState(UIState newState)
        {
            if (_currentState == newState) return;

            _currentState = newState;

            switch (_currentState)
            {
                case UIState.MainMenu:
                    ShowMainMenuInternal();
                    break;
                case UIState.InGame:
                    ShowInGameInternal();
                    break;
                case UIState.GameOver:
                    ShowGameOverInternal();
                    break;
                case UIState.Statistics:
                    ShowStatisticsInternal();
                    break;
            }
        }

        private void ShowMainMenuInternal()
        {
            HideAllScreens();

            if (mainMenuUI)
            {
                mainMenuUI.gameObject.SetActive(true);
                mainMenuUI.Show();
                ClearButtonStates(mainMenuUI.gameObject);
            }

            inputManager?.EnableUIInput();
        }

        private void ShowInGameInternal()
        {
            HideAllScreens();

            if (gameUI)
            {
                gameUI.gameObject.SetActive(true);
                gameUI.Show();
            }

            inputManager?.EnableGameInput();
        }

        private void ShowGameOverInternal()
        {
            HideAllScreens();

            if (gameOverUI)
            {
                gameOverUI.gameObject.SetActive(true);
                gameOverUI.Show();
                ClearButtonStates(gameOverUI.gameObject);
            }

            inputManager?.EnableUIInput();
        }

        private void ShowStatisticsInternal()
        {
            HideAllScreens();

            if (statisticsUI)
            {
                statisticsUI.gameObject.SetActive(true);
                statisticsUI.Show();
                ClearButtonStates(statisticsUI.gameObject);
            }

            inputManager?.EnableUIInput();
        }

        private void ClearButtonStates(GameObject parent)
        {
            var buttons = parent.GetComponentsInChildren<UnityEngine.UI.Button>(true);
            foreach (var button in buttons)
            {
                button.OnDeselect(null);

                if (button.transition == UnityEngine.UI.Selectable.Transition.ColorTint)
                {
                    button.targetGraphic.color = button.colors.normalColor;
                }
            }
        }

        private void HideAllScreens()
        {
            if (mainMenuUI)
            {
                mainMenuUI.Hide();
                mainMenuUI.gameObject.SetActive(false);
            }

            if (gameUI)
            {
                gameUI.Hide();
                gameUI.gameObject.SetActive(false);
            }

            if (gameOverUI)
            {
                gameOverUI.Hide();
                gameOverUI.gameObject.SetActive(false);
            }

            if (statisticsUI)
            {
                statisticsUI.Hide();
                statisticsUI.gameObject.SetActive(false);
            }
        }

        public void StartGame()
        {
            if (GameManager.Instance?.IsCameraTransitioning == true) return;
            SetState(UIState.InGame);
            GameManager.Instance?.StartGame(); // Always starts new game
        }

        public void ResumeGame()
        {
            if (GameManager.Instance?.IsCameraTransitioning == true) return;
            SetState(UIState.InGame);
            GameManager.Instance?.ResumeGame(); // Resumes paused game
        }

        public void RestartGame()
        {
            if (GameManager.Instance?.IsCameraTransitioning == true) return;
            SetState(UIState.InGame);
            GameManager.Instance?.RestartGame(); // Restarts from game over
        }

        public void ShowGameOver()
        {
            Data.DataManager.Instance?.SaveData();
            SetState(UIState.GameOver);
        }

        public void ShowStatistics() => SetState(UIState.Statistics);

        public void ReturnToMainMenu()
        {
            if (GameManager.Instance?.IsCameraTransitioning == true) return;
            SetState(UIState.MainMenu);
            GameManager.Instance?.PauseGame(); // Pauses current game
        }

        public void QuitGame()
        {
            Data.DataManager.Instance?.SaveData();
            
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        protected override void OnSingletonDestroy()
        {
            if (inputManager)
            {
                inputManager.OnUICancel -= HandleEscapeInput;
            }
        }
    }
}