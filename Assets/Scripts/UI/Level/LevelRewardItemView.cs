using Core.PoolSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Level
{
    public class LevelRewardItemView : MonoBehaviour, IPoolable
    {
        [SerializeField] private Image _rewardIcon;
        [SerializeField] private TextMeshProUGUI _rewardText;

        public void Setup(Sprite icon, string displayText)
        {
            _rewardIcon.sprite = icon;
            _rewardText.text = displayText;
            
            gameObject.SetActive(true);
        }

        public void OnSpawned()
        {
            
        }

        public void OnDespawned()
        {
            _rewardIcon.sprite = null;
            _rewardText.text = string.Empty;
        }
    }
}