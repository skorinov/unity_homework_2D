using Managers;
using TMPro;
using UI.Navigation;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens
{
    public class SettingsUI : BaseUI
    {
        [Header("UI Elements")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform content;
        [SerializeField] private Button backButton;
        
        [Header("Audio Settings")]
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private TextMeshProUGUI musicVolumeValue;
        [SerializeField] private TextMeshProUGUI sfxVolumeValue;
        
        [Header("Input Settings")]
        [SerializeField] private Button jumpKeyButton;
        [SerializeField] private Button dropKeyButton;
        [SerializeField] private TextMeshProUGUI jumpKeyText;
        [SerializeField] private TextMeshProUGUI dropKeyText;
        
        private MenuNavigationController _navigation;
        private bool _isWaitingForKey = false;
        private bool _isSettingJumpKey = false;
        
        protected override void Awake()
        {
            base.Awake();
            _navigation = GetComponent<MenuNavigationController>();
            
            // Подписываемся на события навигации для автоскролла
            if (_navigation)
            {
                _navigation.OnNavigateUp += OnNavigationChanged;
                _navigation.OnNavigateDown += OnNavigationChanged;
            }
        }
        
        private void Start()
        {
            SetupCallbacks();
            LoadSettings();
        }
        
        public override void Show()
        {
            base.Show();
            LoadSettings();
            _navigation?.Initialize();
            if (scrollRect) scrollRect.verticalNormalizedPosition = 1f;
        }
        
        public override void Hide()
        {
            base.Hide();
            _navigation?.SetActive(false);
        }
        
        private void Update()
        {
            if (!_isWaitingForKey) return;
            
            // Check for any key press
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key) && IsValidInputKey(key))
                {
                    SetNewKey(key);
                    return;
                }
            }
        }
        
        private void SetupCallbacks()
        {
            backButton?.onClick.AddListener(() => UIManager.Instance?.ReturnToMainMenu());
            
            musicVolumeSlider?.onValueChanged.AddListener(OnMusicVolumeChanged);
            sfxVolumeSlider?.onValueChanged.AddListener(OnSFXVolumeChanged);
            
            jumpKeyButton?.onClick.AddListener(() => StartKeyBinding(true));
            dropKeyButton?.onClick.AddListener(() => StartKeyBinding(false));
        }
        
        private void LoadSettings()
        {
            // Load audio settings
            float musicVolume = AudioManager.Instance?.GetMusicVolume() ?? 0.5f;
            float sfxVolume = AudioManager.Instance?.GetSFXVolume() ?? 0.5f;
            
            // Convert from 0-1 to 0-100 for sliders
            if (musicVolumeSlider) musicVolumeSlider.SetValueWithoutNotify(musicVolume * 100f);
            if (sfxVolumeSlider) sfxVolumeSlider.SetValueWithoutNotify(sfxVolume * 100f);
            
            UpdateVolumeTexts();
            
            // Load input settings
            UpdateKeyTexts();
        }
        
        private void UpdateVolumeTexts()
        {
            if (musicVolumeValue && musicVolumeSlider)
                musicVolumeValue.text = $"{Mathf.RoundToInt(musicVolumeSlider.value)}%";
            if (sfxVolumeValue && sfxVolumeSlider)
                sfxVolumeValue.text = $"{Mathf.RoundToInt(sfxVolumeSlider.value)}%";
        }
        
        private void UpdateKeyTexts()
        {
            if (jumpKeyText) jumpKeyText.text = GetJumpKey().ToString();
            if (dropKeyText) dropKeyText.text = GetDropKey().ToString();
        }
        
        private void OnMusicVolumeChanged(float value)
        {
            // Convert from 0-100 to 0-1 for AudioManager
            float normalizedValue = value / 100f;
            AudioManager.Instance?.SetMusicVolume(normalizedValue);
            if (musicVolumeValue) musicVolumeValue.text = $"{Mathf.RoundToInt(value)}%";
        }
        
        private void OnSFXVolumeChanged(float value)
        {
            // Convert from 0-100 to 0-1 for AudioManager
            float normalizedValue = value / 100f;
            AudioManager.Instance?.SetSFXVolume(normalizedValue);
            if (sfxVolumeValue) sfxVolumeValue.text = $"{Mathf.RoundToInt(value)}%";
        }
        
        private void StartKeyBinding(bool isJumpKey)
        {
            _isWaitingForKey = true;
            _isSettingJumpKey = isJumpKey;
            
            // Временно отключаем UI input чтобы не мешал
            InputManager.Instance?.DisableAllInput();
            
            if (isJumpKey && jumpKeyText)
                jumpKeyText.text = "Press Key...";
            else if (dropKeyText)
                dropKeyText.text = "Press Key...";
        }
        
        private void SetNewKey(KeyCode newKey)
        {
            _isWaitingForKey = false;
            
            // Включаем UI input обратно
            InputManager.Instance?.EnableUIInput();
            
            if (_isSettingJumpKey)
            {
                SetJumpKey(newKey);
            }
            else
            {
                SetDropKey(newKey);
            }
            
            UpdateKeyTexts();
        }
        
        private bool IsValidInputKey(KeyCode key)
        {
            // Exclude system keys and UI navigation keys, but ALLOW them during key binding
            if (_isWaitingForKey)
            {
                // During key binding, only exclude escape
                return key != KeyCode.Escape;
            }
            
            // Normal navigation - exclude UI keys
            return key != KeyCode.Escape && 
                   key != KeyCode.Return && 
                   key != KeyCode.Tab &&
                   key != KeyCode.UpArrow &&
                   key != KeyCode.DownArrow &&
                   key != KeyCode.LeftArrow &&
                   key != KeyCode.RightArrow;
        }
        
        // Helper methods to get/set input keys
        private KeyCode GetJumpKey()
        {
            return InputManager.Instance?.GetJumpKey() ?? KeyCode.Space;
        }
        
        private KeyCode GetDropKey()
        {
            return InputManager.Instance?.GetDropKey() ?? KeyCode.S;
        }
        
        private void SetJumpKey(KeyCode key)
        {
            InputManager.Instance?.SetJumpKey(key);
        }
        
        private void SetDropKey(KeyCode key)
        {
            InputManager.Instance?.SetDropKey(key);
        }
        
        public MenuNavigationController GetNavigation() => _navigation;
        
        public void ScrollUp()
        {
            if (scrollRect) scrollRect.verticalNormalizedPosition = 
                Mathf.Clamp01(scrollRect.verticalNormalizedPosition + 0.1f);
        }
        
        public void ScrollDown()
        {
            if (scrollRect) scrollRect.verticalNormalizedPosition = 
                Mathf.Clamp01(scrollRect.verticalNormalizedPosition - 0.1f);
        }
        
        private void OnNavigationChanged(int currentIndex)
        {
            ScrollToCurrentSelection(currentIndex);
        }
        
        private void ScrollToCurrentSelection(int index)
        {
            if (!scrollRect || !content || !_navigation) return;
            
            var selectables = _navigation.selectables;
            if (index < 0 || index >= selectables.Count) return;
            
            // Simple approach: scroll based on element index
            float totalElements = selectables.Count;
            if (totalElements <= 1) return;
            
            // Calculate scroll position based on index (0 = top, 1 = bottom)
            float scrollPosition = 1f - (index / (totalElements - 1f));
            scrollPosition = Mathf.Clamp01(scrollPosition);
            
            // Animate scroll
            StartCoroutine(AnimateScrollTo(scrollPosition));
        }
        
        private System.Collections.IEnumerator AnimateScrollTo(float targetPosition)
        {
            float startPos = scrollRect.verticalNormalizedPosition;
            float time = 0f;
            float duration = 0.3f;
            
            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                float t = time / duration;
                scrollRect.verticalNormalizedPosition = Mathf.Lerp(startPos, targetPosition, t);
                yield return null;
            }
            
            scrollRect.verticalNormalizedPosition = targetPosition;
        }
        
        private void OnDestroy()
        {
            if (_navigation)
            {
                _navigation.OnNavigateUp -= OnNavigationChanged;
                _navigation.OnNavigateDown -= OnNavigationChanged;
            }
        }
    }
}