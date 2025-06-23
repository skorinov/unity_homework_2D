using Data;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens
{
    public class GameUI : BaseUI
    {
        [SerializeField] private TextMeshProUGUI heightText;
        [SerializeField] private TextMeshProUGUI coinsText;
        [SerializeField] private Button menuButton;
        [SerializeField] private string heightFormat = "{0}m";
        [SerializeField] private string coinsFormat = "{0}";
        
        private void Start()
        {
            menuButton?.onClick.AddListener(() => UIManager.Instance?.ReturnToMainMenu());
        }
        
        public override void Show()
        {
            base.Show();
            UpdateDisplay();
        }
        
        private void Update()
        {
            if (gameObject.activeInHierarchy)
                UpdateDisplay();
        }
        
        private void UpdateDisplay()
        {
            var gameData = DataManager.Instance?.GameData;
            if (gameData == null) return;
            
            if (heightText)
                heightText.text = string.Format(heightFormat, Mathf.RoundToInt(gameData.sessionHeight));
                
            if (coinsText)
                coinsText.text = string.Format(coinsFormat, gameData.sessionCoins);
        }
    }
}