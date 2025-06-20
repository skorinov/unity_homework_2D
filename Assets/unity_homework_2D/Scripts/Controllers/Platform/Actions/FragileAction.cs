using Controllers.Player;
using Managers;
using UnityEngine;
using System.Collections.Generic;

namespace Controllers.Platform.Actions
{
    [CreateAssetMenu(fileName = "FragileAction", menuName = "Platform Actions/Fragile Action")]
    public class FragileAction : PlatformAction
    {
        [Header("Fragile Settings")]
        [SerializeField] private int hitsToBreak = 2;
        [SerializeField] private float respawnDelay = 5f;

        private readonly Dictionary<BasePlatform, FragileData> _fragileData = new();

        private struct FragileData
        {
            public int hitCount;
            public bool isBroken;
            public float respawnTimer;
            public Color originalColor;
        }

        public override void Initialize(BasePlatform platform)
        {
            var spriteRenderer = platform.GetComponent<SpriteRenderer>();
            _fragileData[platform] = new FragileData
            {
                hitCount = 0,
                isBroken = false,
                respawnTimer = 0f,
                originalColor = spriteRenderer ? spriteRenderer.color : Color.white
            };
        }

        public override void OnPlayerLanded(PlayerController player, BasePlatform platform)
        {
            if (!_fragileData.TryGetValue(platform, out var data) || data.isBroken) return;

            data.hitCount++;

            if (data.hitCount >= hitsToBreak)
            {
                var spriteRenderer = platform.GetComponent<SpriteRenderer>();
                var boxCollider = platform.GetComponent<BoxCollider2D>();
                
                if (spriteRenderer) spriteRenderer.color = Color.clear;
                if (boxCollider) boxCollider.enabled = false;
                
                AudioManager.Instance?.PlayPlatformBreakSound();
                
                data.isBroken = true;
                data.respawnTimer = respawnDelay;
            }

            _fragileData[platform] = data;
        }

        public override void OnUpdate(BasePlatform platform)
        {
            if (!_fragileData.TryGetValue(platform, out var data) || !data.isBroken) return;

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
                
                data.hitCount = 0;
                data.isBroken = false;
                data.respawnTimer = 0f;
            }
            _fragileData[platform] = data;
        }

        public override void OnReset(BasePlatform platform)
        {
            if (_fragileData.TryGetValue(platform, out var data))
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
                
                data.hitCount = 0;
                data.isBroken = false;
                data.respawnTimer = 0f;
                _fragileData[platform] = data;
            }
        }

        private void OnDestroy() => _fragileData.Clear();
    }
}