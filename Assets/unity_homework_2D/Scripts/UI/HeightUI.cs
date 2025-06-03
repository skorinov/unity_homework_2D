using UnityEngine;
using Managers;
using TMPro;

namespace UI
{
    public class HeightUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private TextMeshProUGUI  heightText;
        
        [Header("Settings")]
        [SerializeField] private string heightFormat = "{0}m";
        
        private float _startHeight;
        
        private void Start()
        {
            if (!gameManager) return;
            // Subscribe to height change event
            gameManager.OnHeightChanged += OnHeightChanged;

            if (!gameManager.GetPlayer()) return;
            _startHeight = gameManager.GetPlayer().position.y;
            UpdateDisplay(gameManager.GetHighestPlayerY());
        }
        
        private void OnDestroy()
        {
            // Unsubscribe to prevent memory leaks
            if (gameManager)
            {
                gameManager.OnHeightChanged -= OnHeightChanged;
            }
        }
        
        private void OnHeightChanged(float newMaxHeight)
        {
            UpdateDisplay(newMaxHeight);
        }
        
        private void UpdateDisplay(float maxHeight)
        {
            if (!heightText) return;
            
            int heightValue = Mathf.RoundToInt(maxHeight - _startHeight);
            heightText.text = string.Format(heightFormat, Mathf.Max(0, heightValue));
        }
        
        // Called when game restarts
        public void ResetHeight()
        {
            if (!gameManager?.GetPlayer()) return;
            _startHeight = gameManager.GetPlayer().position.y;
            UpdateDisplay(gameManager.GetHighestPlayerY());
        }
    }
}