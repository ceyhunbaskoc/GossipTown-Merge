using Core.GridSystem;
using UnityEngine;

namespace Data
{
    public abstract class BaseItemDefinitionSO : ScriptableObject
    {
        [field: SerializeField] public string Id { get; protected set; }
        [field: SerializeField] public string ItemName { get; protected set; }
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
                return "Bu eşyayı geliştirmek için aynısından bir tane daha bularak birleştirin.";
            }
            return _description;
        }
    }
}