using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class TopSafeAreaShifter : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Canvas _parentCanvas;
    
    private Rect _lastSafeArea = Rect.zero;
    private Vector2 _lastResolution = Vector2.zero;
    private float _initialAnchoredPositionY;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _parentCanvas = GetComponentInParent<Canvas>();
        _initialAnchoredPositionY = _rectTransform.anchoredPosition.y;
        
        ApplySafeAreaShift();
    }

    private void Update()
    {
        if (_lastSafeArea == Screen.safeArea && 
            _lastResolution.x == Screen.width && 
            _lastResolution.y == Screen.height)
        {
            return;
        }

        ApplySafeAreaShift();
    }

    private void ApplySafeAreaShift()
    {
        if (_parentCanvas == null) return;

        _lastSafeArea = Screen.safeArea;
        _lastResolution = new Vector2(Screen.width, Screen.height);

        float topPaddingPixels = Screen.height - _lastSafeArea.yMax;
        float canvasTopPadding = topPaddingPixels / _parentCanvas.scaleFactor;
        Vector2 newPosition = _rectTransform.anchoredPosition;
        newPosition.y = _initialAnchoredPositionY - canvasTopPadding;
        
        _rectTransform.anchoredPosition = newPosition;
    }
}