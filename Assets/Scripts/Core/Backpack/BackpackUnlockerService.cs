using Core.Economy;
using Core.GridSystem;
using UnityEngine;

namespace Core.Backpack
{
    public class BackpackUnlockerService
    {
        private const int INCREASE_UNLOCK_COST_PER_SLOT = 2;
        private readonly GridDataModel _gridModel;
        private readonly IEconomyModifier _economyModifier;

        public BackpackUnlockerService(IEconomyModifier economyModifier, GridDataModel backpackGridModel)
        {
            _gridModel = backpackGridModel;
            _economyModifier = economyModifier;
        }

        public bool TryUnlockSlot(Vector2Int position, int gemCost)
        {
            if (_gridModel.IsCellLocked(position) && _economyModifier.TrySpendGems(gemCost) && _gridModel.TryUnlockLockedCell(position))
            {
                return true;
            }
            
            Debug.LogWarning("[Backpack] Slot açılamadı! Yetersiz elmas veya slot zaten açık.");
            return false;
        }

        public int CalculateNextUnlockCost(int currentSlotIndex)
        {
            return currentSlotIndex * INCREASE_UNLOCK_COST_PER_SLOT;
        }
    }
}