using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Level
{
    public class LevelHUDView : MonoBehaviour
    {
        [SerializeField] private Slider _experienceSlider;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private Button _openWindowButton;

        public event Action OnOpenWindowClicked;

        private void Awake()
        {
            _openWindowButton.onClick.AddListener(() => OnOpenWindowClicked?.Invoke());
        }

        public void UpdateHUD(int currentLevel, int currentExp, int requiredExp)
        {
            _levelText.text = currentLevel.ToString();
            
            if (requiredExp > 0)
            {
                _experienceSlider.value = (float)currentExp / requiredExp;
            }
            else
            {
                _experienceSlider.value = 1f;
            }
        }
    }
}