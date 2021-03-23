using System;
using System.Collections.Generic;
using DigitalRuby.Tween;
using UnityEngine;
using UnityEngine.UI;

public class HideButton : MonoBehaviour
{
    private Image image;
    public Sprite activate;
    public Sprite disabled;
    public List<Rect> rectangles;
    public bool enable;
    
    [Serializable]
    public class Rect
    {
        public RectTransform transform;
        [HideInInspector] public float y;
    }
    
    private void Start()
    {
        image = GetComponent<Image>();

        foreach (var rect in rectangles) {
            rect.y = rect.transform.localPosition.y;
        }
    }

    public void OnButtonPressed()
    {
        enable = !enable;
        image.sprite = enable ? activate : disabled;
        foreach (var rect in rectangles) {
            var trans = rect.transform;
            var current = trans.localPosition.y;
            var target = enable ? rect.y : rect.y - trans.sizeDelta.y;

            void Movement(ITween<float> t) {
                var pos = trans.localPosition;
                pos.y = t.CurrentValue;
                trans.localPosition = pos;
            }

            var obj = trans.gameObject;
            obj.Tween(obj.name, current, target, 1.0f, TweenScaleFunctions.CubicEaseOut, Movement);
        }
    }
}
