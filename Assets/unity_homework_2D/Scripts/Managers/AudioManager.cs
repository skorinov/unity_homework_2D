using Constants;
using UnityEngine;
using Utilities;

namespace Managers
{
    public class AudioManager : Singleton<AudioManager>
    {
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField] private AudioClip jumpSound;
        [SerializeField] private AudioClip gameOverSound;
        [SerializeField] private AudioClip coinSound;
        [SerializeField] private AudioClip buttonHoverSound;
        [SerializeField] private AudioClip buttonClickSound;

        private AudioSource _menuMusicSource;
        private AudioSource _backgroundMusicSource;
        private AudioSource _sfxSource;

        // Store current volume values
        private float _currentMusicVolume = GameConstants.DEFAULT_MUSIC_VOLUME;
        private float _currentSFXVolume = GameConstants.DEFAULT_SFX_VOLUME;

        protected override void OnSingletonAwake()
        {
            LoadVolumeSettings();
            CreateAudioSources();
        }

        private void Start() => PlayMenuMusic();

        private void CreateAudioSources()
        {
            _menuMusicSource = CreateAudioSource(menuMusic, _currentMusicVolume, true);
            _backgroundMusicSource = CreateAudioSource(backgroundMusic, _currentMusicVolume, true);
            _sfxSource = CreateAudioSource(null, _currentSFXVolume, false);
            // UI source больше не нужен, убираем его
        }

        private AudioSource CreateAudioSource(AudioClip clip, float volume, bool loop)
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = volume;
            source.loop = loop;
            source.playOnAwake = false;
            return source;
        }

        public void PlayMenuMusic()
        {
            StopBackgroundMusic();
            if (_menuMusicSource && menuMusic && !_menuMusicSource.isPlaying)
                _menuMusicSource.Play();
        }

        public void PlayBackgroundMusic()
        {
            StopMenuMusic();
            if (_backgroundMusicSource && backgroundMusic && !_backgroundMusicSource.isPlaying)
                _backgroundMusicSource.Play();
        }

        public void StopMenuMusic()
        {
            if (_menuMusicSource && _menuMusicSource.isPlaying)
                _menuMusicSource.Stop();
        }

        public void StopBackgroundMusic()
        {
            if (_backgroundMusicSource && _backgroundMusicSource.isPlaying)
                _backgroundMusicSource.Stop();
        }

        public void StopAllMusic()
        {
            StopMenuMusic();
            StopBackgroundMusic();
        }

        public void SetMusicVolume(float volume)
        {
            _currentMusicVolume = Mathf.Clamp01(volume);
            if (_menuMusicSource) _menuMusicSource.volume = _currentMusicVolume;
            if (_backgroundMusicSource) _backgroundMusicSource.volume = _currentMusicVolume;
            SaveVolumeSettings();
        }

        public void SetSFXVolume(float volume)
        {
            _currentSFXVolume = Mathf.Clamp01(volume);
            if (_sfxSource) _sfxSource.volume = _currentSFXVolume;
            SaveVolumeSettings();
        }

        public void PlayJumpSound() => PlaySFX(jumpSound);
        public void PlayGameOverSound() => PlaySFX(gameOverSound);
        public void PlayCoinSound() => PlaySFX(coinSound);
        public void PlayButtonHover() => PlaySFX(buttonHoverSound);
        public void PlayButtonClick() => PlaySFX(buttonClickSound);
        
        private void PlaySFX(AudioClip clip)
        {
            if (_sfxSource && clip)
                _sfxSource.PlayOneShot(clip);
        }

        // Get current volume values
        public float GetMusicVolume() => _currentMusicVolume;
        public float GetSFXVolume() => _currentSFXVolume;

        private void LoadVolumeSettings()
        {
            // Проверяем есть ли сохранённые настройки
            bool hasMusicSetting = PlayerPrefs.HasKey("MusicVolume");
            bool hasSFXSetting = PlayerPrefs.HasKey("SFXVolume");
            
            if (hasMusicSetting)
            {
                _currentMusicVolume = PlayerPrefs.GetFloat("MusicVolume");
                // Если сохранённое значение 0, используем дефолт
                if (_currentMusicVolume <= 0f) _currentMusicVolume = GameConstants.DEFAULT_MUSIC_VOLUME;
            }
            else
            {
                _currentMusicVolume = GameConstants.DEFAULT_MUSIC_VOLUME;
            }
            
            if (hasSFXSetting)
            {
                _currentSFXVolume = PlayerPrefs.GetFloat("SFXVolume");
                // Если сохранённое значение 0, используем дефолт
                if (_currentSFXVolume <= 0f) _currentSFXVolume = GameConstants.DEFAULT_SFX_VOLUME;
            }
            else
            {
                _currentSFXVolume = GameConstants.DEFAULT_SFX_VOLUME;
            }
        }

        private void SaveVolumeSettings()
        {
            PlayerPrefs.SetFloat("MusicVolume", _currentMusicVolume);
            PlayerPrefs.SetFloat("SFXVolume", _currentSFXVolume);
            PlayerPrefs.Save();
        }
    }
}