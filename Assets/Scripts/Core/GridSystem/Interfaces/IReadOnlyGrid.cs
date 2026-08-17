using UnityEngine;

namespace Core.GridSystem
{
    public interface IReadOnlyGrid
    {
        int Width { get; }
        int Height { get; }

        bool IsValidBoundary(Vector2Int position);
        bool IsCellOccupied(Vector2Int position);
        bool IsCellLocked(Vector2Int position);
        bool IsCellUnlockable(Vector2Int position);
        IGridItem GetItemAt(Vector2Int position);
        Vector2Int? FindNearestEmptyCell(Vector2Int position);
    }
}