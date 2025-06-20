using Controllers.Player;
using UnityEngine;
using System.Collections.Generic;
using Controllers.Platform.Actions;
using Controllers.Platform.Presets;
using Pooling;

namespace Controllers.Platform
{
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(PlatformEffector2D))]
    public class BasePlatform : MonoBehaviour, IPoolable
    {
        [SerializeField] private List<PlatformAction> platformActions = new();

        private SpriteRenderer _spriteRenderer;
        private BoxCollider2D _boxCollider;
        private PlatformEffector2D _platformEffector;
        private PlayerController _playerOnPlatform;

        private Vector2 _originalColliderSize;
        private Vector2 _originalColliderOffset;
        private Color _originalColor;
        private Vector2 _originalSpriteSize;
        private float _originalWidth;

        private bool _isInitialized;
        private bool _hasActiveActions;

        private const string PLAYER_TAG = "Player";
        private const float LANDING_VELOCITY_THRESHOLD = 0.1f;

        private void Awake()
        {
            if (!_isInitialized)
                InitializePlatform();
        }

        private void OnEnable() => ResetPlatform();

        private void Update()
        {
            if (!_hasActiveActions) return;

            for (int i = 0; i < platformActions.Count; i++)
                platformActions[i]?.OnUpdate(this);
        }

        public void OnCreatedInPool() => InitializeActions();

        public void OnGetFromPool()
        {
            ResetPlatform();
            gameObject.SetActive(true);
        }

        public void OnReturnToPool()
        {
            _playerOnPlatform = null;
            
            if (_hasActiveActions)
            {
                for (int i = 0; i < platformActions.Count; i++)
                    platformActions[i]?.OnReset(this);
            }
            
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
            _hasActiveActions = platformActions.Count > 0;
            
            if (_hasActiveActions)
            {
                for (int i = 0; i < platformActions.Count; i++)
                    platformActions[i]?.Initialize(this);
            }
        }

        private void CacheOriginalValues()
        {
            if (_spriteRenderer)
            {
                _originalColor = _spriteRenderer.color;
                _originalSpriteSize = _spriteRenderer.size;
                _originalWidth = _spriteRenderer.size.x;
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
            
            // Only reset action states, don't remove actions
            if (_hasActiveActions)
            {
                for (int i = 0; i < platformActions.Count; i++)
                    platformActions[i]?.OnReset(this);
            }
        }

        public void SetCustomWidth(float width)
        {
            if (!_spriteRenderer) return;

            // Update sprite renderer size (for sliced sprites)
            _spriteRenderer.size = new Vector2(width, _spriteRenderer.size.y);
            
            // Update collider size to match new width
            if (_boxCollider)
            {
                _boxCollider.size = new Vector2(width, _boxCollider.size.y);
            }
            
            // Update original size for reset purposes
            _originalSpriteSize = _spriteRenderer.size;
            _originalColliderSize = _boxCollider.size;
            _originalWidth = width;
        }

        public void DisableCollision() => Invoke(nameof(ResetCollision), 0.2f);
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
                    for (int i = 0; i < platformActions.Count; i++)
                        platformActions[i]?.OnPlayerLanded(player, this);
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
                    for (int i = 0; i < platformActions.Count; i++)
                        platformActions[i]?.OnPlayerStaying(player, this);
                }
            }
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            if (other.gameObject.CompareTag(PLAYER_TAG))
            {
                var player = other.gameObject.GetComponent<PlayerController>();

                if (_hasActiveActions)
                {
                    for (int i = 0; i < platformActions.Count; i++)
                        platformActions[i]?.OnPlayerLeft(player, this);
                }

                _playerOnPlatform = null;
            }
        }

        private bool IsPlayerLanding(Collision2D collision) =>
            collision.gameObject.CompareTag(PLAYER_TAG) && 
            collision.rigidbody.linearVelocity.y <= LANDING_VELOCITY_THRESHOLD;

        public void BreakPlatform()
        {
            if (_spriteRenderer) _spriteRenderer.color = Color.clear;
            if (_boxCollider) _boxCollider.enabled = false;
            PlatformPool.Instance?.ReturnPlatform(this);
        }

        public void AddAction(PlatformAction action)
        {
            if (action && !platformActions.Contains(action))
            {
                platformActions.Add(action);
                action.Initialize(this);
                _hasActiveActions = platformActions.Count > 0;
            }
        }

        public void RemoveAction(PlatformAction action)
        {
            if (action && platformActions.Contains(action))
            {
                action.OnReset(this);
                platformActions.Remove(action);
                _hasActiveActions = platformActions.Count > 0;
            }
        }

        public void ClearActions()
        {
            if (_hasActiveActions)
            {
                for (int i = 0; i < platformActions.Count; i++)
                    platformActions[i]?.OnReset(this);
            }
            
            platformActions.Clear();
            _hasActiveActions = false;
        }

        public void ApplyPreset(PlatformPreset preset)
        {
            if (!preset) return;

            preset.ApplyToPlatform(this);

            if (_spriteRenderer)
                _originalColor = _spriteRenderer.color;
        }

        public bool HasAction<T>() where T : PlatformAction
        {
            for (int i = 0; i < platformActions.Count; i++)
            {
                if (platformActions[i] is T) return true;
            }
            return false;
        }

        public T GetAction<T>() where T : PlatformAction
        {
            for (int i = 0; i < platformActions.Count; i++)
            {
                if (platformActions[i] is T action) return action;
            }
            return null;
        }
    }
}