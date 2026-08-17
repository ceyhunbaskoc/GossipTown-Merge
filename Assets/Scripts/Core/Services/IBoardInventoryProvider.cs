using Core.GridSystem;

namespace Core.Services
{
    public interface IBoardInventoryProvider
    {
        int GetItemCountOnBoard(ItemIdentifier identifier);
    }
}