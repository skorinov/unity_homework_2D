using Constants;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Effects
{
    [RequireComponent(typeof(Image))]
    public class CloudMover : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 50f;
        [SerializeField] private float floatingSpeed = 1.5f;
        [SerializeField] private float floatingAmplitude = 30f;
        [SerializeField] private float spawnOffset = 0f;
        
        private RectTransform _rectTransform;
        private Canvas _parentCanvas;
        private Vector2 _startPosition;
        private float _randomOffset;
        private float _canvasHalfWidth;
        
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _parentCanvas = GetComponentInParent<Canvas>();
            _startPosition = _rectTransform.anchoredPosition;
            _randomOffset = Random.Range(0f, Mathf.PI * 2f);
        }
        
        private void Start()
        {
            UpdateCanvasSize();
        }
        
        private void Update()
        {
            MoveCloud();
            ApplyFloating();
        }
        
        private void MoveCloud()
        {
            Vector2 pos = _rectTransform.anchoredPosition;
            pos.x -= moveSpeed * Time.unscaledDeltaTime;
            
            // Reset to right edge when goes beyond left edge
            if (pos.x < -_canvasHalfWidth - spawnOffset)
            {
                pos.x = _canvasHalfWidth + spawnOffset;
            }
            
            _rectTransform.anchoredPosition = pos;
        }
        
        private void ApplyFloating()
        {
            float floatingY = _startPosition.y + Mathf.Sin(Time.unscaledTime * floatingSpeed + _randomOffset) * floatingAmplitude;
            
            Vector2 pos = _rectTransform.anchoredPosition;
            pos.y = floatingY;
            _rectTransform.anchoredPosition = pos;
        }
        
        private void UpdateCanvasSize()
        {
            if (_parentCanvas?.GetComponent<RectTransform>())
            {
                _canvasHalfWidth = _parentCanvas.GetComponent<RectTransform>().rect.width * GameConstants.HALF_WIDTH_MULTIPLIER;
            }
            else
            {
                _canvasHalfWidth = Screen.width * GameConstants.HALF_WIDTH_MULTIPLIER;
            }
        }
    }
}