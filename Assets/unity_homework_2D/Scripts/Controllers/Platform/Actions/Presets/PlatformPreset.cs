using System.Collections.Generic;
using UnityEngine;

namespace Controllers.Platform.Actions.Presets
{
    [CreateAssetMenu(fileName = "PlatformPreset", menuName = "Platform System/Platform Preset")]
    public class PlatformPreset : ScriptableObject
    {
        [SerializeField] public string presetName;
        [SerializeField, TextArea(2, 4)] public string description;
        [SerializeField] public List<PlatformAction> actions = new();
        [SerializeField, Range(-1f, 100f)] private float overrideChance = -1f;

        public void ApplyToPlatform(BasePlatform platform)
        {
            if (!platform) return;
            ApplyActions(platform);
        }

        private void ApplyActions(BasePlatform platform)
        {
            foreach (var action in actions)
            {
                if (action)
                {
                    var actionInstance = Instantiate(action);
                    
                    if (actionInstance.HasChance() && overrideChance >= 0f)
                        actionInstance.SetChance(overrideChance);
                
                    platform.AddAction(actionInstance);
                }
            }
        }
    }
}