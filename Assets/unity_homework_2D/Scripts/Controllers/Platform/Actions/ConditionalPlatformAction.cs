using UnityEngine;

namespace Controllers.Platform.Actions
{
    public abstract class ConditionalPlatformAction : PlatformAction
    {
        [SerializeField, TextArea(2, 3)] 
        protected string conditionDescription = "Describe when this action should activate";
        
        protected bool _isActive = false;
        protected PlatformContext _context;
        
        public abstract bool ShouldActivate(PlatformContext context);
        
        public override void OnPlatformReady(BasePlatform platform, Vector3 worldPosition)
        {
            _context = CreateContext(platform, worldPosition);
            _isActive = ShouldActivate(_context);
            
            if (_isActive)
            {
                OnActivated(platform);
            }
        }
        
        public override void OnReset(BasePlatform platform)
        {
            if (_isActive)
            {
                OnDeactivated(platform);
            }
            
            _isActive = false;
            _context = null;
            base.OnReset(platform);
        }
        
        // Override these in derived classes for conditional behavior
        protected virtual void OnActivated(BasePlatform platform) { }
        protected virtual void OnDeactivated(BasePlatform platform) { }
        
        // Only call base methods if action is active
        public override void OnUpdate(BasePlatform platform)
        {
            if (_isActive) OnConditionalUpdate(platform);
        }
        
        public override void OnPlayerLanded(Controllers.Player.PlayerController player, BasePlatform platform)
        {
            if (_isActive) OnConditionalPlayerLanded(player, platform);
        }
        
        public override void OnPlayerStaying(Controllers.Player.PlayerController player, BasePlatform platform)
        {
            if (_isActive) OnConditionalPlayerStaying(player, platform);
        }
        
        public override void OnPlayerLeft(Controllers.Player.PlayerController player, BasePlatform platform)
        {
            if (_isActive) OnConditionalPlayerLeft(player, platform);
        }
        
        // Override these instead of the base methods
        protected virtual void OnConditionalUpdate(BasePlatform platform) { }
        protected virtual void OnConditionalPlayerLanded(Controllers.Player.PlayerController player, BasePlatform platform) { }
        protected virtual void OnConditionalPlayerStaying(Controllers.Player.PlayerController player, BasePlatform platform) { }
        protected virtual void OnConditionalPlayerLeft(Controllers.Player.PlayerController player, BasePlatform platform) { }
        
        private PlatformContext CreateContext(BasePlatform platform, Vector3 worldPosition)
        {
            var context = new PlatformContext
            {
                platform = platform,
                worldPosition = worldPosition,
                playerHeight = Managers.GameManager.Instance ? Managers.GameManager.Instance.GetHighestPlayerY() : 0f,
                levelIndex = Mathf.FloorToInt(worldPosition.y / 10f) // Simple level calculation
            };
            
            // Find platforms and categorize them by proximity type
            var activePlatforms = Pooling.PlatformPool.Instance?.GetActivePlatforms();
            if (activePlatforms != null)
            {
                foreach (var otherPlatform in activePlatforms)
                {
                    if (otherPlatform == platform) continue;
                    
                    Vector3 otherPos = otherPlatform.transform.position;
                    float xDistance = Mathf.Abs(platform.transform.position.x - otherPos.x);
                    float yDistance = Mathf.Abs(platform.transform.position.y - otherPos.y);
                    
                    // Check horizontal proximity (same X column, within reasonable Y range)
                    if (yDistance < 0f)
                    {
                        context.horizontallyNearbyPlatforms.Add(otherPlatform);
                    }
                    
                    // Check vertical proximity (same Y level, within reasonable X range)
                    if (xDistance > 0f)
                    {
                        context.verticallyNearbyPlatforms.Add(otherPlatform);
                    }
                    
                    // Add to general nearby list if reasonably close
                    if (xDistance > 0f && yDistance < 0f)
                    {
                        context.allNearbyPlatforms.Add(otherPlatform);
                    }
                }
            }
            
            return context;
        }
        
        // Debug info for inspector
        public bool IsActive => _isActive;
        public PlatformContext Context => _context;
    }
}