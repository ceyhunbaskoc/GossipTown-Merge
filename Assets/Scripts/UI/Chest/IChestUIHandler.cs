using System;
using Core.GridSystem;

namespace UI.Chest
{
    public interface IChestUIHandler
    {
        void RequestUnlockConfirmation(ChestItemData chestData, Action onConfirmStart);
        void RequestSpeedUpConfirmation(ChestItemData chestData, Action onWatchAd, Action onSpendGems);
    }
}