using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitting : MonoBehaviour
{
    private RectTransform rectTransform;
    private Rect lastSafeArea;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    void OnEnable()
    {
        ApplySafeArea();
        Canvas.willRenderCanvases += ApplySafeArea; // her çizimde uygula
    }

    void OnDisable()
    {
        Canvas.willRenderCanvases -= ApplySafeArea;
    }

    void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;

        float topPadding = Screen.height - (safeArea.y + safeArea.height);
        rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, -topPadding);

        lastSafeArea = safeArea;
    }
}
