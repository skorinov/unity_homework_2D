using Constants;
using UI.Effects;
using UnityEngine;
using Utilities;

namespace Managers
{
    public class WorldSpaceMenuSystem : Singleton<WorldSpaceMenuSystem>
    {
        [SerializeField] private Vector3 menuAreaCenter = new Vector3(GameConstants.MENU_AREA_X, 0, 0);
        [SerializeField] private Vector3 gameAreaCenter = new Vector3(GameConstants.GAME_AREA_X, 0, 0);
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float transitionSpeed = 2f;
        [SerializeField] private float menuCameraSize = 6f;
        [SerializeField] private float gameCameraSize = 5f;
        [SerializeField] private GameObject[] menuObjects;
        [SerializeField] private Transform cloudsParent;

        private bool _isMenuMode = true;
        private bool _isTransitioning = false;
        private CameraFollow _cameraFollow;
        private CloudMover[] _cloudMovers;
        
        public bool IsMenuMode => _isMenuMode;
        public bool IsTransitioning => _isTransitioning;
        
        protected override void OnSingletonAwake()
        {
            SetupCamera();
            SetupClouds();
            SetMenuMode(true, false);
        }
        
        private void SetupCamera()
        {
            if (!mainCamera) mainCamera = Camera.main;
            
            _cameraFollow = mainCamera.GetComponent<CameraFollow>();
            if (!_cameraFollow)
                _cameraFollow = mainCamera.gameObject.AddComponent<CameraFollow>();
        }

        private void SetupClouds()
        {
            if (cloudsParent)
                _cloudMovers = cloudsParent.GetComponentsInChildren<CloudMover>();
        }
        
        public void SetMenuMode(bool isMenuMode, bool animate = true)
        {
            if (_isTransitioning) return;
            
            _isMenuMode = isMenuMode;
            
            if (animate)
                StartCoroutine(TransitionToArea(isMenuMode));
            else
                SetCameraInstant(isMenuMode);
            
            SetMenuObjectsActive(isMenuMode);
            SetCloudsGameMode(!isMenuMode);
        }
        
        private System.Collections.IEnumerator TransitionToArea(bool toMenuMode)
        {
            _isTransitioning = true;
            
            Vector3 startPos = mainCamera.transform.position;
            Vector3 targetPos = toMenuMode ? menuAreaCenter : gameAreaCenter;
            targetPos.z = startPos.z;
            
            float startSize = mainCamera.orthographicSize;
            float targetSize = toMenuMode ? menuCameraSize : gameCameraSize;
            
            float elapsed = 0f;
            float duration = 1f / transitionSpeed;
            
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                
                mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
                mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
                
                yield return null;
            }
            
            mainCamera.transform.position = targetPos;
            mainCamera.orthographicSize = targetSize;
            
            SetupCameraForMode(toMenuMode);
            _isTransitioning = false;
        }

        private void SetCameraInstant(bool isMenuMode)
        {
            Vector3 targetPos = isMenuMode ? menuAreaCenter : gameAreaCenter;
            targetPos.z = mainCamera.transform.position.z;
            mainCamera.transform.position = targetPos;
            mainCamera.orthographicSize = isMenuMode ? menuCameraSize : gameCameraSize;
            
            SetupCameraForMode(isMenuMode);
        }
        
        private void SetupCameraForMode(bool isMenuMode)
        {
            if (_cameraFollow)
                _cameraFollow.enabled = !isMenuMode;
        }
        
        private void SetMenuObjectsActive(bool active)
        {
            if (menuObjects == null) return;
            
            foreach (var obj in menuObjects)
            {
                if (obj) obj.SetActive(active);
            }
        }

        private void SetCloudsGameMode(bool isGameMode)
        {
            if (_cloudMovers == null) return;
            
            foreach (var cloudMover in _cloudMovers)
            {
                cloudMover?.SetGameMode(isGameMode);
            }
        }
        
        public void SetGameCameraTarget(Transform target)
        {
            if (_cameraFollow && target)
                _cameraFollow.SetTarget(target);
        }
        
        public Vector3 GetMenuAreaCenter() => menuAreaCenter;
        public Vector3 GetGameAreaCenter() => gameAreaCenter;
    }
}