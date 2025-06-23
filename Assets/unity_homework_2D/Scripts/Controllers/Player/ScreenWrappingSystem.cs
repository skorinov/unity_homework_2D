using Managers;
using UnityEngine;

namespace Controllers.Player
{
    public class ScreenWrappingSystem : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;

        private float _screenHalfWidth;
        private float _lastCameraSize;

        private void Start()
        {
            if (!targetCamera) targetCamera = Camera.main;
            UpdateScreenBounds();
        }

        private void Update()
        {
            if (GameManager.Instance?.IsGameActive != true || !targetCamera) return;
            
            CheckForCameraSizeChange();
            CheckScreenWrapping();
        }

        private void CheckForCameraSizeChange()
        {
            float currentSize = targetCamera.orthographicSize;
            if (!Mathf.Approximately(currentSize, _lastCameraSize))
            {
                UpdateScreenBounds();
                _lastCameraSize = currentSize;
            }
        }

        private void UpdateScreenBounds()
        {
            if (!targetCamera) return;
            _screenHalfWidth = targetCamera.orthographicSize * targetCamera.aspect;
        }

        private void CheckScreenWrapping()
        {
            Vector3 playerPos = transform.position;
            float cameraX = targetCamera.transform.position.x;
            float relativeX = playerPos.x - cameraX;

            if (relativeX > _screenHalfWidth)
            {
                playerPos.x = cameraX - _screenHalfWidth;
                transform.position = playerPos;
            }
            else if (relativeX < -_screenHalfWidth)
            {
                playerPos.x = cameraX + _screenHalfWidth;
                transform.position = playerPos;
            }
        }
    }
}