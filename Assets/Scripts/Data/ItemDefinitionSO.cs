using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [Serializable]
    public class NormalItemData
    {
        [field: SerializeField] public int Level { get; private set; }
        [field: SerializeField] public Sprite ItemIcon { get; private set; }
    }

    [CreateAssetMenu(fileName = "NewNormalItem", menuName = "MergeGame/Normal Item SO")]
    public class ItemDefinitionSO : BaseItemDefinitionSO
    {
        [field: SerializeField] public List<NormalItemData> Items { get; private set; }
        
        [field: SerializeField] public SpawnerDefinitionSO SourceSpawner { get; private set; }

        public override int MaxLevel => Items != null ? Items.Count : 0;
        public override Sprite GetIcon(int level)
        {
            var data = Items.Find(i => i.Level == level);
            return data?.ItemIcon;
        }
    }
}