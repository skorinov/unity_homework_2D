using Controllers.Player;
using Managers;
using UnityEngine;
using System.Collections.Generic;

namespace Controllers.Platform.Actions
{
    [CreateAssetMenu(fileName = "CrumblingAction", menuName = "Platform Actions/Crumbling Action")]
    public class CrumblingAction : PlatformAction
    {
        [Header("Crumbling Settings")]
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
                data.respawnTimer -= Time.deltaTime;
                if (data.respawnTimer <= 0f)
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
                    
                    data.isBroken = false;
                    data.isCrumbling = false;
                    data.respawnTimer = 0f;
                }
                _crumbleData[platform] = data;
                return;
            }

            if (!data.isCrumbling) return;

            data.crumbleTimer -= Time.deltaTime;

            if (data.crumbleTimer <= 0f)
            {
                var spriteRenderer = platform.GetComponent<SpriteRenderer>();
                var boxCollider = platform.GetComponent<BoxCollider2D>();
                
                if (spriteRenderer) spriteRenderer.color = Color.clear;
                if (boxCollider) boxCollider.enabled = false;
                
                AudioManager.Instance?.PlayPlatformBreakSound();
                
                data.isCrumbling = false;
                data.isBroken = true;
                data.respawnTimer = respawnDelay;
            }
            else
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

            _crumbleData[platform] = data;
        }

        public override void OnReset(BasePlatform platform)
        {
            if (_crumbleData.TryGetValue(platform, out var data))
            {
                var spriteRenderer = platform.GetComponent<SpriteRenderer>();
                var boxCollider = platform.GetComponent<BoxCollider2D>();
                var platformEffector = platform.GetComponent<PlatformEffector2D>();
                
                if (spriteRenderer) 
                {
                    spriteRenderer.color = data.originalColor;
                    data.originalColor = spriteRenderer.color;
                }
                if (boxCollider) 
                {
                    boxCollider.enabled = true;
                    boxCollider.usedByEffector = true;
                }
                if (platformEffector) platformEffector.useOneWay = true;
                
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