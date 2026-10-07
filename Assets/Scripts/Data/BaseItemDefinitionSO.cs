using System;
using Core.GridSystem;
using UnityEngine;

namespace Data
{
    public abstract class BaseItemDefinitionSO : ScriptableObject
    {
        [field: SerializeField] public string Id { get; protected set; }
        [field: SerializeField] public string ItemName { get; protected set; }
        [field: SerializeField] public bool IsSellable { get; protected set; } = true;
        [field: SerializeField] public int BaseSellPrice { get; protected set; } = 3;
        public abstract int MaxLevel { get; }
        
        [Header("UI Info")]
        [SerializeField, TextArea(2, 4)] private string _description;
        public abstract Sprite GetIcon(int level);
        
        public virtual IGridItem CreateRuntimeData(string id, int level)
        {
            return new ItemData(id, level);
        }
        
        public string GetDescription()
        {
            if (string.IsNullOrWhiteSpace(_description))
            {
                return "To upgrade this item, find another one just like it and merge them.";
            }
            return _description;
        }
        
        public virtual int GetSellPrice(int level)
        {
            return Mathf.FloorToInt(BaseSellPrice * (level*0.75f));
        }
    }
}