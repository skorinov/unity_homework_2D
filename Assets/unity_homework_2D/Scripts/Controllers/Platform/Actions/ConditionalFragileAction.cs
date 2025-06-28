using Controllers.Player;
using Pooling;
using System.Collections.Generic;
using UnityEngine;

namespace Controllers.Platform.Actions
{
    [CreateAssetMenu(fileName = "ConditionalFragileAction", menuName = "Platform Actions/Conditional Fragile Action")]
    public class ConditionalFragileAction : PlatformAction
    {
        [SerializeField] private float neighborCheckRadius = 10f;
        [SerializeField] private float respawnDelay = 3f;
        [SerializeField, Range(0f, 100f)] private float fragileChance = 20f;
        [SerializeField] private float sameHeightTolerance = 1f;
        [SerializeField] private int landingsToBreak = 2;

        private readonly Dictionary<BasePlatform, FragileState> _fragileStates = new();
        
        public override void SetChance(float chance) => fragileChance = Mathf.Clamp(chance, 0f, 100f);
        public override bool HasChance() => true;

        private struct FragileState
        {
            public bool isFragile;
            public bool isBroken;
            public float respawnTimer;
            public Color originalColor;
            public GameObject[] disabledChildren;
            public int landingCount;
        }

        public override void Initialize(BasePlatform platform)
        {
            bool isFragile = Random.Range(0f, 100f) <= fragileChance;
            var spriteRenderer = platform.GetComponent<SpriteRenderer>();
            _fragileStates[platform] = new FragileState
            {
                originalColor = spriteRenderer ? spriteRenderer.color : Color.white,
                isBroken = false,
                respawnTimer = 0f,
                disabledChildren = null,
                landingCount = 0,
                isFragile = isFragile
            };
        }

        public override void OnPlayerLanded(PlayerController player, BasePlatform platform)
        {
            if (!_fragileStates.TryGetValue(platform, out var state) || state.isBroken) return;

            if (state.isFragile && HasNearbyPlatforms(platform))
            {
                state.landingCount++;

                if (state.landingCount >= landingsToBreak)
                {
                    BreakPlatform(platform, ref state);
                    state.isBroken = true;
                    state.respawnTimer = respawnDelay;
                }

                _fragileStates[platform] = state;
            }
        }

        public override void OnUpdate(BasePlatform platform)
        {
            if (!_fragileStates.TryGetValue(platform, out var state) || !state.isBroken) return;

            state.respawnTimer -= Time.deltaTime;
            if (state.respawnTimer <= 0f)
            {
                RestorePlatform(platform, state);
                state.isBroken = false;
                _fragileStates[platform] = state;
            }
            else
            {
                _fragileStates[platform] = state;
            }
        }

        public override void OnReset(BasePlatform platform)
        {
            if (_fragileStates.TryGetValue(platform, out var state))
            {
                RestorePlatform(platform, state);
                state.isBroken = false;
                state.respawnTimer = 0f;
                state.disabledChildren = null;
                state.landingCount = 0;
                _fragileStates[platform] = state;
            }
        }

        private bool HasNearbyPlatforms(BasePlatform platform)
        {
            var activePlatforms = PlatformPool.Instance?.GetActivePlatforms();
            if (activePlatforms == null) return false;

            float platformY = platform.transform.position.y;
            float platformX = platform.transform.position.x;

            for (int i = 0; i < activePlatforms.Count; i++)
            {
                var other = activePlatforms[i];
                if (other == platform) continue;

                var otherTransform = other.transform;
                if (Mathf.Abs(platformY - otherTransform.position.y) <= sameHeightTolerance &&
                    Mathf.Abs(platformX - otherTransform.position.x) <= neighborCheckRadius)
                    return true;
            }

            return false;
        }

        private void BreakPlatform(BasePlatform platform, ref FragileState state)
        {
            var spriteRenderer = platform.GetComponent<SpriteRenderer>();
            var boxCollider = platform.GetComponent<BoxCollider2D>();

            if (spriteRenderer) spriteRenderer.color = Color.clear;
            if (boxCollider) boxCollider.enabled = false;

            // Hide child objects but keep them active for proper pool management
            int childCount = platform.transform.childCount;
            if (childCount > 0)
            {
                var hiddenChildren = new GameObject[childCount];
                int hiddenCount = 0;

                for (int i = 0; i < childCount; i++)
                {
                    var child = platform.transform.GetChild(i).gameObject;
                    if (child.activeInHierarchy)
                    {
                        var renderer = child.GetComponent<Renderer>();
                        if (renderer) renderer.enabled = false;
                        
                        var collider = child.GetComponent<Collider2D>();
                        if (collider) collider.enabled = false;
                        
                        hiddenChildren[hiddenCount++] = child;
                    }
                }

                if (hiddenCount > 0)
                {
                    state.disabledChildren = new GameObject[hiddenCount];
                    System.Array.Copy(hiddenChildren, state.disabledChildren, hiddenCount);
                }
            }
        }

        private void RestorePlatform(BasePlatform platform, FragileState state)
        {
            var spriteRenderer = platform.GetComponent<SpriteRenderer>();
            var boxCollider = platform.GetComponent<BoxCollider2D>();

            if (spriteRenderer) spriteRenderer.color = state.originalColor;
            if (boxCollider)
            {
                boxCollider.enabled = true;
                boxCollider.usedByEffector = true;
            }

            // Restore child objects
            if (state.disabledChildren != null)
            {
                for (int i = 0; i < state.disabledChildren.Length; i++)
                {
                    var child = state.disabledChildren[i];
                    if (child)
                    {
                        var renderer = child.GetComponent<Renderer>();
                        if (renderer) renderer.enabled = true;
                        
                        var collider = child.GetComponent<Collider2D>();
                        if (collider) collider.enabled = true;
                    }
                }
            }
        }

        private void OnDestroy() => _fragileStates.Clear();
    }
}