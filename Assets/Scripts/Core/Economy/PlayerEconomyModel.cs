using System;

namespace Core.Economy
{
    public interface IReadOnlyEconomyModel
    {
        int Energy { get; }
        int Gems { get; }
        int Golds { get; }
        int EnergyRegenerationTrigger { get; }
        bool CanEnergyRegeneration { get; }
        event Action<int> OnEnergyChanged;
        event Action<int> OnGoldChanged;
        event Action<int> OnGemChanged;
        
        event Action <int> OnEnergySpend;
        event Action <int> OnGoldSpend;
        event Action <int> OnGemSpend;
    }
    
    public interface IEconomyModifier
    {
        bool AddGold(int amount);
        bool AddEnergy(int amount);
        bool AddGem(int amount);
        bool TrySpendEnergy(int amount);
        bool TrySpendGems(int amount);
        bool TrySpendGold(int amount);
        bool HasEnoughEnergy(int amount);
        bool HasEnoughGold(int amount);
    }
    public class PlayerEconomyModel : IReadOnlyEconomyModel, IEconomyModifier
    {
        public int Energy { get; private set; }
        public int Golds { get; private set; }
        public int Gems { get; private set; }
        
        public int EnergyRegenerationTrigger { get; private set; }
        public bool CanEnergyRegeneration => Energy < EnergyRegenerationTrigger;

        public event Action<int> OnEnergyChanged;
        public event Action<int> OnGoldChanged;
        public event Action<int> OnGemChanged;
        
        public event Action<int> OnEnergySpend;
        public event Action<int> OnGoldSpend;
        public event Action<int> OnGemSpend;

        public PlayerEconomyModel(int startingEnergy, int startingGem, int startingGold, int energyRegenerationTrigger)
        {
            Energy = startingEnergy;
            Gems = startingGem;
            Golds = startingGold;
            EnergyRegenerationTrigger = energyRegenerationTrigger;
        }

        public bool TrySpendEnergy(int amount)
        {
            if (Energy >= amount)
            {
                Energy -= amount;
                OnEnergyChanged?.Invoke(Energy);
                OnEnergySpend?.Invoke(Energy);
                return true;
            }
            return false;
        }

        public bool TrySpendGems(int amount)
        {
            if (Gems >= amount)
            {
                Gems -= amount;
                OnGemChanged?.Invoke(Gems);
                OnGemSpend?.Invoke(Gems);
                return true;
            }
            return false;
        }
        
        public bool TrySpendGold(int amount)
        {
            if (Golds >= amount)
            {
                Golds -= amount;
                OnGoldChanged?.Invoke(Golds);
                OnGoldSpend?.Invoke(Golds);
                return true;
            }
            return false;
        }

        public bool HasEnoughEnergy(int amount)
        {
            if (Energy >= amount)
            {
                return true;
            }
            return false;
        }
        
        public bool HasEnoughGold(int amount)
        {
            if (Golds >= amount)
            {
                return true;
            }
            return false;
        }

        public bool AddGold(int amount)
        {
            if (amount < 0) return false;
            Golds += amount;
            OnGoldChanged?.Invoke(Golds);
            return true;
        }

        public bool AddEnergy(int amount)
        {
            if (amount < 0) return false;
            Energy += amount;
            OnEnergyChanged?.Invoke(Energy);
            return true;
        }

        public bool AddGem(int amount)
        {
            if (amount < 0) return false;
            Gems += amount;
            OnGemChanged?.Invoke(Gems);
            return true;
        }
    }
}