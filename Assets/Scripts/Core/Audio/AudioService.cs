using Data.Audio;
using UnityEngine;
using UnityEngine.Audio;

namespace Core.Audio
{
    public class AudioService : IAudioService
    {
        private readonly SfxDatabaseSO _database;
        
        private readonly AudioSource _bgmSource;
        private readonly AudioSource _sfxSource;
        private readonly AudioClip _backgroundMusic;

        public AudioService(SfxDatabaseSO database, AudioSource bgmSource, AudioSource sfxSource, AudioClip backgroundMusic)
        {
            _database = database;
            _database.Initialize();

            _backgroundMusic = backgroundMusic;
            _bgmSource = bgmSource;
            _sfxSource = sfxSource;
            PlayBGM(_backgroundMusic);
        }

        public void PlaySFX(SfxId id)
        {
            if (id == SfxId.None) return;

            SfxConfig config = _database.GetSfxConfig(id);
            if (config != null)
            {
                config.Play(_sfxSource);
            }
            else
            {
                Debug.LogWarning($"[AudioService] SFX ID not found: {id}");
            }
        }

        public void PlayBGM(AudioClip bgMusic, float volume = 1f)
        {
            if (bgMusic == null) return;

            if (_bgmSource.isPlaying && _bgmSource.clip == bgMusic) return;

            _bgmSource.clip = bgMusic;
            _bgmSource.volume = volume;
            _bgmSource.Play();
        }

        public void StopBGM()
        {
            if (_bgmSource.isPlaying)
            {
                _bgmSource.Stop();
            }
        }
    }
}