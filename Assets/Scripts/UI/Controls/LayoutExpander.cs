using UnityEngine;
using UnityEngine.EventSystems;

public class LayoutExpander : UIBehaviour
{
    public float extraWidth;
    public RectTransform rectLayout;

    private float height;
    private RectTransform rectTransform;

    protected override void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        height = rectLayout.sizeDelta.y;
    }

    protected override void OnRectTransformDimensionsChange()
    {
        if (rectTransform) {
            rectLayout.sizeDelta = new Vector2(rectTransform.sizeDelta.x + extraWidth, height);
        }
    }
}
