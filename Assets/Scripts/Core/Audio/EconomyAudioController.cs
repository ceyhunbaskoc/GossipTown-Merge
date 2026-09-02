using System;
using Core.Audio;
using Core.Economy;
using Data.Audio;

namespace Core.Audio
{
    public class EconomyAudioController : IDisposable
    {
        private readonly IReadOnlyEconomyModel _economyModel;
        private readonly IAudioService _audioService;

        public EconomyAudioController(IReadOnlyEconomyModel economyModel, IAudioService audioService)
        {
            _economyModel = economyModel;
            _audioService = audioService;

            _economyModel.OnGoldChanged += HandleGoldChanged;
            _economyModel.OnGemChanged += HandleGemChanged;
            _economyModel.OnEnergyChanged += HandleEnergyChanged;
        }

        private void HandleGoldChanged(int currentGold)
        {
            _audioService.PlaySFX(SfxId.Reward_CoinCollect);
        }

        private void HandleGemChanged(int currentGem)
        {
            _audioService.PlaySFX(SfxId.Reward_GemCollect);
        }

        private void HandleEnergyChanged(int remainingEnergy)
        {
            _audioService.PlaySFX(SfxId.Reward_EnergyCollect); 
        }

        public void Dispose()
        {
            _economyModel.OnGoldChanged -= HandleGoldChanged;
            _economyModel.OnGemChanged -= HandleGemChanged;
            _economyModel.OnEnergyChanged -= HandleEnergyChanged;
        }
    }
}