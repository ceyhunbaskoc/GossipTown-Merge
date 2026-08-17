using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "RewardSelectionConfig", menuName = "Data/Reward Selection Config")]
    public class RewardSelectionConfigSO : ScriptableObject
    {
        [Header("Reward Probabilities")]
        public float SpawnerUpgradeWeight = 50f;
        public float ChestWeight = 30f;
        public float NormalItemWeight = 20f;
    }
}