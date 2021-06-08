using System;
using DigitalRuby.Tween;
using UnityEngine;
using UnityEngine.UI;

public class HideButton : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Sprite activate;
    [SerializeField] private Sprite disabled;
    [SerializeField] private Rect[] rectangles;
    [SerializeField] private bool enable;
    
    [Serializable]
    public class Rect
    {
        public RectTransform transform;
        [HideInInspector] public float y;
    }
    
    private void Awake()
    {
        foreach (var rect in rectangles) {
            rect.y = rect.transform.localPosition.y;
        }
    }

    public void Toggle()
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
            obj.Tween(obj.name, current, target, 1f, TweenScaleFunctions.CubicEaseOut, Movement);
        }
    }
}
