using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Level
{
    public class LevelRewardRowView : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _levelTitleText;
        [SerializeField] private Transform _rewardsContainer;
        
        [Header("State Visuals (Optional)")]
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Color _lockedColor = Color.gray;
        [SerializeField] private Color _completedColor = Color.green;
        [SerializeField] private Color _currentColor = Color.yellow;

        public Transform RewardsContainer => _rewardsContainer;

        public void SetLevelInfo(int level, bool isCompleted, bool isCurrent, bool isNoRewards = false)
        {
            _levelTitleText.text = $"Level {level}";
            
            if (isNoRewards)
            {
                _backgroundImage.gameObject.SetActive(false);
                return;
            }

            _backgroundImage.gameObject.SetActive(true);

            if (isCurrent) 
                _backgroundImage.color = _currentColor;
            else if (isCompleted) 
                _backgroundImage.color = _completedColor;
            else 
                _backgroundImage.color = _lockedColor;
        }
    }
}