using Controllers.Player;
using UnityEngine;

namespace Controllers.Platform.Actions
{
    [CreateAssetMenu(fileName = "StickyAction", menuName = "Platform Actions/Sticky Action")]
    public class StickyAction : PlatformAction
    {
        [Header("Sticky Settings")]
        [SerializeField, Range(0.1f, 1f)] private float playerSlowdownFactor = 0.7f;
        [SerializeField, Range(0.1f, 1f)] private float jumpForceReductionFactor = 0.7f;

        public override void OnPlayerStaying(PlayerController player, BasePlatform platform)
        {
            player.SetHorizontalSpeedMultiplier(playerSlowdownFactor);
            player.SetJumpForceMultiplier(jumpForceReductionFactor);
        }

        public override void OnPlayerLeft(PlayerController player, BasePlatform platform)
        {
            player.ResetMultipliers();
        }
    }
}