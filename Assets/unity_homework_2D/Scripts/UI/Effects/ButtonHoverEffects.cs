using Managers;
using UI.Navigation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Effects
{
    [RequireComponent(typeof(Button))]
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
        
        private Button _button;
        private Image _buttonImage;
        private Vector3 _originalScale;
        private Color _targetColor;
        private float _targetScale;
        private bool _isHovered;
        private bool _isPressed;
        private bool _isKeyboardSelected;
        private MenuNavigationController _navigation;
        
        private void Awake()
        {
            _button = GetComponent<Button>();
            _buttonImage = GetComponent<Image>();
            _originalScale = transform.localScale;
            _navigation = GetComponentInParent<MenuNavigationController>();
            
            if (_buttonImage)
            {
                normalColor = _buttonImage.color;
                _targetColor = normalColor;
            }
            
            _targetScale = 1f;
        }
        
        private void Update()
        {
            UpdateColorTransition();
            if (enableScaleEffect)
                UpdateScaleTransition();
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_button.interactable) return;
            
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
            if (!_button.interactable) return;
            
            _isPressed = true;
            SetPressedState();
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
            
            var navigationButtons = _navigation.buttons;
            for (int i = 0; i < navigationButtons.Count; i++)
            {
                if (navigationButtons[i] == _button && _button.gameObject.activeInHierarchy && _button.interactable)
                {
                    _navigation.SetCurrentIndex(i);
                    break;
                }
            }
        }
        
        private void UpdateColorTransition()
        {
            if (!_buttonImage) return;
            
            _buttonImage.color = Color.Lerp(_buttonImage.color, _targetColor, colorTransitionSpeed * Time.unscaledDeltaTime);
        }
        
        private void UpdateScaleTransition()
        {
            Vector3 targetScaleVector = _originalScale * _targetScale;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScaleVector, scaleTransitionSpeed * Time.unscaledDeltaTime);
        }
        
        public void SetColors(Color normal, Color hover, Color pressed)
        {
            normalColor = normal;
            hoverColor = hover;
            pressedColor = pressed;
            
            if (!_isHovered && !_isPressed && !_isKeyboardSelected)
                _targetColor = normalColor;
        }
        
        public void SetScaleSettings(bool enabled, float hover = 1.05f, float pressed = 0.95f)
        {
            enableScaleEffect = enabled;
            hoverScale = hover;
            pressedScale = pressed;
        }
    }
}