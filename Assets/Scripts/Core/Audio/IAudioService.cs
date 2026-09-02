using Data.Audio;
using UnityEngine;

namespace Core.Audio
{
    public interface IAudioService
    {
        void PlaySFX(SfxId id);
        void PlayBGM(AudioClip musicClip, float volume = 1f);
        void StopBGM();
    }
}