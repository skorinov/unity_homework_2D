using System.Collections.Generic;
using UI.Effects;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Navigation
{
    public class MenuNavigationController : MonoBehaviour
    {
        [SerializeField] private List<Button> _buttons = new();
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = new Color(1f, 1f, 0.5f, 1f);
        
        private int _currentIndex = 0;
        private bool _isActive = false;
        
        public System.Action<int> OnNavigateUp;
        public System.Action<int> OnNavigateDown;
        public System.Action OnConfirm;

        public List<Button> buttons => _buttons;
        
        private void Awake()
        {
            if (_buttons.Count == 0)
                _buttons.AddRange(GetComponentsInChildren<Button>());
        }
        
        public void Initialize()
        {
            if (_buttons.Count == 0) return;
            
            _currentIndex = FindFirstValidButtonIndex();
            _isActive = true;
            UpdateButtonStates();
        }
        
        public void SetActive(bool active)
        {
            _isActive = active;
            if (!active)
                ClearSelection();
        }
        
        public void NavigateUp()
        {
            if (!_isActive || _buttons.Count == 0) return;
            
            int startIndex = _currentIndex;
            do
            {
                _currentIndex = (_currentIndex - 1 + _buttons.Count) % _buttons.Count;
            }
            while (!IsButtonValid(_currentIndex) && _currentIndex != startIndex);
            
            UpdateButtonStates();
            Managers.AudioManager.Instance?.PlayButtonHover();
            OnNavigateUp?.Invoke(_currentIndex);
        }
        
        public void NavigateDown()
        {
            if (!_isActive || _buttons.Count == 0) return;
            
            int startIndex = _currentIndex;
            do
            {
                _currentIndex = (_currentIndex + 1) % _buttons.Count;
            }
            while (!IsButtonValid(_currentIndex) && _currentIndex != startIndex);
            
            UpdateButtonStates();
            Managers.AudioManager.Instance?.PlayButtonHover();
            OnNavigateDown?.Invoke(_currentIndex);
        }
        
        public void Confirm()
        {
            if (!_isActive || _buttons.Count == 0 || !IsButtonValid(_currentIndex)) 
                return;
                
            var button = _buttons[_currentIndex];
            Managers.AudioManager.Instance?.PlayButtonClick();
            button.onClick.Invoke();
            OnConfirm?.Invoke();
        }
        
        private bool IsButtonValid(int index)
        {
            if (index < 0 || index >= _buttons.Count || !_buttons[index]) return false;
            
            var button = _buttons[index];
            return button.gameObject.activeInHierarchy && button.interactable;
        }
        
        private int FindFirstValidButtonIndex()
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                if (IsButtonValid(i)) return i;
            }
            return 0; // Fallback to first button if none are valid
        }
        
        private void UpdateButtonStates()
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                if (!_buttons[i]) continue;
                
                bool isSelected = i == _currentIndex;
                
                // Update ButtonHoverEffects if present
                var hoverEffect = _buttons[i].GetComponent<ButtonHoverEffects>();
                if (hoverEffect)
                {
                    hoverEffect.SetKeyboardSelected(isSelected);
                }
                else
                {
                    // Fallback to direct color change
                    var targetGraphic = _buttons[i].targetGraphic;
                    if (targetGraphic)
                    {
                        targetGraphic.color = isSelected ? selectedColor : normalColor;
                    }
                }
            }
        }
        
        public void SetCurrentIndex(int index)
        {
            if (index >= 0 && index < _buttons.Count && IsButtonValid(index))
            {
                _currentIndex = index;
                UpdateButtonStates();
            }
        }
        
        private void ClearSelection()
        {
            foreach (var button in _buttons)
            {
                if (!button) continue;
                
                var hoverEffect = button.GetComponent<ButtonHoverEffects>();
                if (hoverEffect)
                {
                    hoverEffect.SetKeyboardSelected(false);
                }
                else if (button.targetGraphic)
                {
                    button.targetGraphic.color = normalColor;
                }
            }
        }
        
        public void AddButton(Button button)
        {
            if (button && !_buttons.Contains(button))
                _buttons.Add(button);
        }
        
        public void RemoveButton(Button button)
        {
            if (_buttons.Contains(button))
                _buttons.Remove(button);
        }
    }
}