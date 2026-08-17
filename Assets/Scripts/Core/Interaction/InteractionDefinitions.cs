using Core.GridSystem;

namespace Core.Interaction
{
    public interface IInteractableDefinition
    {
        bool CanInteract(int currentLevel);
        
        ItemIdentifier GetDropItem(int currentLevel); 
    }

    public interface ICollectibleDefinition
    {
        int GetRewardAmount(int currentLevel);
    }
}