using System.Collections.Generic;
using Constants;
using UI.Effects;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Navigation
{
    public class MenuNavigationController : MonoBehaviour
    {
        [SerializeField] private List<Selectable> _selectables = new();
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = new Color(1f, 1f, 0.5f, 1f);
        [SerializeField] private float sliderStep = 5f;
        
        private int _currentIndex = 0;
        private bool _isActive = false;
        
        public System.Action<int> OnNavigateUp;
        public System.Action<int> OnNavigateDown;
        public System.Action OnConfirm;

        public List<Selectable> selectables => _selectables;
        
        private void Awake()
        {
            if (_selectables.Count == 0)
                _selectables.AddRange(GetComponentsInChildren<Selectable>());
        }
        
        private void Update()
        {
            if (!_isActive) return;
            
            HandleHorizontalInput();
        }
        
        private void HandleHorizontalInput()
        {
            if (_selectables.Count == 0 || !IsSelectableValid(_currentIndex)) return;
            
            var currentSelectable = _selectables[_currentIndex];
            
            // Handle slider input
            if (currentSelectable is Slider slider)
            {
                float horizontalInput = Input.GetAxis(GameConstants.HORIZONTAL_INPUT);
                
                if (Mathf.Abs(horizontalInput) > GameConstants.INPUT_DEADZONE_THRESHOLD)
                {
                    float newValue = slider.value + horizontalInput * sliderStep * Time.unscaledDeltaTime;
                    slider.value = Mathf.Clamp(newValue, slider.minValue, slider.maxValue);
                }
                
                // Discrete step input
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                {
                    slider.value = Mathf.Max(slider.minValue, slider.value - sliderStep);
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                {
                    slider.value = Mathf.Min(slider.maxValue, slider.value + sliderStep);
                }
            }
        }
        
        public void Initialize()
        {
            if (_selectables.Count == 0) return;
            
            _currentIndex = FindFirstValidSelectableIndex();
            _isActive = true;
            UpdateSelectableStates();
        }
        
        public void SetActive(bool active)
        {
            _isActive = active;
            if (!active)
                ClearSelection();
        }
        
        public void NavigateUp()
        {
            if (!_isActive || _selectables.Count == 0) return;
            
            int startIndex = _currentIndex;
            do
            {
                _currentIndex = (_currentIndex - 1 + _selectables.Count) % _selectables.Count;
            }
            while (!IsSelectableValid(_currentIndex) && _currentIndex != startIndex);
            
            UpdateSelectableStates();
            Managers.AudioManager.Instance?.PlayButtonHover();
            OnNavigateUp?.Invoke(_currentIndex);
        }
        
        public void NavigateDown()
        {
            if (!_isActive || _selectables.Count == 0) return;
            
            int startIndex = _currentIndex;
            do
            {
                _currentIndex = (_currentIndex + 1) % _selectables.Count;
            }
            while (!IsSelectableValid(_currentIndex) && _currentIndex != startIndex);
            
            UpdateSelectableStates();
            Managers.AudioManager.Instance?.PlayButtonHover();
            OnNavigateDown?.Invoke(_currentIndex);
        }
        
        public void Confirm()
        {
            if (!_isActive || _selectables.Count == 0 || !IsSelectableValid(_currentIndex)) 
                return;
                
            var selectable = _selectables[_currentIndex];
            
            // Handle button clicks
            if (selectable is Button button)
            {
                Managers.AudioManager.Instance?.PlayButtonClick();
                button.onClick.Invoke();
                OnConfirm?.Invoke();
            }
            // Handle toggle switches
            else if (selectable is Toggle toggle)
            {
                Managers.AudioManager.Instance?.PlayButtonClick();
                toggle.isOn = !toggle.isOn;
                OnConfirm?.Invoke();
            }
            // Handle dropdown opening
            else if (selectable is Dropdown dropdown)
            {
                Managers.AudioManager.Instance?.PlayButtonClick();
                dropdown.Show();
                OnConfirm?.Invoke();
            }
        }
        
        private bool IsSelectableValid(int index)
        {
            if (index < 0 || index >= _selectables.Count || !_selectables[index]) return false;
            
            var selectable = _selectables[index];
            return selectable.gameObject.activeInHierarchy && selectable.interactable;
        }
        
        private int FindFirstValidSelectableIndex()
        {
            for (int i = 0; i < _selectables.Count; i++)
            {
                if (IsSelectableValid(i)) return i;
            }
            return 0;
        }
        
        private void UpdateSelectableStates()
        {
            for (int i = 0; i < _selectables.Count; i++)
            {
                if (!_selectables[i]) continue;
                
                bool isSelected = i == _currentIndex;
                
                // Update ButtonHoverEffects if present
                var hoverEffect = _selectables[i].GetComponent<ButtonHoverEffects>();
                if (hoverEffect)
                {
                    hoverEffect.SetKeyboardSelected(isSelected);
                }
                else
                {
                    // Fallback to direct color change for different UI elements
                    SetSelectableColor(_selectables[i], isSelected);
                }
            }
        }
        
        private void SetSelectableColor(Selectable selectable, bool isSelected)
        {
            Color targetColor = isSelected ? selectedColor : normalColor;
            
            if (selectable is Button button && button.targetGraphic)
            {
                button.targetGraphic.color = targetColor;
            }
            else if (selectable is Slider slider)
            {
                // Highlight slider handle
                var handle = slider.handleRect?.GetComponent<Image>();
                if (handle) handle.color = targetColor;
            }
            else if (selectable is Toggle toggle && toggle.targetGraphic)
            {
                toggle.targetGraphic.color = targetColor;
            }
            else if (selectable is Dropdown dropdown && dropdown.targetGraphic)
            {
                dropdown.targetGraphic.color = targetColor;
            }
        }
        
        public void SetCurrentIndex(int index)
        {
            if (index >= 0 && index < _selectables.Count && IsSelectableValid(index))
            {
                _currentIndex = index;
                UpdateSelectableStates();
            }
        }
        
        private void ClearSelection()
        {
            foreach (var selectable in _selectables)
            {
                if (!selectable) continue;
                
                var hoverEffect = selectable.GetComponent<ButtonHoverEffects>();
                if (hoverEffect)
                {
                    hoverEffect.SetKeyboardSelected(false);
                }
                else
                {
                    SetSelectableColor(selectable, false);
                }
            }
        }
    }
}