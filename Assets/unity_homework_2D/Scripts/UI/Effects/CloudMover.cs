using UnityEngine;

namespace UI.Effects
{
    public class CloudMover : MonoBehaviour
    {
        [SerializeField] private float menuMoveSpeed = 0.8f;
        [SerializeField] private float gameMoveSpeed = 1.5f;
        [SerializeField] private float resetPosition = -20f;
        [SerializeField] private float startPosition = 20f;
        
        [SerializeField] private bool enableFloating = true;
        [SerializeField] private float floatingSpeed = 1.5f;
        [SerializeField] private float floatingAmplitude = 0.3f;
        
        private Vector3 _originalPosition;
        private float _randomOffset;
        private bool _isGameMode = false;
        private float _currentMoveSpeed;
        
        private void Start()
        {
            _originalPosition = transform.position;
            _randomOffset = Random.Range(0f, Mathf.PI * 2f);
            _currentMoveSpeed = menuMoveSpeed;
            
            ResetPosition();
        }
        
        private void Update()
        {
            // Use unscaledDeltaTime for menu animations to continue during pause
            float deltaTime = _isGameMode ? Time.deltaTime : Time.unscaledDeltaTime;
            
            MoveClouds(deltaTime);
            
            if (enableFloating && !_isGameMode)
                FloatingAnimation(deltaTime);
        }
        
        public void SetGameMode(bool isGameMode)
        {
            _isGameMode = isGameMode;
            _currentMoveSpeed = isGameMode ? gameMoveSpeed : menuMoveSpeed;
            
            if (isGameMode)
            {
                transform.position = new Vector3(
                    transform.position.x,
                    _originalPosition.y,
                    transform.position.z
                );
            }
        }
        
        public void SetMoveSpeed(float menuSpeed, float gameSpeed)
        {
            menuMoveSpeed = menuSpeed;
            gameMoveSpeed = gameSpeed;
            _currentMoveSpeed = _isGameMode ? gameSpeed : menuSpeed;
        }
        
        private void MoveClouds(float deltaTime)
        {
            transform.Translate(Vector3.left * (_currentMoveSpeed * deltaTime));
            
            if (transform.position.x < resetPosition)
                ResetPosition();
        }
        
        private void FloatingAnimation(float deltaTime)
        {
            // Use Time.unscaledTime for menu floating animation
            float floatingY = _originalPosition.y + Mathf.Sin(Time.unscaledTime * floatingSpeed + _randomOffset) * floatingAmplitude;
            
            transform.position = new Vector3(
                transform.position.x,
                floatingY,
                transform.position.z
            );
        }
        
        private void ResetPosition()
        {
            float newX = Random.Range(startPosition * 0.5f, startPosition);
            transform.position = new Vector3(newX, _originalPosition.y, _originalPosition.z);
        }
    }
}