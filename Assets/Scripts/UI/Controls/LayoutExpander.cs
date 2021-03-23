using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LayoutExpander : UIBehaviour
{
    public List<Rect> rectangles;
    private RectTransform rectTransform;

    [Serializable]
    public class Rect
    {
        public RectTransform transform;
        public float minWidth;
        public bool setHeight;
    }
    
    protected override void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    protected override void OnRectTransformDimensionsChange()
    {
        if (rectTransform) {
            var size = rectTransform.sizeDelta;
            foreach (var rect in rectangles) {
                var trans = rect.transform;
                var width = Mathf.Max(rect.minWidth, size.x);
                var sizeDelta = trans.sizeDelta;
                sizeDelta.x = width;
                if (rect.setHeight) {
                    sizeDelta.y = size.y;
                }
                trans.sizeDelta = sizeDelta;
                trans.gameObject.SetActive(width > 0f);
            }
        }
    }
}
