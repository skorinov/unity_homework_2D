using Controllers.Player;
using System.Collections.Generic;
using UnityEngine;

namespace Controllers.Platform.Actions
{
    [CreateAssetMenu(fileName = "CrumblingAction", menuName = "Platform Actions/Crumbling Action")]
    public class CrumblingAction : PlatformAction
    {
        [SerializeField, Range(0f, 100f)] private float crumblingChance = 30f;
        [SerializeField] private float crumbleSpeed = 0.3f;
        [SerializeField] private float restoreSpeed = 0.5f;
        [SerializeField] private float autoRestoreDelay = 2f;
        [SerializeField] private Color crumblingColor = new Color(0.8f, 0.4f, 0.4f, 1f);

        private readonly Dictionary<BasePlatform, CrumblingState> _states = new();
        
        public override void SetChance(float chance) => crumblingChance = Mathf.Clamp(chance, 0f, 100f);
        public override bool HasChance() => true;

        private struct CrumblingState
        {
            public bool playerOnPlatform;
            public float crumbleProgress;
            public float autoRestoreTimer;
            public Color originalColor;
            public Vector3 originalScale;
            public bool isDestroyed;
        }

        public override void Initialize(BasePlatform platform)
        {
            if (Random.Range(0f, 100f) > crumblingChance) return;

            var spriteRenderer = platform.GetComponent<SpriteRenderer>();
            _states[platform] = new CrumblingState
            {
                originalColor = spriteRenderer ? spriteRenderer.color : Color.white,
                originalScale = platform.transform.localScale
            };
        }

        public override void OnPlayerLanded(PlayerController player, BasePlatform platform)
        {
            SetPlayerState(platform, true);
        }

        public override void OnPlayerStaying(PlayerController player, BasePlatform platform)
        {
            SetPlayerState(platform, true);
        }

        public override void OnPlayerLeft(PlayerController player, BasePlatform platform)
        {
            SetPlayerState(platform, false);
        }

        private void SetPlayerState(BasePlatform platform, bool onPlatform)
        {
            if (_states.TryGetValue(platform, out var state))
            {
                state.playerOnPlatform = onPlatform;
                _states[platform] = state;
            }
        }

        public override void OnUpdate(BasePlatform platform)
        {
            if (!_states.TryGetValue(platform, out var state)) return;

            // Auto-restore check
            if (state.isDestroyed)
            {
                state.autoRestoreTimer -= Time.deltaTime;
                if (state.autoRestoreTimer <= 0f)
                {
                    RestoreCompletely(platform, ref state);
                    return;
                }
                _states[platform] = state;
                return;
            }

            float oldProgress = state.crumbleProgress;

            // Update progress
            if (state.playerOnPlatform)
            {
                state.crumbleProgress = Mathf.Clamp01(state.crumbleProgress + crumbleSpeed * Time.deltaTime);
            }
            else if (state.crumbleProgress > 0f)
            {
                state.crumbleProgress = Mathf.Clamp01(state.crumbleProgress - restoreSpeed * Time.deltaTime);
            }

            // Check for destruction
            if (state.crumbleProgress >= 1f)
            {
                state.isDestroyed = true;
                state.autoRestoreTimer = autoRestoreDelay;
                UpdateVisuals(platform, state, true);
            }
            else if (!Mathf.Approximately(oldProgress, state.crumbleProgress))
            {
                UpdateVisuals(platform, state, false);
            }

            _states[platform] = state;
        }

        private void UpdateVisuals(BasePlatform platform, CrumblingState state, bool forceDestroy)
        {
            var spriteRenderer = platform.GetComponent<SpriteRenderer>();
            var boxCollider = platform.GetComponent<BoxCollider2D>();

            if (forceDestroy || state.isDestroyed)
            {
                if (spriteRenderer) spriteRenderer.color = Color.clear;
                if (boxCollider) boxCollider.enabled = false;
                ToggleChildren(platform, false);
            }
            else
            {
                if (spriteRenderer)
                {
                    var color = Color.Lerp(state.originalColor, crumblingColor, state.crumbleProgress);
                    color.a = Mathf.Lerp(1f, 0.3f, state.crumbleProgress);
                    spriteRenderer.color = color;
                }
                
                if (boxCollider) boxCollider.enabled = true;
                
                float scale = Mathf.Lerp(1f, 0.8f, state.crumbleProgress);
                platform.transform.localScale = state.originalScale * scale;
            }
        }

        private void RestoreCompletely(BasePlatform platform, ref CrumblingState state)
        {
            var spriteRenderer = platform.GetComponent<SpriteRenderer>();
            var boxCollider = platform.GetComponent<BoxCollider2D>();

            if (spriteRenderer) spriteRenderer.color = state.originalColor;
            if (boxCollider) boxCollider.enabled = true;
            
            platform.transform.localScale = state.originalScale;
            ToggleChildren(platform, true);

            state = new CrumblingState
            {
                originalColor = state.originalColor,
                originalScale = state.originalScale
            };
            _states[platform] = state;
        }

        private void ToggleChildren(BasePlatform platform, bool enabled)
        {
            for (int i = 0; i < platform.transform.childCount; i++)
            {
                var child = platform.transform.GetChild(i);
                var renderer = child.GetComponent<Renderer>();
                var collider = child.GetComponent<Collider2D>();
                
                if (renderer) renderer.enabled = enabled;
                if (collider) collider.enabled = enabled;
            }
        }

        public override void OnReset(BasePlatform platform)
        {
            if (_states.TryGetValue(platform, out var state))
            {
                RestoreCompletely(platform, ref state);
            }
        }

        private void OnDestroy() => _states.Clear();
    }
}