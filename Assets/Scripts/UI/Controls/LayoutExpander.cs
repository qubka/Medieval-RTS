using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class LayoutExpander : UIBehaviour
{
    public Rectangle[] rectangles;
    private RectTransform rectTransform;

    [Serializable]
    public class Rectangle
    {
        public RectTransform transform;
        public float minWidth;
    }
    
    protected override void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    protected override void OnRectTransformDimensionsChange()
    {
        if (rectTransform) {
            var max = rectTransform.sizeDelta.x;
            foreach (var rect in rectangles) {
                var trans = rect.transform;
                var width = Mathf.Max(rect.minWidth, max);
                if (width <= 0f) {
                    trans.gameObject.SetActive(false);
                } else {
                    trans.gameObject.SetActive(true);
                    var sizeDelta = trans.sizeDelta;
                    sizeDelta.x = width;
                    trans.sizeDelta = sizeDelta;
                }
            }
        }
    }
}
