using Core.GridSystem;
using Data;

namespace Core.Services
{
    public interface ILootGenerationService
    {
        ItemIdentifier GenerateLootForChest(ChestData chestData);
    }
}