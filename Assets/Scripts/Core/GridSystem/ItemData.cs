using Core.SaveSystem;

namespace Core.GridSystem
{
    public class ItemData : IGridItem
    {
        public string Id { get; }
        public int Level { get; }

        public ItemData(string id, int level)
        {
            Id = id;
            Level = level;
        }
        
        public virtual ItemSaveData GetSaveData()
        {
            return new ItemSaveData
            {
                Id = this.Id,
                Level = this.Level
            };
        }
    }
}