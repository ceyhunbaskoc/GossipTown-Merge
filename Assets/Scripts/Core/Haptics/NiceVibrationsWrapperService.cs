using Core.Settings;
using Lofelt.NiceVibrations;
using UnityEngine;

namespace Core.Haptics
{
    public class NiceVibrationsWrapperService : IHapticService
    {
        private readonly ISettingsService _settingsService;

        public NiceVibrationsWrapperService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            
            HapticController.hapticsEnabled = _settingsService.CurrentData.IsHapticOn;
        }

        public void Play(HapticType type)
        {
            if (!_settingsService.CurrentData.IsHapticOn) return;
            HapticPatterns.PresetType lofeltPreset = MapToLofelt(type);
            HapticPatterns.PlayPreset(lofeltPreset);
        }

        private HapticPatterns.PresetType MapToLofelt(HapticType type)
        {
            return type switch
            {
                HapticType.Selection => HapticPatterns.PresetType.Selection,
                HapticType.LightImpact => HapticPatterns.PresetType.LightImpact,
                HapticType.MediumImpact => HapticPatterns.PresetType.MediumImpact,
                HapticType.HeavyImpact => HapticPatterns.PresetType.HeavyImpact,
                HapticType.Success => HapticPatterns.PresetType.Success,
                HapticType.Warning => HapticPatterns.PresetType.Warning,
                HapticType.SoftImpact => HapticPatterns.PresetType.SoftImpact,
                _ => HapticPatterns.PresetType.LightImpact
            };
        }
    }
}