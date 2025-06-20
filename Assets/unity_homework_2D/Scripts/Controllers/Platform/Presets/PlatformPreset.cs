using System.Collections.Generic;
using Controllers.Platform.Actions;
using UnityEngine;

namespace Controllers.Platform.Presets
{
    [CreateAssetMenu(fileName = "PlatformPreset", menuName = "Platform System/Platform Preset")]
    public class PlatformPreset : ScriptableObject
    {
        [Header("Preset Info")]
        public string presetName;
        [TextArea(2, 4)] public string description;

        [Header("Visual Settings")]
        public Color platformColor = Color.white;
        public Sprite platformSprite;

        [Header("Size Settings")]
        public float minWidth = 1f;
        public float maxWidth = 3f;

        [Header("Actions")]
        public List<PlatformAction> actions = new();

        [Header("Spawn Settings")]
        [Range(0f, 100f)] public float spawnChance = 25f;

        public void ApplyToPlatform(BasePlatform platform)
        {
            if (!platform) return;

            var spriteRenderer = platform.GetComponent<SpriteRenderer>();
            if (spriteRenderer)
            {
                if (platformSprite) spriteRenderer.sprite = platformSprite;
                spriteRenderer.color = platformColor;
            }

            platform.ClearActions();
            foreach (var action in actions)
            {
                if (action) platform.AddAction(action);
            }
        }

        public float GetRandomWidth() => Random.Range(minWidth, maxWidth);
    }
}