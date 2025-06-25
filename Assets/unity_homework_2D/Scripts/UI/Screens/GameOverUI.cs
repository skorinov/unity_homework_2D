using Data;
using Managers;
using TMPro;
using UI.Navigation;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens
{
    public class GameOverUI : BaseUI
    {
        [SerializeField] private TextMeshProUGUI gameOverText;
        [SerializeField] private TextMeshProUGUI sessionHeightText;
        [SerializeField] private TextMeshProUGUI sessionCoinsText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private string heightFormat = "Height: {0}m";
        [SerializeField] private string coinsFormat = "Coins: {0}";
        
        private MenuNavigationController _navigation;

        protected override void Awake()
        {
            base.Awake();
            _navigation = GetComponent<MenuNavigationController>();
        }
        
        private void Start()
        {
            restartButton?.onClick.AddListener(() => UIManager.Instance?.RestartGame());
            menuButton?.onClick.AddListener(() => UIManager.Instance?.ReturnToMainMenu());
        }
        
        public override void Show()
        {
            base.Show();
            UpdateSessionDisplay();
            _navigation?.Initialize();
        }

        public override void Hide()
        {
            base.Hide();
            _navigation?.SetActive(false);
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

        public MenuNavigationController GetNavigation() => _navigation;
    }
}