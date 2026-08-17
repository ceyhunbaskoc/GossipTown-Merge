using UnityEngine;
using UnityEngine.UI;

namespace UI.Components
{
    [RequireComponent(typeof(Toggle))]
    public class ToggleVisualPresenter : MonoBehaviour
    {
        [Header("View References")]
        [SerializeField] private Image _targetImage;
        
        [Header("State Assets")]
        [SerializeField] private Sprite _onSprite;
        [SerializeField] private Sprite _offSprite;

        private Toggle _toggle;

        private void Awake()
        {
            _toggle = GetComponent<Toggle>();
        }

        private void OnEnable()
        {
            _toggle.onValueChanged.AddListener(UpdateVisualState);
            
            UpdateVisualState(_toggle.isOn);
        }

        private void OnDisable()
        {
            _toggle.onValueChanged.RemoveListener(UpdateVisualState);
        }

        private void UpdateVisualState(bool isOn)
        {
            if (_targetImage == null) return;
            _targetImage.sprite = isOn ? _onSprite : _offSprite;
        }
    }
}