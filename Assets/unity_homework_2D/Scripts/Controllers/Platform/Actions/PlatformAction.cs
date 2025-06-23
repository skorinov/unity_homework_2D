using Controllers.Player;
using UnityEngine;

namespace Controllers.Platform.Actions
{
    public abstract class PlatformAction : ScriptableObject
    {
        [SerializeField] public string actionName;
        [SerializeField, TextArea(2, 4)] public string description;

        public virtual void OnPlayerLanded(PlayerController player, BasePlatform platform) { }
        public virtual void OnPlayerStaying(PlayerController player, BasePlatform platform) { }
        public virtual void OnPlayerLeft(PlayerController player, BasePlatform platform) { }
        public virtual void OnUpdate(BasePlatform platform) { }
        public virtual void OnReset(BasePlatform platform) { }
        public virtual void Initialize(BasePlatform platform) { }
        public virtual void OnPlatformReady(BasePlatform platform) { }
    }
}