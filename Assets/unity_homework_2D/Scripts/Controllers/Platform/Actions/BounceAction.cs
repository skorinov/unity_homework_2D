using Controllers.Player;
using System.Collections.Generic;
using UnityEngine;

namespace Controllers.Platform.Actions
{
    [CreateAssetMenu(fileName = "BounceAction", menuName = "Platform Actions/Chance Bounce Action")]
    public class BounceAction : PlatformAction
    {
        [SerializeField, Range(0f, 100f)] private float bounceChance = 30f;
        [SerializeField] private float jumpForceMultiplier = 1.5f;
        [SerializeField] private float bounceScale = 1.1f;
        [SerializeField] private float bounceDuration = 0.2f;

        private readonly Dictionary<BasePlatform, BounceState> _bounceStates = new();
        
        public override void SetChance(float chance) => bounceChance = Mathf.Clamp(chance, 0f, 100f);
        public override bool HasChance() => true;

        private struct BounceState
        {
            public Vector3 normalScale;
            public float bounceTimer;
            public bool isBouncing;
        }

        public override void Initialize(BasePlatform platform)
        {
            _bounceStates[platform] = new BounceState
            {
                normalScale = platform.transform.localScale,
                bounceTimer = 0f,
                isBouncing = false
            };
        }

        public override void OnPlayerLanded(PlayerController player, BasePlatform platform)
        {
            if (!_bounceStates.TryGetValue(platform, out var state)) return;

            if (Random.Range(0f, 100f) <= bounceChance)
            {
                player.Jump(player.BaseJumpForce * jumpForceMultiplier);
                StartBounceEffect(platform, ref state);
                _bounceStates[platform] = state;
            }
        }

        public override void OnUpdate(BasePlatform platform)
        {
            if (!_bounceStates.TryGetValue(platform, out var state) || !state.isBouncing) return;

            state.bounceTimer -= Time.deltaTime;
            if (state.bounceTimer <= 0f)
            {
                platform.transform.localScale = state.normalScale;
                state.isBouncing = false;
                _bounceStates[platform] = state;
            }
        }

        public override void OnReset(BasePlatform platform)
        {
            if (_bounceStates.TryGetValue(platform, out var state))
            {
                platform.transform.localScale = state.normalScale;
                state.isBouncing = false;
                state.bounceTimer = 0f;
                _bounceStates[platform] = state;
            }
        }

        private void StartBounceEffect(BasePlatform platform, ref BounceState state)
        {
            platform.transform.localScale = state.normalScale * bounceScale;
            state.bounceTimer = bounceDuration;
            state.isBouncing = true;
        }

        private void OnDestroy() => _bounceStates.Clear();
    }
}