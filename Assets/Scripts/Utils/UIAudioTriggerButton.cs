using UnityEngine;
using UnityEngine.UI;
using Core.Audio;
using Core.Architecture;
using Data.Audio;

namespace Utils
{
    [RequireComponent(typeof(Button))]
    public class UIAudioTrigger : MonoBehaviour
    {
        private Button _button;
        private IAudioService _audioService; 

        private void Awake()
        {
            _button = GetComponent<Button>();
            _audioService = ServiceLocator.Get<IAudioService>();
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(PlaySound);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(PlaySound);
        }

        private void PlaySound()
        {
            _audioService?.PlaySFX(SfxId.UI_ButtonClick);
        }
    }
}