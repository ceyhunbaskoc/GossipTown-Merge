using Core.PoolSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.ItemDetail
{
    public class SpawnerDetailSlotView : MonoBehaviour, IPoolable
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private GameObject _lockedOverlay;
        [SerializeField] private TextMeshProUGUI _levelText;
        
        public void Setup(Sprite icon, int level, bool isUnlocked)
        {
            _iconImage.sprite = icon;
            _levelText.text = level.ToString();

            _lockedOverlay.SetActive(!isUnlocked);
        }
        
        public void OnSpawned() {}

        public void OnDespawned()
        {
            _iconImage.sprite = null;
        }
    }
}