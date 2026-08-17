using System;
using Core.SaveSystem;
using UnityEngine;
using UnityEngine.Audio;

namespace Core.Settings
{
    public interface ISettingsService
    {
        SettingsSaveData CurrentData { get; }
        void SetMusicVolume(float volume);
        void SetSfxVolume(float volume);
        void ToggleMusic(bool isOn);
        void ToggleSfx(bool isOn);
        void ToggleHaptic(bool isOn);
        SettingsSaveData GetSaveData();
        event Action OnSettingsChanged;
        void ApplyAndSaveSettings();
    }

    public class SettingsService : ISettingsService
    {
        private readonly AudioMixer _audioMixer;
        public SettingsSaveData CurrentData { get; private set; }
        public event Action OnSettingsChanged;

        private const string MUSIC_VOL_PARAM = "MusicVolume";
        private const string SFX_VOL_PARAM = "SfxVolume";

        public SettingsService(AudioMixer audioMixer, SettingsSaveData settingsSaveData)
        {
            _audioMixer = audioMixer;
            CurrentData = settingsSaveData ?? new SettingsSaveData(); 
            ApplyAllSettings();
        }


        public SettingsSaveData GetSaveData()
        {
            SettingsSaveData saveData = new SettingsSaveData
            {
                MusicVolume = CurrentData.MusicVolume,
                IsHapticOn = CurrentData.IsHapticOn,
                IsMusicOn = CurrentData.IsMusicOn,
                IsSfxOn = CurrentData.IsSfxOn,
                SfxVolume = CurrentData.SfxVolume
            };
            return saveData;
        }

        public void SetMusicVolume(float volume)
        {
            CurrentData.MusicVolume = Mathf.Clamp(volume, 0.0001f, 1f);
            if (CurrentData.IsMusicOn)
            {
                _audioMixer.SetFloat(MUSIC_VOL_PARAM, Mathf.Log10(CurrentData.MusicVolume) * 20f);
            }
        }

        public void ToggleMusic(bool isOn)
        {
            CurrentData.IsMusicOn = isOn;
            float targetVol = isOn ? Mathf.Log10(CurrentData.MusicVolume) * 20f : -80f;
            _audioMixer.SetFloat(MUSIC_VOL_PARAM, targetVol);
        }

        public void SetSfxVolume(float volume)
        {
            CurrentData.SfxVolume = Mathf.Clamp(volume, 0.0001f, 1f);
            if (CurrentData.IsSfxOn)
            {
                _audioMixer.SetFloat(SFX_VOL_PARAM, Mathf.Log10(CurrentData.SfxVolume) * 20f);
            }
        }

        public void ToggleSfx(bool isOn)
        {
            CurrentData.IsSfxOn = isOn;
            float targetVol = isOn ? Mathf.Log10(CurrentData.SfxVolume) * 20f : -80f;
            _audioMixer.SetFloat(SFX_VOL_PARAM, targetVol);
        }

        public void ToggleHaptic(bool isOn)
        {
            CurrentData.IsHapticOn = isOn;
            // Haptic motoruna (Örn: NiceVibrations veya yerleşik API) durumu bildir.
            // HapticManager.SetEnabled(isOn);
        }

        public void ApplyAndSaveSettings()
        {
            OnSettingsChanged?.Invoke();
        }

        private void ApplyAllSettings()
        {
            ToggleMusic(CurrentData.IsMusicOn);
            ToggleSfx(CurrentData.IsSfxOn);
            SetMusicVolume(CurrentData.MusicVolume);
            SetSfxVolume(CurrentData.SfxVolume);
            ToggleHaptic(CurrentData.IsHapticOn);
        }
    }
}