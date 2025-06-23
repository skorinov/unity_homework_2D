using UnityEngine;

namespace Data
{
    [System.Serializable]
    public class GameData
    {
        public int sessionCoins;
        public float sessionHeight;
        public int totalCoins;
        public float maxHeight;
        
        public void ResetSession()
        {
            sessionCoins = 0;
            sessionHeight = 0f;
        }
        
        public void AddCoin()
        {
            sessionCoins++;
            totalCoins++;
        }
        
        public void UpdateHeight(float height)
        {
            sessionHeight = height;
            if (height > maxHeight)
                maxHeight = height;
        }
    }
}