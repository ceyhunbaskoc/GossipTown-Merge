using UnityEngine;
using UnityEngine.UI;

namespace UI.FlightSystem
{
    public class FlightItemView : MonoBehaviour
    {
        [field: SerializeField] public RectTransform RectTransform { get; private set; }
        
        [SerializeField] private Image _iconImage;

        public void SetVisual(Sprite iconSprite)
        {
            if (_iconImage != null)
            {
                _iconImage.sprite = iconSprite;
            }
        }
    }
}