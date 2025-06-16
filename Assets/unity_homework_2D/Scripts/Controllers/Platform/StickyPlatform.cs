using Controllers.Player;
using UnityEngine;

namespace Controllers.Platform
{
    public class StickyPlatform : BasePlatform
    {
        [Header("Sticky Settings")]
        [SerializeField] private float playerSlowdownFactor = 0.7f;
        [SerializeField] private float jumpForceReductionFactor = 0.7f;
        
        public override PlatformType GetPlatformType() => PlatformType.Sticky;
        
        protected override void OnPlayerStaying(PlayerController player)
        {
            PlayerOnPlatform = player;
            player.SetHorizontalSpeedMultiplier(playerSlowdownFactor);
            player.SetJumpForceMultiplier(jumpForceReductionFactor);
        }
        
        protected override void OnPlayerLeft(PlayerController player)
        {
            if (!PlayerOnPlatform) return;
            PlayerOnPlatform.ResetMultipliers();
            PlayerOnPlatform = null;
        }
    }
}