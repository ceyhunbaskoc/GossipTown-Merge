using System;
using Core.Services;
using Core.Economy;
using TMPro;
using UnityEngine;
using System.Collections;

namespace Core.Views
{
    public class EnergyTimerHUDView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private GameObject _timerContainer;

        private EnergyRegenerationService _energyRegenService;
        private IReadOnlyEconomyModel _economyModel;
        private Coroutine _updateRoutine;

        public void Initialize(EnergyRegenerationService energyRegenService, IReadOnlyEconomyModel economyModel)
        {
            _energyRegenService = energyRegenService;
            _economyModel = economyModel;

            _economyModel.OnEnergyChanged += HandleEnergyChanged;
            
            EvaluateUIState();
        }

        private void HandleEnergyChanged(int currentEnergy)
        {
            EvaluateUIState();
        }

        private void EvaluateUIState()
        {
            if (!_economyModel.CanEnergyRegeneration)
            {
                _timerContainer.SetActive(false);
                if (_updateRoutine != null)
                {
                    StopCoroutine(_updateRoutine);
                    _updateRoutine = null;
                }
            }
            else
            {
                _timerContainer.SetActive(true);
                if (_updateRoutine == null)
                {
                    _updateRoutine = StartCoroutine(UpdateTimerRoutine());
                }
            }
        }

        private IEnumerator UpdateTimerRoutine()
        {
            var wait = new WaitForSeconds(1f);

            while (true)
            {
                int remainingSeconds = _energyRegenService.GetRemainingSeconds();
                
                TimeSpan time = TimeSpan.FromSeconds(remainingSeconds);
                _timerText.text = time.ToString(@"mm\:ss");

                yield return wait;
            }
        }

        private void OnDestroy()
        {
            if (_economyModel != null)
            {
                _economyModel.OnEnergyChanged -= HandleEnergyChanged;
            }
        }
    }
}