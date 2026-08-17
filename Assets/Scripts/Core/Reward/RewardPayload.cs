namespace Core.Reward
{
    public readonly struct RewardPayload
    {
        public readonly string ItemId;
        public readonly int Level;
        public readonly int Amount;
        
        public bool IsValid => !string.IsNullOrEmpty(ItemId);
    
        public static RewardPayload Empty => new RewardPayload();

        public RewardPayload(string itemId, int level, int amount = 1)
        {
            ItemId = itemId;
            Level = level;
            Amount = amount;
        }
    }
}