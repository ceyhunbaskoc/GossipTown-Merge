using TMPro;
using UnityEngine;

namespace UI.Achievements
{
    public class AchievementCategoryView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Transform _slotsContainer;
        [SerializeField] private AchievementSlotView _slotPrefab;

        public void SetTitle(string title)
        {
            _titleText.text = title;
        }

        public AchievementSlotView CreateSlot()
        {
            //TODO: Convert to object pooling
            return Instantiate(_slotPrefab, _slotsContainer);
        }
    }
}