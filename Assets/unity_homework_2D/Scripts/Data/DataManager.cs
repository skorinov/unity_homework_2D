using Constants;
using UnityEngine;
using Utilities;

namespace Data
{
    public class DataManager : Singleton<DataManager>
    {
        private GameData _gameData;
        
        public GameData GameData => _gameData;
        
        protected override void OnSingletonAwake()
        {
            LoadData();
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) SaveData();
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) SaveData();
        }
        
        public void AddCoin()
        {
            _gameData.AddCoin();
            SaveData();
        }
        
        public void UpdateHeight(float height) => _gameData.UpdateHeight(height);
        public void StartNewSession() => _gameData.ResetSession();
        
        public void SaveData()
        {
            string json = JsonUtility.ToJson(_gameData);
            PlayerPrefs.SetString(GameConstants.SAVE_KEY, json);
            PlayerPrefs.Save();
        }
        
        private void LoadData()
        {
            string json = PlayerPrefs.GetString(GameConstants.SAVE_KEY, "");
            
            _gameData = string.IsNullOrEmpty(json) ? 
                new GameData() : 
                JsonUtility.FromJson<GameData>(json);
        }
    }
}