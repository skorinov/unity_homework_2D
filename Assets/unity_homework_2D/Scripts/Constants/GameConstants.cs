namespace Constants
{
    public static class GameConstants
    {
        // Common
        public const float HALF_WIDTH_MULTIPLIER = 0.5f;
        public const int DIRECTION_LEFT = -1;
        public const int DIRECTION_RIGHT = 1;
        public const float DEFAULT_MULTIPLIER = 1f;
        
        // Tags
        public const string PLAYER_TAG = "Player";
        
        // PlayerPrefs Keys
        public const string SAVE_KEY = "RubysRiseGameData";
        public const string MUSIC_VOLUME_KEY = "MusicVolume";
        public const string SFX_VOLUME_KEY = "SFXVolume";
        public const string JUMP_KEY = "JumpKey";
        public const string DROP_KEY = "DropKey";
        
        // Input
        public const string HORIZONTAL_INPUT = "Horizontal";
        public const float INPUT_DEADZONE_THRESHOLD = 0.1f;
        
        // Physics
        public const float COYOTE_TIME_WINDOW = 0.2f;
        public const float LANDING_VELOCITY_THRESHOLD = 0.1f;
        
        // Screen
        public const float SCREEN_MARGIN = 0.3f;
        
        // World Positions
        public const float MENU_AREA_X = -50f;
        public const float GAME_AREA_X = 0f;
        
        // Camera
        public const float CAMERA_SIZE = 5f;
        public const float ORTHOGRAPHIC_SIZE_TO_FULL_SIZE_MULTIPLIER = 2f;
        public const float CAMERA_TRANSITION_BASE_DURATION = 1f;
        
        // Player Movement
        public const float DROP_THROUGH_VELOCITY = -5f;
        
        // Player Damage Visual Effects
        public const float BLINK_TRANSPARENT_ALPHA = 0.3f;
        public const float BLINK_OPAQUE_ALPHA = 1f;
        public const float DAMAGE_COLOR_FLASH_DURATION = 0.1f;
        
        // Platform Generation
        public const float DEFAULT_PLATFORM_WIDTH = 2f;
        public const float PLATFORM_GENERATION_MARGIN = 1f;
        public const int MAX_PLACEMENT_ATTEMPTS = 10;
        
        // Platform
        public const float DEFAULT_PLATFORM_HALF_WIDTH = 1f;
        public const float SAME_HORIZONTAL_LEVEL_TOLERANCE = 0.5f;
        public const float DEFAULT_PLATFORM_HALF_WIDTH_FALLBACK = 0.5f;
        public const float MIN_MOVEMENT_RANGE = 1f;
        public const float MIN_MOVEMENT_HALF_RANGE = 0.5f;
        public const float DROP_THROUGH_COLLISION_DELAY = 0.2f;
        
        // Enemy
        public const float MIN_PUSH_UPWARD_FORCE = 0.3f;
        public const float DEFAULT_ENEMY_RADIUS = 0.5f;
        
        // Animation
        public const int ANIMATION_UPDATE_INTERVAL = 3;
        
        // UI Text
        public const string START_GAME_TEXT = "Start Game";
        public const string NEW_GAME_TEXT = "New Game";
        public const string PRESS_KEY_TEXT = "Press Key...";
        
        // UI Scroll
        public const float SCROLL_TOP_POSITION = 1f;
        
        // UI Settings
        public const float VOLUME_TO_PERCENTAGE_MULTIPLIER = 100f;
        
        // UI Animation
        public const float SCROLL_ANIMATION_DURATION = 0.3f;
        
        // Audio
        public const float DEFAULT_MUSIC_VOLUME = 1f;
        public const float DEFAULT_SFX_VOLUME = 1f;
    }
}