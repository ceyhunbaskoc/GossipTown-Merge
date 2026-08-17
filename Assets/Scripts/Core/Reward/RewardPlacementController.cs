using Core.Controllers;
using Core.Factories;
using Core.GridSystem;
using UnityEngine;

namespace Core.Reward
{
    public class RewardPlacementController
    {
        private readonly PendingRewardModel _pendingRewardModel;
        private readonly GridDataModel _gridDataModel;
        
        private readonly GridItemDataFactory _itemFactory; 
        
        public RewardPlacementController(
            PendingRewardModel pendingRewardModel, 
            GridDataModel gridDataModel,
            GridItemDataFactory itemFactory)
        {
            _pendingRewardModel = pendingRewardModel;
            _gridDataModel = gridDataModel;
            _itemFactory = itemFactory;
        }

        public bool OnRewardUIActionClicked()
        {
            Vector2Int? spawnPosition = _gridDataModel.FindNearestEmptyCell(new Vector2Int(0,0));
            if (!spawnPosition.HasValue)
            {
                Debug.LogWarning("No empty cell available for reward placement.");
                return false;
            }

            if (_pendingRewardModel.TryClaimNextReward(out ItemIdentifier claimedReward))
            {
                IGridItem newItem = _itemFactory.CreateItemData(claimedReward.Id, claimedReward.Level);
                
                _gridDataModel.TryPlaceObject(spawnPosition.Value, newItem);
                return true;
            }

            return false;
        }
    }
}