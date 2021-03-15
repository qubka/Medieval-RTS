using UnityEngine;
using UnityEngine.EventSystems;

public class LayoutExpander : UIBehaviour
{
    public float extraWidth;
    public RectTransform rectLayout;

    private float height;
    private RectTransform rectTransform;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        height = rectLayout.sizeDelta.y;
    }

    protected override void OnRectTransformDimensionsChange()
    {
        rectLayout.sizeDelta = new Vector2(rectTransform.sizeDelta.x + extraWidth, height);
    }
}
