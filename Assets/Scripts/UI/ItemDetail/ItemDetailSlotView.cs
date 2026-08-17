using Core.PoolSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.ItemDetail
{
    public class ItemDetailSlotView : MonoBehaviour, IPoolable
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private GameObject _lockedOverlay;

        [Header("Theme Settings")] 
        [SerializeField] private Color _defaultBgColor;
        [SerializeField] private Color _currentLevelBgColor;
        
        public void Setup(Sprite icon, int level, bool isUnlocked, bool isCurrentLevel)
        {
            _iconImage.sprite = icon;
            _levelText.text = level.ToString();

            if (!isUnlocked)
            {
                _lockedOverlay.SetActive(true);
            }
            else if (isCurrentLevel)
            {
                _backgroundImage.color = _currentLevelBgColor;
            }
            else
            {
                _backgroundImage.color = _defaultBgColor;
            }
        }
        
        
        public void OnSpawned() {}

        public void OnDespawned()
        {
            _iconImage.sprite = null;
            _backgroundImage.color = _defaultBgColor;
        }
    }
}