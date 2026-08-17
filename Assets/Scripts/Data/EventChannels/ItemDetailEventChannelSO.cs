using System;
using UnityEngine;

namespace Data.EventChannels
{
    public struct ItemDetailRequest
    {
        public string ItemId;
        public int ItemLevel;
    }

    [CreateAssetMenu(menuName = "Events/Item Detail Event Channel", fileName = "ItemDetailEventChannel")]
    public class ItemDetailEventChannelSO : ScriptableObject
    {
        public event Action<ItemDetailRequest> OnEventRaised;

        public void RaiseEvent(ItemDetailRequest request)
        {
            if (OnEventRaised != null)
            {
                OnEventRaised.Invoke(request);
            }
        }
    }
}