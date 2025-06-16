using UnityEngine;

namespace Controllers.Player
{
    /// System that teleports player from one side of screen to another
    public class ScreenWrappingSystem : MonoBehaviour
    {
        [Header("Screen Wrapping Settings")]
        [SerializeField] private Camera targetCamera;
        
        // Cached screen boundaries for performance
        private float _screenHalfWidth;
        private float _lastCameraSize;
        
        private void Start()
        {
            // Use main camera if no specific camera assigned
            if (!targetCamera)
            {
                targetCamera = Camera.main;
            }
            
            // Calculate initial screen boundaries
            UpdateScreenBounds();
        }
        
        private void Update()
        {
            // Early exit if no camera assigned
            if (!targetCamera) return;
            
            // Recalculate bounds when camera size changes
            CheckForCameraSizeChange();
            
            // Check if player needs to be teleported to opposite side
            CheckScreenWrapping();
        }
        
        /// Detects camera size changes and updates screen bounds accordingly
        private void CheckForCameraSizeChange()
        {
            float currentSize = targetCamera.orthographicSize;
            
            if (!Mathf.Approximately(currentSize, _lastCameraSize))
            {
                UpdateScreenBounds();
                _lastCameraSize = currentSize;
            }
        }
        
        /// Calculates and caches screen boundaries in world coordinates
        private void UpdateScreenBounds()
        {
            if (!targetCamera) return;
            
            // Calculate half-width of screen in world units
            // orthographicSize = half-height of camera view
            // aspect = width/height ratio (e.g., 1.77 for 16:9)
            _screenHalfWidth = targetCamera.orthographicSize * targetCamera.aspect;
        }
        
        /// Main wrapping logic: teleports player to opposite side when they exit screen
        private void CheckScreenWrapping()
        {
            Vector3 playerPos = transform.position;
            float cameraX = targetCamera.transform.position.x;
            
            // Calculate player position relative to camera center
            float relativeX = playerPos.x - cameraX;
            
            // Player went too far RIGHT: teleport to LEFT side
            if (relativeX > _screenHalfWidth)
            {
                // Place player on left edge of screen
                playerPos.x = cameraX - _screenHalfWidth;
                transform.position = playerPos;
            }
            // Player went too far LEFT: teleport to RIGHT side
            else if (relativeX < -_screenHalfWidth)
            {
                // Place player on right edge of screen
                playerPos.x = cameraX + _screenHalfWidth;
                transform.position = playerPos;
            }
        }
    }
}