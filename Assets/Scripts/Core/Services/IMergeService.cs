using System;
using Core.GridSystem;
using UnityEngine;

namespace Core.Services
{
    public interface IMergeService
    {
        event Action<IGridItem> OnItemMerged;
        bool TryProcessMerge(Vector2Int fromPosition, Vector2Int toPosition);
    }
}