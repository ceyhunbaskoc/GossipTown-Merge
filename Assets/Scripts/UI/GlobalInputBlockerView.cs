using UnityEngine;
using UnityEngine.EventSystems;
using Core.Services;

namespace UI
{
    public class GlobalInputBlockerView : MonoBehaviour
    {
        private IInputLockService _inputLockService;

        public void Initialize(IInputLockService inputLockService)
        {
            _inputLockService = inputLockService;
            _inputLockService.OnUILockStateChanged += HandleUILockStateChanged;
            HandleUILockStateChanged(_inputLockService.IsUILocked);
        }

        private void HandleUILockStateChanged(bool isLocked)
        {
            EventSystem currentEventSystem = EventSystem.current;
            
            if (currentEventSystem == null)
            {
                currentEventSystem = FindObjectOfType<EventSystem>();
            }

            if (currentEventSystem != null)
            {
                currentEventSystem.enabled = !isLocked;
                
                if (isLocked)
                    Debug.Log("[GlobalInputBlockerView] UI KİLİTLENDİ (EventSystem disabled)");
                else
                    Debug.Log("[GlobalInputBlockerView] UI AÇILDI (EventSystem enabled)");
            }
        }

        private void OnDestroy()
        {
            if (_inputLockService != null)
            {
                _inputLockService.OnUILockStateChanged -= HandleUILockStateChanged;
            }
        }
    }
}