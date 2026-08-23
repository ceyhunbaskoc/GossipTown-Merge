using System.Collections.Generic;
using UnityEngine;

namespace Data.Roadmap
{
    [CreateAssetMenu(fileName = "RoadmapDatabase", menuName = "Roadmap/Database")]
    public class RoadmapDatabaseSO : ScriptableObject
    {
        [field: SerializeField] public List<MapNodeDefinitionSO> AllNodes { get; private set; }
        [field: SerializeField] public Sprite UnBuildSprite { get; private set; }
    }
}