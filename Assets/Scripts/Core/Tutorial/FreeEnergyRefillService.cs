using System;
using Core.Economy;
using Core.Economy.Offers;
using UI.Tutorial;

namespace Core.Tutorial
{
    public class FreeEnergyRefillService : IDisposable
    {
        private PlayerEconomyModel _economyModel;
        private FreeEnergyRefillPanelPresenter _freeEnergyRefillPanelPresenter;
        private IFirstTimeOfferService _firstTimeOfferService;
        private int _freeEnergyRefillAmount;

        public FreeEnergyRefillService(PlayerEconomyModel economyModel,
            FreeEnergyRefillPanelPresenter freeEnergyRefillPanelPresenter,
            IFirstTimeOfferService firstTimeOfferService,
            int freeEnergyRefillAmount = 20)
        {
            _economyModel = economyModel;
            _freeEnergyRefillPanelPresenter = freeEnergyRefillPanelPresenter;
            _firstTimeOfferService = firstTimeOfferService;
            _freeEnergyRefillAmount = freeEnergyRefillAmount;

            _economyModel.OnEnergyChanged += _onEnergyChanged;
        }

        private void _onEnergyChanged(int amount)
        {
            if (_firstTimeOfferService.IsFreeEnergyOfferAvailable() && amount <= 0)
            {
                _freeEnergyRefillPanelPresenter.OpenPanel(_applyEnergyRefill);
            }
        }

        private void _applyEnergyRefill()
        {
            _economyModel.AddEnergy(_freeEnergyRefillAmount);
            _firstTimeOfferService.MarkFreeEnergyOfferClaimed();
            _freeEnergyRefillPanelPresenter.ClosePanel();
        }


        public void Dispose()
        {
            _economyModel.OnEnergyChanged -= _onEnergyChanged;
        }
    }
}