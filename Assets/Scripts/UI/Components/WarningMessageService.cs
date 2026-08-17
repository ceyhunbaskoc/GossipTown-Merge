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

        public WarningMessageService(SingleWarningTextView warningView, FloatingTextConfigSO warningConfig)
        {
            _warningView = warningView;
            _warningConfig = warningConfig;
            
            _warningView.Initialize();
        }

        public void ShowWarning(string message, Vector2 screenPosition, InteractionResult result = InteractionResult.None)
        {
            _warningView.PlayWarning(message, screenPosition, _warningConfig, result);
        }
    }
}