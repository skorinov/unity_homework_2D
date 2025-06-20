using UnityEngine;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private PlatformGenerator platformGenerator;
        [SerializeField] private float cameraFollowSpeed = 3f;
        [SerializeField] private float cameraOffsetY = 2f;
        [SerializeField] private float minCameraY = 0f;

        public System.Action<float> OnHeightChanged;

        private float _highestPlayerY;

        private void Start() => InitializeGame();

        private void Update()
        {
            if (!player || !mainCamera) return;

            UpdatePlayerHeight();
            UpdateCamera();
            platformGenerator?.UpdateGeneration(player.position.y);
        }

        private void InitializeGame()
        {
            if (!player) return;

            _highestPlayerY = player.position.y;
            OnHeightChanged?.Invoke(_highestPlayerY);
            platformGenerator?.Initialize(player.position);
        }

        private void UpdatePlayerHeight()
        {
            if (player.position.y > _highestPlayerY)
            {
                _highestPlayerY = player.position.y;
                OnHeightChanged?.Invoke(_highestPlayerY);
            }
        }

        private void UpdateCamera()
        {
            Vector3 targetPos = new Vector3(
                mainCamera.transform.position.x,
                Mathf.Max(player.position.y + cameraOffsetY, minCameraY),
                mainCamera.transform.position.z
            );

            mainCamera.transform.position = Vector3.Lerp(
                mainCamera.transform.position,
                targetPos,
                cameraFollowSpeed * Time.deltaTime
            );
        }

        public void ResetGame()
        {
            if (!player) return;

            _highestPlayerY = player.position.y;
            OnHeightChanged?.Invoke(_highestPlayerY);
            platformGenerator?.ResetGeneration(player.position);
        }

        public float GetHighestPlayerY() => _highestPlayerY;
        public Transform GetPlayer() => player;
    }
}