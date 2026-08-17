using UnityEngine;

namespace Data.Roadmap
{
    [CreateAssetMenu(fileName = "NewMapNode", menuName = "Roadmap/Map Node")]
    public class MapNodeDefinitionSO : ScriptableObject
    {
        [field: SerializeField] public string NodeId { get; private set; }
        [field: SerializeField] public BuildingDefinitionSO Building { get; private set; }
        
        [Header("Unlock Requirements")]
        [field: SerializeField] public MapNodeDefinitionSO RequiredPreviousNode { get; private set; }
        [field: SerializeField] public int RequiredPreviousNodeLevel { get; private set; } = 1;
        
        [Header("Unlock Requirements")]
        [SerializeField] private int _requiredPlayerLevel = 1;

        public int RequiredPlayerLevel => _requiredPlayerLevel;
    }
}