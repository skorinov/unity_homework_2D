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
        
        private UIState _currentState = UIState.None;  // Initialize to None
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
            
            GameManager.Instance?.StopGame();
            
            if (mainMenuUI)
            {
                mainMenuUI.gameObject.SetActive(true);
                mainMenuUI.Show();
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
            
            GameManager.Instance?.StartGame();
            inputManager?.EnableGameInput();
        }
        
        private void ShowGameOverInternal()
        {
            HideAllScreens();
    
            if (gameOverUI)
            {
                gameOverUI.gameObject.SetActive(true);
                gameOverUI.Show();
            }
            
            GameManager.Instance?.StopGame();
            inputManager?.EnableUIInput();
        }
        
        private void ShowStatisticsInternal()
        {
            HideAllScreens();
            
            if (statisticsUI)
            {
                statisticsUI.gameObject.SetActive(true);
                statisticsUI.Show();
            }
            inputManager?.EnableUIInput();
        }
        
        private void HideAllScreens()
        {
            mainMenuUI?.Hide();
            gameUI?.Hide();
            gameOverUI?.Hide();
            statisticsUI?.Hide();
        }
        
        public void StartGame() => SetState(UIState.InGame);
        public void ResumeGame() => SetState(UIState.InGame);
        public void RestartGame() => SetState(UIState.InGame);
        public void ShowGameOver()
        {
            Data.DataManager.Instance?.SaveData();
            SetState(UIState.GameOver);
        }
        public void ShowStatistics() => SetState(UIState.Statistics);
        public void ReturnToMainMenu() => SetState(UIState.MainMenu);
        
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
            }
        }
    }
}