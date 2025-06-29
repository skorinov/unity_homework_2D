using Constants;
using Controllers.Player;
using Controllers.Platform.Actions;
using Controllers.Platform.Actions.Presets;
using Pooling;
using System.Collections.Generic;
using UnityEngine;

namespace Controllers.Platform
{
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(PlatformEffector2D))]
    public class BasePlatform : MonoBehaviour, IPoolable
    {
        [SerializeField] private PlatformPreset platformPreset;

        private List<PlatformAction> _runtimeActions = new();
        private SpriteRenderer _spriteRenderer;
        private BoxCollider2D _boxCollider;
        private PlatformEffector2D _platformEffector;
        private PlayerController _playerOnPlatform;

        private Vector2 _originalColliderSize;
        private Vector2 _originalColliderOffset;
        private Color _originalColor;
        private Vector2 _originalSpriteSize;

        private bool _isInitialized;
        private bool _hasActiveActions;

        private void Awake()
        {
            if (!_isInitialized)
                InitializePlatform();
        }

        private void OnEnable() => ResetPlatform();

        private void Update()
        {
            if (!_hasActiveActions) return;

            for (int i = 0; i < _runtimeActions.Count; i++)
                _runtimeActions[i]?.OnUpdate(this);
        }

        public void OnCreatedInPool() => InitializeActions();

        public void OnGetFromPool()
        {
            ResetPlatform();
            gameObject.SetActive(true);
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
            
            // Apply preset if available and actions not yet loaded
            if (platformPreset && _runtimeActions.Count == 0)
                ApplyPreset(platformPreset);
            
            // Notify actions that platform is ready with actual position
            if (_hasActiveActions)
            {
                for (int i = 0; i < _runtimeActions.Count; i++)
                    _runtimeActions[i]?.OnPlatformReady(this, position);
            }
        }

        public void OnReturnToPool()
        {
            _playerOnPlatform = null;
            
            if (_hasActiveActions)
            {
                for (int i = 0; i < _runtimeActions.Count; i++)
                    _runtimeActions[i]?.OnReset(this);
            }
            
            ClearRuntimeActions();
            gameObject.SetActive(false);
        }

        public bool CanReturnToPool() => _playerOnPlatform == null;

        private void InitializePlatform()
        {
            CacheComponents();
            CacheOriginalValues();
            SetupOneWayCollision();
            _isInitialized = true;
        }

        private void CacheComponents()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _boxCollider = GetComponent<BoxCollider2D>();
            _platformEffector = GetComponent<PlatformEffector2D>();
        }

        private void InitializeActions()
        {
            if (platformPreset)
                ApplyPreset(platformPreset);
        }

        private void CacheOriginalValues()
        {
            if (_spriteRenderer)
            {
                _originalColor = _spriteRenderer.color;
                _originalSpriteSize = _spriteRenderer.size;
            }

            if (_boxCollider)
            {
                _originalColliderSize = _boxCollider.size;
                _originalColliderOffset = _boxCollider.offset;
            }
        }

        private void SetupOneWayCollision()
        {
            if (!_platformEffector) return;

            _platformEffector.useOneWay = true;
            _platformEffector.surfaceArc = 180f;
            _boxCollider.usedByEffector = true;
        }

        private void ResetPlatform()
        {
            if (_spriteRenderer)
            {
                _spriteRenderer.color = _originalColor;
                _spriteRenderer.size = _originalSpriteSize;
            }

            if (_boxCollider)
            {
                _boxCollider.enabled = true;
                _boxCollider.size = _originalColliderSize;
                _boxCollider.offset = _originalColliderOffset;
                _boxCollider.usedByEffector = true;
            }
            
            _playerOnPlatform = null;
            
            if (_hasActiveActions)
            {
                for (int i = 0; i < _runtimeActions.Count; i++)
                    _runtimeActions[i]?.OnReset(this);
            }
        }

        public void DisableCollision() => Invoke(nameof(ResetCollision), GameConstants.DROP_THROUGH_COLLISION_DELAY);
        private void ResetCollision()
        {
            if (_boxCollider) _boxCollider.enabled = true;
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (IsPlayerLanding(other))
            {
                var player = other.gameObject.GetComponent<PlayerController>();
                _playerOnPlatform = player;

                if (_hasActiveActions)
                {
                    for (int i = 0; i < _runtimeActions.Count; i++)
                        _runtimeActions[i]?.OnPlayerLanded(player, this);
                }
            }
        }

        private void OnCollisionStay2D(Collision2D other)
        {
            if (IsPlayerLanding(other))
            {
                var player = other.gameObject.GetComponent<PlayerController>();
                _playerOnPlatform = player;

                if (_hasActiveActions)
                {
                    for (int i = 0; i < _runtimeActions.Count; i++)
                        _runtimeActions[i]?.OnPlayerStaying(player, this);
                }
            }
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            if (other.gameObject.CompareTag(GameConstants.PLAYER_TAG))
            {
                var player = other.gameObject.GetComponent<PlayerController>();

                if (_hasActiveActions)
                {
                    for (int i = 0; i < _runtimeActions.Count; i++)
                        _runtimeActions[i]?.OnPlayerLeft(player, this);
                }

                _playerOnPlatform = null;
            }
        }

        private bool IsPlayerLanding(Collision2D collision) =>
            collision.gameObject.CompareTag(GameConstants.PLAYER_TAG) && 
            collision.rigidbody.linearVelocity.y <= GameConstants.LANDING_VELOCITY_THRESHOLD;

        public void ApplyPreset(PlatformPreset preset)
        {
            if (!preset) return;

            ClearRuntimeActions();
            preset.ApplyToPlatform(this);
        }

        // Called by PlatformPreset to add actions
        public void AddAction(PlatformAction action)
        {
            if (!action) return;

            var actionInstance = Instantiate(action);
            _runtimeActions.Add(actionInstance);
            actionInstance.Initialize(this);
            _hasActiveActions = _runtimeActions.Count > 0;
        }

        private void ClearRuntimeActions()
        {
            if (_hasActiveActions)
            {
                for (int i = 0; i < _runtimeActions.Count; i++)
                    _runtimeActions[i]?.OnReset(this);
            }
            
            _runtimeActions.Clear();
            _hasActiveActions = false;
        }
    }
}