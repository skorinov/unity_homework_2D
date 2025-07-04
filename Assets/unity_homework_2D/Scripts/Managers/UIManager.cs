using UnityEngine;
using Utilities;
using UI.Screens;
using UI.Navigation;

namespace Managers
{
    public enum UIState
    {
        None,
        MainMenu,
        InGame,
        GameOver,
        Statistics,
        Settings
    }

    public class UIManager : Singleton<UIManager>
    {
        [SerializeField] private MainMenuUI mainMenuUI;
        [SerializeField] private GameUI gameUI;
        [SerializeField] private GameOverUI gameOverUI;
        [SerializeField] private StatisticsUI statisticsUI;
        [SerializeField] private SettingsUI settingsUI;
        [SerializeField] private InputManager inputManager;

        private UIState _currentState = UIState.None;
        private UIState _previousState = UIState.None;
        private bool _hasInitialized;
        private MenuNavigationController _currentNavigation;

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
                inputManager.OnUINavigateUp += HandleNavigateUp;
                inputManager.OnUINavigateDown += HandleNavigateDown;
                inputManager.OnUIConfirm += HandleConfirm;
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
                case UIState.Settings:
                    SetState(UIState.MainMenu);
                    break;
            }
        }

        private void HandleNavigateUp() => _currentNavigation?.NavigateUp();
        private void HandleNavigateDown() => _currentNavigation?.NavigateDown();
        private void HandleConfirm() => _currentNavigation?.Confirm();

        public void SetState(UIState newState)
        {
            if (_currentState == newState) return;

            _previousState = _currentState;
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
                case UIState.Settings:
                    ShowSettingsInternal();
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
                _currentNavigation = mainMenuUI.GetNavigation();
            }

            AudioManager.Instance?.PlayMenuMusic();
            inputManager?.EnableUIInput();
        }

        private void ShowInGameInternal()
        {
            HideAllScreens();
            _currentNavigation = null;

            if (gameUI)
            {
                gameUI.gameObject.SetActive(true);
                gameUI.Show();
            }

            AudioManager.Instance?.PlayBackgroundMusic();
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
                _currentNavigation = gameOverUI.GetNavigation();
            }

            AudioManager.Instance?.PlayMenuMusic();
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
                _currentNavigation = statisticsUI.GetNavigation();
            }

            AudioManager.Instance?.PlayMenuMusic();
            inputManager?.EnableUIInput();
        }

        private void ShowSettingsInternal()
        {
            HideAllScreens();

            if (settingsUI)
            {
                settingsUI.gameObject.SetActive(true);
                settingsUI.Show();
                ClearButtonStates(settingsUI.gameObject);
                _currentNavigation = settingsUI.GetNavigation();
            }

            AudioManager.Instance?.PlayMenuMusic();
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

            if (settingsUI)
            {
                settingsUI.Hide();
                settingsUI.gameObject.SetActive(false);
            }
        }

        public void StartGame()
        {
            if (GameManager.Instance?.IsCameraTransitioning == true) return;
            SetState(UIState.InGame);
            GameManager.Instance?.StartGame();
        }

        public void ResumeGame()
        {
            if (GameManager.Instance?.IsCameraTransitioning == true) return;
            SetState(UIState.InGame);
            GameManager.Instance?.ResumeGame();
        }

        public void RestartGame()
        {
            if (GameManager.Instance?.IsCameraTransitioning == true) return;
            SetState(UIState.InGame);
            GameManager.Instance?.RestartGame();
        }

        public void ShowGameOver()
        {
            Data.DataManager.Instance?.SaveData();
            SetState(UIState.GameOver);
        }

        public void ShowStatistics() => SetState(UIState.Statistics);
        public void ShowSettings() => SetState(UIState.Settings);

        public void ReturnToMainMenu()
        {
            if (GameManager.Instance?.IsCameraTransitioning == true) return;
            SetState(UIState.MainMenu);
            
            // Only pause game if coming from in-game
            if (_previousState == UIState.InGame)
                GameManager.Instance?.PauseGame();
        }

        public void QuitGame()
        {
            Data.DataManager.Instance?.SaveData();
            Application.Quit();
        }

        protected override void OnSingletonDestroy()
        {
            if (inputManager)
            {
                inputManager.OnUICancel -= HandleEscapeInput;
                inputManager.OnUINavigateUp -= HandleNavigateUp;
                inputManager.OnUINavigateDown -= HandleNavigateDown;
                inputManager.OnUIConfirm -= HandleConfirm;
            }
        }
    }
}