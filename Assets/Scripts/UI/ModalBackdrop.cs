using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    [RequireComponent(typeof(RectTransform))]
    public class ModalBackdrop : MonoBehaviour, IPointerClickHandler
    {
        private Action _onBackdropClicked;

        public void Initialize(Action onBackdropClicked)
        {
            _onBackdropClicked = onBackdropClicked;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _onBackdropClicked?.Invoke();
            gameObject.SetActive(false);
        }
    }
}