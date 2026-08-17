using UnityEngine;

namespace Core.GridSystem
{
    public interface IViewItem
    {
        IGridItem Data { get; }
        Vector2Int CurrentGridPosition { get; set; }
        
        void SnapBackToStart();
        void MoveToWorldPosition(Vector3 targetPosition);
        void MoveWithArcTo(Vector3 targetPosition);
    }
}