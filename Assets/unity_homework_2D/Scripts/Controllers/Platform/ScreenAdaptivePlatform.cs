using Constants;
using UnityEngine;

namespace Controllers.Platform
{
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class ScreenAdaptivePlatform : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private float widthPadding = 1f;
        [SerializeField] private bool adaptOnStart = true;
        [SerializeField] private bool adaptOnScreenChange = true;
        
        private SpriteRenderer _spriteRenderer;
        private BoxCollider2D _boxCollider;
        private float _lastScreenWidth;
        
        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _boxCollider = GetComponent<BoxCollider2D>();
            
            if (!targetCamera) 
                targetCamera = Camera.main;
        }
        
        private void Start()
        {
            if (adaptOnStart)
                AdaptToScreen();
        }
        
        private void Update()
        {
            if (adaptOnScreenChange && HasScreenSizeChanged())
                AdaptToScreen();
        }
        
        public void AdaptToScreen()
        {
            if (!targetCamera) return;
            
            float screenWidth = GetScreenWidth();
            float targetWidth = screenWidth + widthPadding * 2f;
            
            UpdateSpriteSize(targetWidth);
            UpdateColliderSize(targetWidth);
            
            _lastScreenWidth = screenWidth;
        }
        
        private float GetScreenWidth()
        {
            return targetCamera.orthographicSize * targetCamera.aspect * GameConstants.ORTHOGRAPHIC_SIZE_TO_FULL_SIZE_MULTIPLIER;
        }
        
        private bool HasScreenSizeChanged()
        {
            float currentScreenWidth = GetScreenWidth();
            return !Mathf.Approximately(currentScreenWidth, _lastScreenWidth);
        }
        
        private void UpdateSpriteSize(float targetWidth)
        {
            if (!_spriteRenderer) return;
            
            Vector2 spriteSize = _spriteRenderer.size;
            spriteSize.x = targetWidth;
            _spriteRenderer.size = spriteSize;
        }
        
        private void UpdateColliderSize(float targetWidth)
        {
            if (!_boxCollider) return;
            
            Vector2 colliderSize = _boxCollider.size;
            colliderSize.x = targetWidth;
            _boxCollider.size = colliderSize;
        }
    }
}