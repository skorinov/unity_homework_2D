using System.Collections.Generic;
using UnityEngine;

namespace Controllers.Platform.Actions.Presets
{
    [CreateAssetMenu(fileName = "PlatformPreset", menuName = "Platform System/Platform Preset")]
    public class PlatformPreset : ScriptableObject
    {
        [SerializeField] public string presetName;
        [SerializeField, TextArea(2, 4)] public string description;

        [SerializeField] public Color platformColor = Color.white;
        [SerializeField] public Sprite platformSprite;

        [SerializeField] public float minWidth = 1f;
        [SerializeField] public float maxWidth = 3f;

        [SerializeField] public List<PlatformAction> actions = new();

        [SerializeField, Range(0f, 100f)] public float spawnChance = 25f;

        public void ApplyToPlatform(BasePlatform platform)
        {
            if (!platform) return;

            ApplyVisualSettings(platform);
            ApplyActions(platform);
        }

        public float GetRandomWidth() => Random.Range(minWidth, maxWidth);

        private void ApplyVisualSettings(BasePlatform platform)
        {
            var spriteRenderer = platform.GetComponent<SpriteRenderer>();
            if (!spriteRenderer) return;

            if (platformSprite) spriteRenderer.sprite = platformSprite;
            spriteRenderer.color = platformColor;
        }

        private void ApplyActions(BasePlatform platform)
        {
            platform.ClearActions();
            foreach (var action in actions)
            {
                if (action) platform.AddAction(action);
            }
        }
    }
}