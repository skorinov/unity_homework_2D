using Controllers.Player;
using Managers;
using System.Collections.Generic;
using UnityEngine;

namespace Controllers.Platform.Actions
{
    [CreateAssetMenu(fileName = "CrumblingAction", menuName = "Platform Actions/Crumbling Action")]
    public class CrumblingAction : PlatformAction
    {
        [SerializeField] private float crumbleDuration = 2f;
        [SerializeField] private float respawnDelay = 5f;

        private readonly Dictionary<BasePlatform, CrumbleData> _crumbleData = new();

        private struct CrumbleData
        {
            public bool isCrumbling;
            public bool isBroken;
            public float crumbleTimer;
            public float respawnTimer;
            public Color originalColor;
            public bool hasPlayer;
        }

        public override void Initialize(BasePlatform platform)
        {
            var spriteRenderer = platform.GetComponent<SpriteRenderer>();
            _crumbleData[platform] = new CrumbleData
            {
                originalColor = spriteRenderer ? spriteRenderer.color : Color.white,
                isCrumbling = false,
                isBroken = false,
                crumbleTimer = 0f,
                respawnTimer = 0f,
                hasPlayer = false
            };
        }

        public override void OnPlayerLanded(PlayerController player, BasePlatform platform)
        {
            if (!_crumbleData.TryGetValue(platform, out var data) || data.isCrumbling || data.isBroken) return;

            data.hasPlayer = true;
            data.isCrumbling = true;
            data.crumbleTimer = crumbleDuration;
            _crumbleData[platform] = data;
        }

        public override void OnPlayerStaying(PlayerController player, BasePlatform platform)
        {
            if (_crumbleData.TryGetValue(platform, out var data) && !data.isBroken)
            {
                data.hasPlayer = true;
                _crumbleData[platform] = data;
            }
        }

        public override void OnPlayerLeft(PlayerController player, BasePlatform platform)
        {
            if (_crumbleData.TryGetValue(platform, out var data))
            {
                data.hasPlayer = false;
                if (data.isCrumbling && !data.isBroken)
                {
                    data.isCrumbling = false;
                    data.crumbleTimer = 0f;
                    var spriteRenderer = platform.GetComponent<SpriteRenderer>();
                    if (spriteRenderer) spriteRenderer.color = data.originalColor;
                }
                _crumbleData[platform] = data;
            }
        }

        public override void OnUpdate(BasePlatform platform)
        {
            if (!_crumbleData.TryGetValue(platform, out var data)) return;

            if (data.isBroken)
            {
                HandleRespawn(platform, data);
                return;
            }

            if (data.isCrumbling)
                HandleCrumbling(platform, data);
        }

        private void HandleRespawn(BasePlatform platform, CrumbleData data)
        {
            data.respawnTimer -= Time.deltaTime;
            if (data.respawnTimer <= 0f)
            {
                RestorePlatform(platform, data);
                data.isBroken = false;
                data.isCrumbling = false;
                data.respawnTimer = 0f;
            }
            _crumbleData[platform] = data;
        }

        private void HandleCrumbling(BasePlatform platform, CrumbleData data)
        {
            data.crumbleTimer -= Time.deltaTime;

            if (data.crumbleTimer <= 0f)
            {
                BreakPlatform(platform, data);
                data.isCrumbling = false;
                data.isBroken = true;
                data.respawnTimer = respawnDelay;
            }
            else
            {
                UpdateCrumbleVisual(platform, data);
            }

            _crumbleData[platform] = data;
        }

        private void BreakPlatform(BasePlatform platform, CrumbleData data)
        {
            var spriteRenderer = platform.GetComponent<SpriteRenderer>();
            var boxCollider = platform.GetComponent<BoxCollider2D>();
            
            if (spriteRenderer) spriteRenderer.color = Color.clear;
            if (boxCollider) boxCollider.enabled = false;
        }

        private void UpdateCrumbleVisual(BasePlatform platform, CrumbleData data)
        {
            var spriteRenderer = platform.GetComponent<SpriteRenderer>();
            if (spriteRenderer)
            {
                float fadeAmount = data.crumbleTimer / crumbleDuration;
                Color color = data.originalColor;
                color.a = fadeAmount;
                spriteRenderer.color = color;
            }
        }

        private void RestorePlatform(BasePlatform platform, CrumbleData data)
        {
            var spriteRenderer = platform.GetComponent<SpriteRenderer>();
            var boxCollider = platform.GetComponent<BoxCollider2D>();
            var platformEffector = platform.GetComponent<PlatformEffector2D>();
            
            if (spriteRenderer) spriteRenderer.color = data.originalColor;
            if (boxCollider) 
            {
                boxCollider.enabled = true;
                boxCollider.usedByEffector = true;
            }
            if (platformEffector) platformEffector.useOneWay = true;
        }

        public override void OnReset(BasePlatform platform)
        {
            if (_crumbleData.TryGetValue(platform, out var data))
            {
                RestorePlatform(platform, data);
                
                if (platform.GetComponent<SpriteRenderer>() is var spriteRenderer && spriteRenderer)
                    data.originalColor = spriteRenderer.color;
                
                data.isCrumbling = false;
                data.isBroken = false;
                data.crumbleTimer = 0f;
                data.respawnTimer = 0f;
                data.hasPlayer = false;
                _crumbleData[platform] = data;
            }
        }

        private void OnDestroy() => _crumbleData.Clear();
    }
}