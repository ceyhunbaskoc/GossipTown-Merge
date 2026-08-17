using System.Collections.Generic;
using Core.Discovery;
using Core.GridSystem;
using Core.Services;
using Data;

namespace Order
{
    public interface IOrderSelectionStrategy
    {
        ItemIdentifier SelectItem(List<ItemDefinitionSO> availableItems, IReadOnlyItemDiscovery discovery, IBoardInventoryProvider boardInventory);
    }
}