using UnityEngine;
using System.Collections.Generic;
using Core.GridSystem;
using Order;

namespace Data.Tutorial
{
    [CreateAssetMenu(fileName = "StartingOrderSetup", menuName = "Data/Starting Order Setup")]
    public class StartingOrderSetupSO : ScriptableObject
    {
        [Header("Tutorial Order Configuration")]
        public string ItemId;
        public int ItemLevel;
        public int RequiredCount = 1;
        public int RewardAmount = 10;

        public OrderModel CreateTutorialOrder()
        {
            var items = new Dictionary<ItemIdentifier, int>
            {
                { new ItemIdentifier(ItemId, ItemLevel), RequiredCount }
            };
            return new OrderModel(items, RewardAmount);
        }
    }
}