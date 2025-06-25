using System.Collections.Generic;
using UnityEngine;

namespace Controllers.Platform
{
    public class PlatformContext
    {
        public BasePlatform platform;
        public Vector3 worldPosition;
        public float playerHeight;
        public int levelIndex;
        
        // Separate lists for different types of proximity
        public List<BasePlatform> horizontallyNearbyPlatforms = new();  // Close by X (same vertical column)
        public List<BasePlatform> verticallyNearbyPlatforms = new();    // Close by Y (same horizontal level)
        public List<BasePlatform> allNearbyPlatforms = new();           // All nearby platforms (legacy support)
        
        // Legacy property for backward compatibility
        public List<BasePlatform> nearbyPlatforms => allNearbyPlatforms;
        
        // Utility methods for common checks
        public bool IsHorizontallyIsolated => horizontallyNearbyPlatforms.Count == 0;
        public bool IsVerticallyIsolated => verticallyNearbyPlatforms.Count == 0;
        public bool IsCompletelyIsolated => allNearbyPlatforms.Count == 0;
        
        public int HorizontalNeighborCount => horizontallyNearbyPlatforms.Count;
        public int VerticalNeighborCount => verticallyNearbyPlatforms.Count;
        public int TotalNeighborCount => allNearbyPlatforms.Count;
        
        public bool HasHorizontalNeighbors => horizontallyNearbyPlatforms.Count > 0;
        public bool HasVerticalNeighbors => verticallyNearbyPlatforms.Count > 0;
        
        public float GetDistanceToNearestHorizontalPlatform()
        {
            if (horizontallyNearbyPlatforms.Count == 0) return float.MaxValue;
            
            float minDistance = float.MaxValue;
            foreach (var nearby in horizontallyNearbyPlatforms)
            {
                float distance = Mathf.Abs(worldPosition.x - nearby.transform.position.x);
                if (distance < minDistance)
                    minDistance = distance;
            }
            return minDistance;
        }
        
        public float GetDistanceToNearestVerticalPlatform()
        {
            if (verticallyNearbyPlatforms.Count == 0) return float.MaxValue;
            
            float minDistance = float.MaxValue;
            foreach (var nearby in verticallyNearbyPlatforms)
            {
                float distance = Mathf.Abs(worldPosition.y - nearby.transform.position.y);
                if (distance < minDistance)
                    minDistance = distance;
            }
            return minDistance;
        }
    }
}