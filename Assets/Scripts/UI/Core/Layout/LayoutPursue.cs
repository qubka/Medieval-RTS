using DigitalRuby.Tween;
using UnityEngine;

public class LayoutPursue : MonoBehaviour
{
    public Transform layoutTransform;
    [HideInInspector] public Transform worldTransform;
    private bool move;

    private void Awake()
    {
        worldTransform = transform;
    }

    private void Update()
    {
        if (!move && worldTransform.position != layoutTransform.position) {
            gameObject.Tween("CardMove_" + GetInstanceID(), worldTransform.position, layoutTransform.position, 0.5f, TweenScaleFunctions.CubicEaseInOut, CardMove, TweenDone);
            move = true;
        }
    }
    
    private void TweenDone(ITween<Vector3> obj)
    {
        move = false;
    }

    private void CardMove(ITween<Vector3> obj)
    {
        worldTransform.position = obj.CurrentValue;
    }
}
