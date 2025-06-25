using Data;
using Managers;
using TMPro;
using UI.Navigation;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens
{
    public class StatisticsUI : BaseUI
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI maxHeightText;
        [SerializeField] private TextMeshProUGUI totalCoinsText;
        [SerializeField] private Button backButton;
        [SerializeField] private string maxHeightFormat = "Best Height: {0}m";
        [SerializeField] private string totalCoinsFormat = "Total Coins: {0}";
        
        private MenuNavigationController _navigation;

        protected override void Awake()
        {
            base.Awake();
            _navigation = GetComponent<MenuNavigationController>();
        }
        
        private void Start()
        {
            backButton?.onClick.AddListener(() => UIManager.Instance?.ReturnToMainMenu());
        }
        
        public override void Show()
        {
            base.Show();
            UpdateStatisticsDisplay();
            _navigation?.Initialize();
        }

        public override void Hide()
        {
            base.Hide();
            _navigation?.SetActive(false);
        }
        
        private void UpdateStatisticsDisplay()
        {
            var gameData = DataManager.Instance?.GameData;
            if (gameData == null) return;
            
            if (maxHeightText)
                maxHeightText.text = string.Format(maxHeightFormat, Mathf.RoundToInt(gameData.maxHeight));
                
            if (totalCoinsText)
                totalCoinsText.text = string.Format(totalCoinsFormat, gameData.totalCoins);
        }

        public MenuNavigationController GetNavigation() => _navigation;
    }
}