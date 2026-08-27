using System;
using Core.SaveSystem;
using Core.Tutorial;
using Data.Tutorial;

namespace Core.Economy.Offers
{
    public interface IFirstTimeOfferService
    {
        bool IsFreeEnergyOfferAvailable();
        void MarkFreeEnergyOfferClaimed();
        bool IsFreeSpawnerOfferAvailable();
        void MarkFreeSpawnerOfferClaimed();
    }

    public class FirstTimeOfferService : IFirstTimeOfferService
    {
        private readonly TutorialSaveData _tutorialSaveData;
        private readonly TutorialOrchestrator _tutorialOrchestrator;
        private readonly Action _requestSaveCallback;

        public FirstTimeOfferService(
            TutorialSaveData tutorialSaveData, 
            TutorialOrchestrator tutorialOrchestrator,
            Action requestSaveCallback)
        {
            _tutorialSaveData = tutorialSaveData;
            _tutorialOrchestrator = tutorialOrchestrator;
            _requestSaveCallback = requestSaveCallback;
        }

        public bool IsFreeEnergyOfferAvailable()
        {
            return _tutorialOrchestrator.IsCompleted && !_tutorialSaveData.HasClaimedFreeEnergyRefill;
        }

        public void MarkFreeEnergyOfferClaimed()
        {
            _tutorialSaveData.HasClaimedFreeEnergyRefill = true;
            _requestSaveCallback?.Invoke();
        }

        public bool IsFreeSpawnerOfferAvailable()
        {
            return _tutorialOrchestrator.IsCompleted && !_tutorialSaveData.HasClaimedFreeSpawnerRefill;
        }

        public void MarkFreeSpawnerOfferClaimed()
        {
            _tutorialSaveData.HasClaimedFreeSpawnerRefill = true;
            _requestSaveCallback?.Invoke();
        }
    }
}