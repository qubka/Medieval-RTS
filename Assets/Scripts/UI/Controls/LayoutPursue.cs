using DigitalRuby.Tween;
using UnityEngine;

public class LayoutPursue : MonoBehaviour
{
    [SerializeField] private Transform layoutTransform;
    private Transform rectTransform;
    private bool move;

    private void Start()
    {
        rectTransform = transform as RectTransform;
    }

    private void Update()
    {
        if (!move && rectTransform.position != layoutTransform.position) {
            gameObject.Tween("CardMove_" + GetInstanceID(), rectTransform.position, layoutTransform.position, 0.5f, TweenScaleFunctions.CubicEaseInOut, CardMove, TweenDone);
            move = true;
        }
    }
    
    private void TweenDone(ITween<Vector3> obj)
    {
        move = false;
    }

    private void CardMove(ITween<Vector3> obj)
    {
        rectTransform.position = obj.CurrentValue;
    }
}
