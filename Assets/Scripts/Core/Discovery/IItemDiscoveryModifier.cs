namespace Core.Discovery
{
    public interface IItemDiscoveryModifier
    {
        bool TryUnlockItem(string itemId, int level);
    }
}