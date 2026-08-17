using UnityEngine;

namespace Core.GridSystem
{
    public interface IDropRequestReceiver
    {
        void OnItemDropRequested(IViewItem viewItem, Vector3 dropWorldPosition);
    }
}