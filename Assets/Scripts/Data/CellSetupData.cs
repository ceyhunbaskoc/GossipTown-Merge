using System;
using UnityEngine;

namespace Data
{
    [Serializable]
    public struct CellSetupData
    {
        public Vector2Int Position;
        
        [Header("Item Settings (Can be left empty)")]
        public BaseItemDefinitionSO ItemDef;
        public int ItemLevel;
        
        [Header("Lock Settings")]
        public bool IsLocked;
    }
    
}