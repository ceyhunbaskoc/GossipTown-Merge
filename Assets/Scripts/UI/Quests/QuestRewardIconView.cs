using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Quests
{
    public class QuestRewardIconView : MonoBehaviour
    {
        [SerializeField] private Image _rewardIcon;
        [SerializeField] private TextMeshProUGUI _amountText;

        public void Initialize(Sprite icon, string amountText)
        {
            if (_rewardIcon != null && icon != null)
            {
                _rewardIcon.sprite = icon;
            }
            
            if (_amountText != null)
            {
                _amountText.text = amountText;
            }
        }
    }
}