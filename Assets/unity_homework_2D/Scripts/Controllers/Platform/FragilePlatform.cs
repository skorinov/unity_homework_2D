using Controllers.Player;
using Managers;
using UnityEngine;

namespace Controllers.Platform
{
    public class FragilePlatform : BasePlatform
    {
        [Header("Fragile Settings")]
        [SerializeField] private int hitsToBreak = 2;
        
        private int _currentHits;
        
        public override PlatformType GetPlatformType() => PlatformType.Fragile;
        
        public override void ResetPlatform()
        {
            base.ResetPlatform();
            _currentHits = 0;
        }
        
        protected override void OnPlayerLanded(PlayerController player)
        {
            _currentHits++;

            if (_currentHits >= hitsToBreak)
            {
                AudioManager.Instance?.PlayPlatformBreakSound(); // Play break sound
                BreakPlatform();
            }
        }
    }
}