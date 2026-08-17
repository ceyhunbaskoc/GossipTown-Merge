using UnityEngine;
using UnityEngine.EventSystems;
using Core.Services;

namespace Core.Views
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class BoardBackdropView : MonoBehaviour, IPointerClickHandler
    {
        private BoardSelectionService _selectionService;

        public void Initialize(BoardSelectionService selectionService)
        {
            _selectionService = selectionService;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.pointerDrag != null) return;

            if (_selectionService != null && _selectionService.CurrentSelectedPosition.HasValue)
            {
                _selectionService.ClearSelection();
            }
        }
    }
}