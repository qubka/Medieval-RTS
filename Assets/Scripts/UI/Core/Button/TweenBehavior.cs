using System.Collections;
using DigitalRuby.Tween;
using UnityEngine;

public abstract class TweenBehaviour : MonoBehaviour
{
    [SerializeField] private float offset;
    private RectTransform rectTransform;
    private float initial;
    private bool enable;
    private bool shift;

    protected virtual void Awake()
    {
        rectTransform = transform as RectTransform;
        initial = rectTransform.localPosition.y;
    }
    
    protected virtual void Start()
    {
        StartCoroutine(Tick());
    }

    private IEnumerator Tick()
    {
        while (true) {
            OnUpdate();
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }

    public abstract void OnUpdate();
    
    public virtual void Toggle(bool value)
    {
        if (enable == value)
            return;
        
        enable = value;
        
        var current = rectTransform.localPosition.x;
        var target = rectTransform.sizeDelta.x / (value ? 2f : -2f);

        gameObject.Tween(name + "_Move_" + GetInstanceID(), current, target, 0.5f, TweenScaleFunctions.CubicEaseInOut, Move);
    }

    public virtual void Shift(bool value)
    {
        if (shift == value)
            return;
        
        shift = value;
        
        var current = rectTransform.localPosition.y;
        var target = value ? initial + offset : initial;

        gameObject.Tween(name + "_Shift_" + GetInstanceID(), current, target, 0.5f, TweenScaleFunctions.CubicEaseInOut, Shift);
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