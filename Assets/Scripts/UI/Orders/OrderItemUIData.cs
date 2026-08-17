using Core.GridSystem;
using UnityEngine;

namespace UI.Orders
{
    public readonly struct OrderItemUIData
    {
        public readonly ItemIdentifier Identifier;
        public readonly Sprite Icon;
        public readonly int RequiredCount;

        public OrderItemUIData(ItemIdentifier identifier, Sprite icon, int requiredCount)
        {
            Identifier = identifier;
            Icon = icon;
            RequiredCount = requiredCount;
        }
    }
}