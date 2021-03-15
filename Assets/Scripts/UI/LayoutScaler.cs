using System;
using UnityEngine;

public class LayoutScaler : MonoBehaviour
{
    public float extraWidth;
    public RectTransform rectLayout;

    private float height;
    private RectTransform rectTransform;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        height = rectTransform.sizeDelta.y;
    }

    public void Update()
    {
        rectTransform.sizeDelta = new Vector2(rectLayout.sizeDelta.x + extraWidth, height);
    }

    private void OnRectTransformDimensionsChange()
    {
        throw new NotImplementedException();
    }
}
