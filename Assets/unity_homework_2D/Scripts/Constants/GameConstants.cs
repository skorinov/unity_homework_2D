namespace Constants
{
    public static class GameConstants
    {
        // Tags
        public const string PLAYER_TAG = "Player";
        
        // Save Keys
        public const string SAVE_KEY = "GameData";
        
        // Input Keys
        public const string HORIZONTAL_INPUT = "Horizontal";
        
        // Physics
        public const float COYOTE_TIME_WINDOW = 0.2f;
        public const float LANDING_VELOCITY_THRESHOLD = 0.1f;
        
        // Screen
        public const float SCREEN_MARGIN = 0.3f;
        public const float SCREEN_DEATH_MARGIN = 0.5f;
        
        // World Positions
        public const float MENU_AREA_X = -50f;
        public const float GAME_AREA_X = 0f;
        
        // Camera
        public const float CAMERA_SIZE = 5f;
        
        // Platform Generation
        public const float PLATFORM_SEARCH_RANGE = 2f;
        public const float PLATFORM_SPACING = 0.5f;
        public const float VERTICAL_CHECK_RANGE = 3f;
        public const float DEFAULT_PLATFORM_WIDTH = 2f;
        public const float FIRST_PLATFORM_POSITION = -1f;
        public const float HALF_WIDTH_MULTIPLIER = 0.5f;
        public const int MAX_PLACEMENT_ATTEMPTS = 10;
        
        // Animation
        public const int ANIMATION_UPDATE_INTERVAL = 3;
        
        // UI
        public const float UI_FADE_SPEED = 5f;
        public const float UI_TRANSITION_SPEED = 2f;
        
        // Audio
        public const float DEFAULT_MUSIC_VOLUME = 0.5f;
        public const float DEFAULT_SFX_VOLUME = 0.2f;
        public const float DEFAULT_UI_VOLUME = 0.5f;
    }
}