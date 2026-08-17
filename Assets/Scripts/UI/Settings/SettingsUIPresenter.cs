using Core.Settings;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.Settings
{
    public class SettingsUIPresenter : MonoBehaviour
    {
        [SerializeField] private GameObject _panelView;
        [SerializeField] private Button _panelOpenButton;
        [Header("Music UI")]
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Toggle _musicToggle;

        [Header("SFX UI")]
        [SerializeField] private Slider _sfxSlider;
        [SerializeField] private Toggle _sfxToggle;

        [Header("Haptic UI")]
        [SerializeField] private Toggle _hapticToggle;
        
        [Header("URL's")]
        [SerializeField] private Button _privacyPolicyButton;
        [SerializeField] private string _privacyPolicyUrl;
        [SerializeField] private Button _termsOfServiceButton;
        [SerializeField] private string _termsOfServiceUrl;
        [Space]
        [SerializeField] private Button _closeButton;
        [SerializeField] private UIPopupAnimator _popupAnimator;
        

        private ISettingsService _settingsService;

        public void Initialize(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            
            SyncUIWithData();
            SubscribeEvents();
        }

        private void SyncUIWithData()
        {
            var data = _settingsService.CurrentData;
            
            _musicSlider.value = data.MusicVolume;
            _sfxSlider.value = data.SfxVolume;

            _musicToggle.SetIsOnWithoutNotify(data.IsMusicOn);
            _sfxToggle.SetIsOnWithoutNotify(data.IsSfxOn);
            _hapticToggle.SetIsOnWithoutNotify(data.IsHapticOn);
        }

        private void SubscribeEvents()
        {
            _musicSlider.onValueChanged.AddListener(_settingsService.SetMusicVolume);
            _sfxSlider.onValueChanged.AddListener(_settingsService.SetSfxVolume);
            
            _musicToggle.onValueChanged.AddListener(_settingsService.ToggleMusic);
            _sfxToggle.onValueChanged.AddListener(_settingsService.ToggleSfx);
            _hapticToggle.onValueChanged.AddListener(_settingsService.ToggleHaptic);
            
            _closeButton.onClick.AddListener(HandleCloseRequested);
            _privacyPolicyButton.onClick.AddListener(_openPrivacyPolicy);
            _termsOfServiceButton.onClick.AddListener(_openTermsOfService);
            _panelOpenButton.onClick.AddListener(_openPanel);
        }

        private void HandleCloseRequested()
        {
            _popupAnimator.Hide(() =>
            {
                _panelView.SetActive(false);
            });
            _settingsService.ApplyAndSaveSettings();
        }

        private void _openPanel()
        {
            _panelView.SetActive(true);
            _popupAnimator.Show();
        }

        private void _openPrivacyPolicy()
        {
            Application.OpenURL(_privacyPolicyUrl);
        }

        private void _openTermsOfService()
        {
            Application.OpenURL(_termsOfServiceUrl);
        }

        private void OnDestroy()
        {
            if (_musicSlider != null) _musicSlider.onValueChanged.RemoveAllListeners();
            if (_sfxSlider != null) _sfxSlider.onValueChanged.RemoveAllListeners();
            if (_musicToggle != null) _musicToggle.onValueChanged.RemoveAllListeners();
            if (_sfxToggle != null) _sfxToggle.onValueChanged.RemoveAllListeners();
            if (_hapticToggle != null) _hapticToggle.onValueChanged.RemoveAllListeners();
            if(_closeButton!=null) _closeButton.onClick.RemoveAllListeners();
            if(_privacyPolicyButton!=null) _privacyPolicyButton.onClick.RemoveAllListeners();
            if(_termsOfServiceButton!=null) _termsOfServiceButton.onClick.RemoveAllListeners();
            if(_panelOpenButton!=null) _panelOpenButton.onClick.RemoveAllListeners();
        }
    }
}