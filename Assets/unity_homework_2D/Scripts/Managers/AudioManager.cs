using UnityEngine;
using Utilities;

namespace Managers
{
    public class AudioManager : Singleton<AudioManager>
    {
        [Header("Background Music")]
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField] private float musicVolume = 0.5f;

        [Header("Sound Effects")]
        [SerializeField] private AudioClip jumpSound;
        [SerializeField] private AudioClip bounceSound;
        [SerializeField] private AudioClip platformBreakSound;
        [SerializeField] private AudioClip gameOverSound;
        [SerializeField] private AudioClip coinSound;
        [SerializeField] private float sfxVolume = 0.7f;

        private AudioSource _musicSource;
        private AudioSource _sfxSource;

        protected override void OnSingletonAwake()
        {
            InitializeAudioSources();
        }

        private void Start() => PlayBackgroundMusic();

        private void InitializeAudioSources()
        {
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.clip = backgroundMusic;
            _musicSource.volume = musicVolume;
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.volume = sfxVolume;
            _sfxSource.playOnAwake = false;
        }

        public void PlayBackgroundMusic()
        {
            if (_musicSource && backgroundMusic && !_musicSource.isPlaying)
                _musicSource.Play();
        }

        public void StopBackgroundMusic()
        {
            if (_musicSource && _musicSource.isPlaying)
                _musicSource.Stop();
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (_musicSource) _musicSource.volume = musicVolume;
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            if (_sfxSource) _sfxSource.volume = sfxVolume;
        }

        public void PlayJumpSound() => PlaySFX(jumpSound);
        public void PlayBounceSound() => PlaySFX(bounceSound);
        public void PlayPlatformBreakSound() => PlaySFX(platformBreakSound);
        public void PlayGameOverSound() => PlaySFX(gameOverSound);
        public void PlayCoinSound() => PlaySFX(coinSound);

        private void PlaySFX(AudioClip clip)
        {
            if (_sfxSource && clip)
                _sfxSource.PlayOneShot(clip);
        }
    }
}