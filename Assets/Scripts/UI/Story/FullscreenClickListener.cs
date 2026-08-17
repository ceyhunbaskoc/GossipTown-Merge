using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Story
{
    public class FullscreenClickListener : MonoBehaviour, IPointerClickHandler
    {
        public event Action OnScreenClicked;

        public void OnPointerClick(PointerEventData eventData)
        {
            OnScreenClicked?.Invoke();
        }
    }
}