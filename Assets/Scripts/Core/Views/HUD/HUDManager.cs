using System;
using Core.Economy;
using Data.Quests;
using DG.Tweening;
using TMPro;
using UI.FlightSystem;
using UnityEngine;

namespace Core.Views
{
    public class HUDManager : MonoBehaviour, IHUDTargetProvider
    {
        [Header("Energy Elements")]
        [SerializeField] private RectTransform _energyIconTransform;
        [SerializeField] private TextMeshProUGUI _energyText;
        
        [Header("Gem Elements")]
        [SerializeField] private RectTransform _gemIconTransform;
        [SerializeField] private TextMeshProUGUI _gemText;
        
        [Header("Gold Elements")]
        [SerializeField] private RectTransform _goldIconTransform;
        [SerializeField] private TextMeshProUGUI _goldText;
        
        [Header("Animation Settings")]
        [SerializeField] private float _countDuration = 1f;
        [SerializeField] private float _punchScaleAmount = 0.3f;
        [SerializeField] private float _punchDuration = 0.4f;
        
        private IReadOnlyEconomyModel _economyModel;

        private int _currentVisualEnergy;
        private int _currentVisualGem;
        private int _currentVisualGold;

        private Tween _energyCountTween;
        private Tween _gemCountTween;
        private Tween _goldCountTween;

        private Tween _energyIconTween;
        private Tween _gemIconTween;
        private Tween _goldIconTween;

        public void Initialize(IReadOnlyEconomyModel economyModel)
        {
            _economyModel = economyModel;

            if (_economyModel != null)
            {
                SetImmediateVisualValues();

                _economyModel.OnEnergyChanged += UpdateEnergyText;
                _economyModel.OnGemChanged += UpdateGemText;
                _economyModel.OnGoldChanged += UpdateGoldText;
            }
        }

        private void SetImmediateVisualValues()
        {
            _currentVisualEnergy = _economyModel.Energy;
            _energyText.text = _currentVisualEnergy.ToString();

            _currentVisualGem = _economyModel.Gems;
            _gemText.text = _currentVisualGem.ToString();

            _currentVisualGold = _economyModel.Golds;
            _goldText.text = _currentVisualGold.ToString();
        }
        
        public Vector3 GetTargetScreenPosition(RewardCategory category)
        {
            switch (category)
            {
                case RewardCategory.Energy:
                    return _energyIconTransform.position;
                case RewardCategory.Gem:
                    return _gemIconTransform.position;
                case RewardCategory.Gold:
                    return _goldIconTransform.position;
                default:
                    return Vector3.zero;
            }
        }
        
        private void UpdateEnergyText(int targetEnergy)
        {
            if (targetEnergy > _currentVisualEnergy)
            {
                PlayIconFeedbackAnimation(_energyIconTransform, ref _energyIconTween);
            }

            _energyCountTween?.Kill();
            _energyCountTween = DOTween.To(
                () => _currentVisualEnergy, 
                x => 
                {
                    _currentVisualEnergy = x;
                    _energyText.text = _currentVisualEnergy.ToString();
                }, 
                targetEnergy, 
                _countDuration).SetEase(Ease.OutQuad);
        }
        
        private void UpdateGemText(int targetGem)
        {
            if (targetGem > _currentVisualGem)
            {
                PlayIconFeedbackAnimation(_gemIconTransform, ref _gemIconTween);
            }

            _gemCountTween?.Kill();
            _gemCountTween = DOTween.To(
                () => _currentVisualGem, 
                x => 
                {
                    _currentVisualGem = x;
                    _gemText.text = _currentVisualGem.ToString();
                }, 
                targetGem, 
                _countDuration).SetEase(Ease.OutQuad);
        }
        
        private void UpdateGoldText(int targetGold)
        {
            if (targetGold > _currentVisualGold)
            {
                PlayIconFeedbackAnimation(_goldIconTransform, ref _goldIconTween);
            }

            _goldCountTween?.Kill();
            _goldCountTween = DOTween.To(
                () => _currentVisualGold, 
                x => 
                {
                    _currentVisualGold = x;
                    _goldText.text = _currentVisualGold.ToString();
                }, 
                targetGold, 
                _countDuration).SetEase(Ease.OutQuad);
        }
        private void PlayIconFeedbackAnimation(RectTransform iconTransform, ref Tween iconTween)
        {
            iconTween?.Kill();
            iconTransform.localScale = Vector3.one; 
            
            iconTween = iconTransform.DOPunchScale(
                Vector3.one * _punchScaleAmount, 
                _punchDuration, 
                vibrato: 4, 
                elasticity: 0.5f);
        }

        private void OnDestroy()
        {
            if (_economyModel != null)
            {
                _economyModel.OnEnergyChanged -= UpdateEnergyText;
                _economyModel.OnGemChanged -= UpdateGemText;
                _economyModel.OnGoldChanged -= UpdateGoldText;
            }

            _energyCountTween?.Kill();
            _gemCountTween?.Kill();
            _goldCountTween?.Kill();

            _energyIconTween?.Kill();
            _gemIconTween?.Kill();
            _goldIconTween?.Kill();
        }
    }
}