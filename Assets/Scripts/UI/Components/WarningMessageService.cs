using Core.Haptics;
using Core.Services;
using Data.UI;
using UnityEngine;

namespace UI.Components
{
    public interface IWarningMessageService
    {
        void ShowWarning(string message, Vector2 screenPosition, InteractionResult result = InteractionResult.None);
    }

    public class WarningMessageService : IWarningMessageService
    {
        private readonly SingleWarningTextView _warningView;
        private readonly FloatingTextConfigSO _warningConfig;
        private readonly IHapticService _hapticService;

        public WarningMessageService(SingleWarningTextView warningView, FloatingTextConfigSO warningConfig, IHapticService hapticService)
        {
            _warningView = warningView;
            _warningConfig = warningConfig;
            _hapticService = hapticService;
            
            _warningView.Initialize();
        }

        public void ShowWarning(string message, Vector2 screenPosition, InteractionResult result = InteractionResult.None)
        {
            _warningView.PlayWarning(message, screenPosition, _warningConfig, result);
            _hapticService.Play(HapticType.Warning);
        }
    }
}