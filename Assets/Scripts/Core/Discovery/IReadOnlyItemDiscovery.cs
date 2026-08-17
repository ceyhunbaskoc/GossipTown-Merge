using System.Collections.Generic;

namespace Core.Discovery
{
    public interface IReadOnlyItemDiscovery
    {
        IReadOnlyDictionary<string,int>  UnlockedItems();
        IReadOnlyList<string> GetUnlockedItemIds();
        int GetMaxUnlockedLevelFor(string itemId);
        bool IsItemUnlocked(string itemId, int level);
    }
}