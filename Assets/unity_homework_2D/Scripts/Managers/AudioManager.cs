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
        private AudioSource _uiSource;

        protected override void OnSingletonAwake()
        {
            CreateAudioSources();
        }

        private void Start() => PlayMenuMusic();

        private void CreateAudioSources()
        {
            _menuMusicSource = CreateAudioSource(menuMusic, GameConstants.DEFAULT_MUSIC_VOLUME, true);
            _backgroundMusicSource = CreateAudioSource(backgroundMusic, GameConstants.DEFAULT_MUSIC_VOLUME, true);
            _sfxSource = CreateAudioSource(null, GameConstants.DEFAULT_SFX_VOLUME, false);
            _uiSource = CreateAudioSource(null, GameConstants.DEFAULT_UI_VOLUME, false);
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
            volume = Mathf.Clamp01(volume);
            if (_menuMusicSource) _menuMusicSource.volume = volume;
            if (_backgroundMusicSource) _backgroundMusicSource.volume = volume;
        }

        public void SetSFXVolume(float volume)
        {
            if (_sfxSource) _sfxSource.volume = Mathf.Clamp01(volume);
        }
        
        public void SetUIVolume(float volume)
        {
            if (_uiSource) _uiSource.volume = Mathf.Clamp01(volume);
        }

        public void PlayJumpSound() => PlaySFX(jumpSound);
        public void PlayGameOverSound() => PlaySFX(gameOverSound);
        public void PlayCoinSound() => PlaySFX(coinSound);
        public void PlayButtonHover() => PlayUISound(buttonHoverSound);
        public void PlayButtonClick() => PlayUISound(buttonClickSound);
        
        public void PlayUISound(AudioClip clip, float volumeMultiplier = 1f)
        {
            if (_uiSource && clip)
                _uiSource.PlayOneShot(clip, _uiSource.volume * volumeMultiplier);
        }

        private void PlaySFX(AudioClip clip)
        {
            if (_sfxSource && clip)
                _sfxSource.PlayOneShot(clip);
        }
    }
}