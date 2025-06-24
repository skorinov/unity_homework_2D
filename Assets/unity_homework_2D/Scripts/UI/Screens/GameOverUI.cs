using Data;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens
{
    public class GameOverUI : BaseUI
    {
        [SerializeField] private TextMeshProUGUI sessionHeightText;
        [SerializeField] private TextMeshProUGUI sessionCoinsText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private string heightFormat = "Height: {0}m";
        [SerializeField] private string coinsFormat = "Coins: {0}";
        
        private void Start()
        {
            restartButton?.onClick.AddListener(() => UIManager.Instance?.RestartGame());
            menuButton?.onClick.AddListener(() => UIManager.Instance?.ReturnToMainMenu());
        }
        
        public override void Show()
        {
            base.Show();
            UpdateSessionDisplay();
        }
        
        private void UpdateSessionDisplay()
        {
            var gameData = DataManager.Instance?.GameData;
            if (gameData == null) return;
            
            if (sessionHeightText)
                sessionHeightText.text = string.Format(heightFormat, Mathf.RoundToInt(gameData.sessionHeight));
                
            if (sessionCoinsText)
                sessionCoinsText.text = string.Format(coinsFormat, gameData.sessionCoins);
        }
    }
}