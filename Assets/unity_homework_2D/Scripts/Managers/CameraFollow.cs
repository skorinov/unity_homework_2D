using UnityEngine;

namespace Managers
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float followSpeed = 3f;
        [SerializeField] private float offsetY = 2f;
        [SerializeField] private float minY = 0f;
        
        private void LateUpdate()
        {
            if (!target) return;
            
            Vector3 targetPos = new Vector3(
                transform.position.x,
                Mathf.Max(target.position.y + offsetY, minY),
                transform.position.z
            );
            
            transform.position = Vector3.Lerp(
                transform.position,
                targetPos,
                followSpeed * Time.deltaTime
            );
        }
        
        public void SetTarget(Transform newTarget) => target = newTarget;
        
        public void SetFollowSettings(float speed, float yOffset, float minimumY)
        {
            followSpeed = speed;
            offsetY = yOffset;
            minY = minimumY;
        }
    }
}