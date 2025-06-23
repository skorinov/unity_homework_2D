using Managers;
using TMPro;
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
        [SerializeField] private Button quitButton;
        
        private void Start()
        {
            resumeButton?.onClick.AddListener(() => UIManager.Instance?.ResumeGame());
            startButton?.onClick.AddListener(() => UIManager.Instance?.StartGame());
            statisticsButton?.onClick.AddListener(() => UIManager.Instance?.ShowStatistics());
            quitButton?.onClick.AddListener(() => UIManager.Instance?.QuitGame());
        }
        
        public override void Show()
        {
            base.Show();
            UpdateButtonsVisibility();
        }
        
        private void UpdateButtonsVisibility()
        {
            bool gameInProgress = UIManager.Instance?.IsGameInProgress == true;
            
            if (resumeButton)
                resumeButton.gameObject.SetActive(gameInProgress);
                
            if (startButton)
            {
                var startButtonText = startButton.GetComponentInChildren<TextMeshProUGUI>();
                if (startButtonText)
                    startButtonText.text = gameInProgress ? "New Game" : "Start Game";
            }
        }
    }
}