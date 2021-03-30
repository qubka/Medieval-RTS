using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
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
        if (gameObject.activeInHierarchy) {
            StartCoroutine(AfterRectTransformDimensionsChange());
        }
    }
    
    private IEnumerator AfterRectTransformDimensionsChange()
    {
        yield return new WaitForEndOfFrame();
        if (rectTransform) {
            var size = rectTransform.sizeDelta;
            foreach (var rect in rectangles) {
                var trans = rect.transform;
                var width = math.max(rect.minWidth, size.x);
                var enable = width > 0f;
                if (enable) {
                    var sizeDelta = trans.sizeDelta;
                    sizeDelta.x = width;
                    if (rect.setHeight) {
                        sizeDelta.y = size.y;
                    }
                    trans.sizeDelta = sizeDelta;
                }
                trans.gameObject.SetActive(enable);
            }
        }
    }
}
