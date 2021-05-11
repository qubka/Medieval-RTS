using DigitalRuby.Tween;
using UnityEngine;

public abstract class TweenBehaviour : MonoBehaviour
{
    private RectTransform rectTransform;
    private float initial;
    private bool enable;
    private bool shift;

    protected virtual float Offset => 0f;
    
    protected virtual void Awake()
    {
        rectTransform = transform as RectTransform;
        initial = rectTransform.localPosition.y;
    }
    
    public void Toggle(bool value)
    {
        if (enable == value)
            return;
        
        enable = value;
        
        var current = rectTransform.localPosition.x;
        var target = rectTransform.sizeDelta.x / (value ? 2f : -2f);

        gameObject.Tween(name + "Move", current, target, 0.5f, TweenScaleFunctions.CubicEaseInOut, Move);
    }

    public void Shift(bool value)
    {
        if (shift == value)
            return;
        
        shift = value;
        
        var current = rectTransform.localPosition.y;
        var target = value ? initial + Offset : initial;

        gameObject.Tween(name + "Shift", current, target, 0.5f, TweenScaleFunctions.CubicEaseInOut, Shift);
    }

    public void Move(ITween<float> obj)
    {
        var position = rectTransform.localPosition;
        position.x = obj.CurrentValue;
        rectTransform.localPosition = position;
    }
    
    public void Shift(ITween<float> obj)
    {
        var position = rectTransform.localPosition;
        position.y = obj.CurrentValue;
        rectTransform.localPosition = position;
    }
}