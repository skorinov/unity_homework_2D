using UnityEngine;

namespace Controllers.Platform
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovingPlatform : BasePlatform
    {
        [Header("Moving Settings")]
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float moveRange = 5f;
        [SerializeField] private bool startMovingRight = true;
        
        private Rigidbody2D _rb;
        private float _startX;
        private int _direction;
        
        public override PlatformType GetPlatformType() => PlatformType.Moving;
        
        protected override void Awake()
        {
            base.Awake();
            InitializeRigidbody();
        }
        
        private void InitializeRigidbody()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (_rb)
            {
                _rb.bodyType = RigidbodyType2D.Kinematic;
                _rb.gravityScale = 0f;
                _rb.freezeRotation = true;
            }
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            ResetMovement();
        }
        
        public override void ResetPlatform()
        {
            base.ResetPlatform();
            ResetMovement();
        }
        
        private void ResetMovement()
        {
            _startX = transform.position.x;
            _direction = startMovingRight ? 1 : -1;
        }
        
        private void FixedUpdate()
        {
            if (!_rb) return;
            MovePlatform();
        }
        
        private void MovePlatform()
        {
            Vector2 currentPos = _rb.position;
            float newX = currentPos.x + _direction * moveSpeed * Time.fixedDeltaTime;
            
            // Check boundaries and reverse direction
            float distanceFromStart = newX - _startX;
            float halfRange = moveRange * 0.5f;
            
            if (distanceFromStart >= halfRange)
            {
                _direction = -1;
                newX = _startX + halfRange;
            }
            else if (distanceFromStart <= -halfRange)
            {
                _direction = 1;
                newX = _startX - halfRange;
            }
            
            // Move platform using physics
            Vector2 newPosition = new Vector2(newX, currentPos.y);
            _rb.MovePosition(newPosition);
        }
    }
}