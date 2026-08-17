using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "NewStartingSetup", menuName = "Bootstrap/Starting Board Setup")]
    public class StartingBoardSetupSO : ScriptableObject
    {
        [Header("Grid Dimensions")]
        [field: SerializeField, Min(1)] public int BoardWidth { get; private set; } = 7;
        [field: SerializeField, Min(1)] public int BoardHeight { get; private set; } = 9;
        
        [field: SerializeField, Min(1)] public int BackpackWidth { get; private set; } = 4;
        [field: SerializeField, Min(1)] public int BackpackHeight { get; private set; } = 8;
        [field: SerializeField] public List<CellSetupData> InitialCells { get; private set; }
        [field: SerializeField] public List<CellSetupData> BackpackCells { get; private set; }
    }
}