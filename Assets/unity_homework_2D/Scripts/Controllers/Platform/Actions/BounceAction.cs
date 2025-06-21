using System.Collections.Generic;
using Controllers.Player;
using Managers;
using UnityEngine;

namespace Controllers.Platform.Actions
{
    [CreateAssetMenu(fileName = "BounceAction", menuName = "Platform Actions/Bounce Action")]
    public class BounceAction : PlatformAction
    {
        [Header("Bounce Settings")]
        [SerializeField] private float jumpForceMultiplier = 1.5f;
        [SerializeField] private float bounceScale = 1.1f;
        [SerializeField] private float bounceDuration = 0.2f;

        private readonly Dictionary<BasePlatform, BounceData> _bounceData = new();

        private struct BounceData
        {
            public Vector3 normalScale;
            public float bounceTimer;
            public bool isBouncing;
        }

        public override void Initialize(BasePlatform platform)
        {
            _bounceData[platform] = new BounceData
            {
                normalScale = platform.transform.localScale,
                bounceTimer = 0f,
                isBouncing = false
            };
        }

        public override void OnPlayerLanded(PlayerController player, BasePlatform platform)
        {
            if (_bounceData.TryGetValue(platform, out var data))
            {
                data.normalScale = platform.transform.localScale;
                _bounceData[platform] = data;
            }

            player.Jump(player.BaseJumpForce * jumpForceMultiplier);
            AudioManager.Instance?.PlayBounceSound();
            StartBounceEffect(platform);
        }

        public override void OnUpdate(BasePlatform platform)
        {
            if (!_bounceData.TryGetValue(platform, out var data) || !data.isBouncing) return;

            data.bounceTimer -= Time.deltaTime;
            if (data.bounceTimer <= 0f)
            {
                platform.transform.localScale = data.normalScale;
                data.isBouncing = false;
            }
            _bounceData[platform] = data;
        }

        public override void OnReset(BasePlatform platform)
        {
            if (_bounceData.TryGetValue(platform, out var data))
            {
                platform.transform.localScale = data.normalScale;
                data.isBouncing = false;
                data.bounceTimer = 0f;
                _bounceData[platform] = data;
            }
        }

        private void StartBounceEffect(BasePlatform platform)
        {
            if (!_bounceData.TryGetValue(platform, out var data)) return;

            platform.transform.localScale = data.normalScale * bounceScale;
            data.bounceTimer = bounceDuration;
            data.isBouncing = true;
            _bounceData[platform] = data;
        }

        private void OnDestroy() => _bounceData.Clear();
    }
}