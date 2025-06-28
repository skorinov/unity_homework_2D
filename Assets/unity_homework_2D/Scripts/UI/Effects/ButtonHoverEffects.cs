using Constants;
using Managers;
using UI.Navigation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Effects
{
    [RequireComponent(typeof(Selectable))]
    public class ButtonHoverEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color hoverColor = new Color(1f, 1f, 0.5f, 1f);
        [SerializeField] private Color pressedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        [SerializeField] private float colorTransitionSpeed = 8f;
        
        [SerializeField] private bool enableScaleEffect = true;
        [SerializeField] private float hoverScale = 1.05f;
        [SerializeField] private float pressedScale = 0.95f;
        [SerializeField] private float scaleTransitionSpeed = 10f;
        
        private Selectable _selectable;
        private Image _targetImage;
        private Vector3 _originalScale;
        private Color _targetColor;
        private float _targetScale;
        private bool _isHovered;
        private bool _isPressed;
        private bool _isKeyboardSelected;
        private MenuNavigationController _navigation;
        
        private void Awake()
        {
            _selectable = GetComponent<Selectable>();
            _navigation = GetComponentInParent<MenuNavigationController>();
            _originalScale = transform.localScale;
            
            // Find the appropriate image to modify based on selectable type
            FindTargetImage();
            
            if (_targetImage)
            {
                normalColor = _targetImage.color;
                _targetColor = normalColor;
            }
            
            _targetScale = GameConstants.DEFAULT_MULTIPLIER;
        }
        
        private void FindTargetImage()
        {
            if (_selectable is Button button)
            {
                _targetImage = button.targetGraphic as Image ?? GetComponent<Image>();
            }
            else if (_selectable is Slider slider)
            {
                // For sliders, target the handle
                _targetImage = slider.handleRect?.GetComponent<Image>();
                if (!_targetImage)
                {
                    // Fallback to fill area or background
                    _targetImage = slider.fillRect?.GetComponent<Image>() ?? GetComponent<Image>();
                }
            }
            else if (_selectable is Toggle toggle)
            {
                _targetImage = toggle.targetGraphic as Image ?? GetComponent<Image>();
            }
            else if (_selectable is Dropdown dropdown)
            {
                _targetImage = dropdown.targetGraphic as Image ?? GetComponent<Image>();
            }
            else
            {
                // Generic fallback
                _targetImage = GetComponent<Image>();
            }
        }
        
        private void Update()
        {
            UpdateColorTransition();
            if (enableScaleEffect)
                UpdateScaleTransition();
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_selectable.interactable) return;
            
            _isHovered = true;
            UpdateNavigationSelection();
            SetHoverState();
            AudioManager.Instance?.PlayButtonHover();
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            
            if (!_isPressed && !_isKeyboardSelected)
            {
                SetNormalState();
            }
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_selectable.interactable) return;
            
            _isPressed = true;
            SetPressedState();
            
            // Different audio for different selectable types
            if (_selectable is Slider)
            {
                // Don't play click sound for sliders on mouse down
                return;
            }
            
            AudioManager.Instance?.PlayButtonClick();
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
            
            if (_isHovered || _isKeyboardSelected)
            {
                SetHoverState();
            }
            else
            {
                SetNormalState();
            }
        }
        
        public void SetKeyboardSelected(bool selected)
        {
            _isKeyboardSelected = selected;
            
            if (!_isPressed)
            {
                if (selected || _isHovered)
                    SetHoverState();
                else
                    SetNormalState();
            }
        }
        
        private void SetNormalState()
        {
            _targetColor = normalColor;
            if (enableScaleEffect)
                _targetScale = 1f;
        }
        
        private void SetHoverState()
        {
            _targetColor = hoverColor;
            if (enableScaleEffect)
                _targetScale = hoverScale;
        }
        
        private void SetPressedState()
        {
            _targetColor = pressedColor;
            if (enableScaleEffect)
                _targetScale = pressedScale;
        }
        
        private void UpdateNavigationSelection()
        {
            if (!_navigation) return;
            
            var navigationSelectables = _navigation.selectables;
            for (int i = 0; i < navigationSelectables.Count; i++)
            {
                if (navigationSelectables[i] == _selectable && 
                    _selectable.gameObject.activeInHierarchy && 
                    _selectable.interactable)
                {
                    _navigation.SetCurrentIndex(i);
                    break;
                }
            }
        }
        
        private void UpdateColorTransition()
        {
            if (!_targetImage) return;
            
            _targetImage.color = Color.Lerp(_targetImage.color, _targetColor, colorTransitionSpeed * Time.unscaledDeltaTime);
        }
        
        private void UpdateScaleTransition()
        {
            Vector3 targetScaleVector = _originalScale * _targetScale;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScaleVector, scaleTransitionSpeed * Time.unscaledDeltaTime);
        }
    }
}