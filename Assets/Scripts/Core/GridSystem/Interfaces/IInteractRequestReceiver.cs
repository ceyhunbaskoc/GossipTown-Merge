using UnityEngine;

namespace Core.GridSystem
{
    public interface IInteractRequestReceiver
    {
        void OnItemInteractRequested(Vector2Int gridPosition, IGridItem gridItem);
    }
}