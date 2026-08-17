using System;
using UnityEngine;

namespace Core.PoolSystem
{
    public enum PoolObjectType
    {
        None = 0,
        DraggableItem = 1,
        CellVisual = 2,
        OrderCard = 3,
        OrderItem = 4,
        RewardItem = 5,
        FlightItem,
        QuestItem,
        QuestRewardItem,
        LevelRewardItemViewNoEffect,
        LevelRewardItemView,
        LevelRewardRowView,
        MergeParticle,
        ShopPurchaseItem,
        ItemDetailSlotView
    }
    [Serializable]
    public class PoolMapping
    {
        public PoolObjectType type;
        public GameObject prefab;
        [Min(1)] public int initialSize = 10;
    }
}