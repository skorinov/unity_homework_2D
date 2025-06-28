using Managers;
using TMPro;
using UI.Navigation;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens
{
    public class MainMenuUI : BaseUI
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Image logoImage;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button startButton;
        [SerializeField] private Button statisticsButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        private MenuNavigationController _navigation;

        protected override void Awake()
        {
            base.Awake();
            _navigation = GetComponent<MenuNavigationController>();
        }

        private void Start()
        {
            resumeButton?.onClick.AddListener(OnResumeClicked);
            startButton?.onClick.AddListener(OnStartClicked);
            statisticsButton?.onClick.AddListener(OnStatisticsClicked);
            settingsButton?.onClick.AddListener(OnSettingsClicked);
            quitButton?.onClick.AddListener(OnQuitClicked);
        }

        public override void Show()
        {
            base.Show();
            UpdateButtonsVisibility();
            _navigation?.Initialize();
        }

        public override void Hide()
        {
            base.Hide();
            _navigation?.SetActive(false);
        }

        private void OnResumeClicked()
        {
            if (CanInteract())
                UIManager.Instance?.ResumeGame();
        }

        private void OnStartClicked()
        {
            if (CanInteract())
                UIManager.Instance?.StartGame();
        }

        private void OnStatisticsClicked()
        {
            if (CanInteract())
                UIManager.Instance?.ShowStatistics();
        }

        private void OnSettingsClicked()
        {
            if (CanInteract())
                UIManager.Instance?.ShowSettings();
        }

        private void OnQuitClicked()
        {
            if (CanInteract())
                UIManager.Instance?.QuitGame();
        }

        private bool CanInteract()
        {
            return GameManager.Instance?.IsCameraTransitioning != true;
        }

        private void UpdateButtonsVisibility()
        {
            bool gameEverStarted = GameManager.Instance?.HasGameEverStarted == true;

            if (resumeButton)
                resumeButton.gameObject.SetActive(gameEverStarted);

            if (startButton)
            {
                var startButtonText = startButton.GetComponentInChildren<TextMeshProUGUI>();
                if (startButtonText)
                    startButtonText.text = gameEverStarted ? "New Game" : "Start Game";
            }
        }

        public MenuNavigationController GetNavigation() => _navigation;
    }
}