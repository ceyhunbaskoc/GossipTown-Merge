using UnityEngine;
using System.Collections.Generic;
using Core.GridSystem;
using Order;

namespace Data.Tutorial
{
    [CreateAssetMenu(fileName = "StartingOrderSetup", menuName = "Bootstrap/Starting Order Setup")]
    public class StartingOrderSetupSO : ScriptableObject
    {
        [Header("Tutorial Order Configuration")]
        public BaseItemDefinitionSO Item;
        public int ItemLevel;
        public int RequiredCount = 1;
        public int RewardAmount = 10;

        public OrderModel CreateTutorialOrder()
        {
            var items = new Dictionary<ItemIdentifier, int>
            {
                { new ItemIdentifier(Item.Id, ItemLevel), RequiredCount }
            };
            return new OrderModel(items, RewardAmount);
        }
    }
}