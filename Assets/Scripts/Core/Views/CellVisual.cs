using Core.GridSystem;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Views
{
    public class CellVisual : MonoBehaviour, IViewCell
    {
        [SerializeField] private SpriteRenderer _cellVisualBg;
        [SerializeField] private SpriteRenderer _lockedImage;
        [SerializeField] private SpriteRenderer _unlockableImage;


        public void SetState(CellVisualState state)
        {
            switch (state)
            {
                case CellVisualState.Unlocked:
                    _lockedImage.gameObject.SetActive(false);
                    _unlockableImage.gameObject.SetActive(false);
                    break;
                
                case CellVisualState.LockedObscured:
                    _lockedImage.gameObject.SetActive(true);
                    _unlockableImage.gameObject.SetActive(false);
                    break;
                
                case CellVisualState.LockedUnlockable:
                    _lockedImage.gameObject.SetActive(false);
                    _unlockableImage.gameObject.SetActive(true);
                    break;
            }
        }

        public void SetBackgroundColor(Color color)
        {
            _cellVisualBg.color = color;
        }
    }
}