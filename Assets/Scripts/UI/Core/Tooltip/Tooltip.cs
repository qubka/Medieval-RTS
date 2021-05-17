using System;
using UnityEngine;
using UnityEngine.UI;

public abstract class Tooltip : MonoBehaviour
{
    [SerializeField] private float padding;
    //[SerializeField] private Vector3 anchor;
    
#pragma warning disable 108,114    
    private Camera camera;
#pragma warning restore 108,114
    private Vector3 shift;
    private RectTransform rectTransform;
    private Canvas popupCanvas;
    
    //private Vector3 position => anchor.SqMagnitude() > 0f ? camera.WorldToScreenPoint(anchor) : Input.mousePosition;
    
    private void Awake()
    {
        rectTransform = transform as RectTransform;
        Resize();
        popupCanvas = transform.parent.GetComponent<Canvas>();
        SetActive(false);
    }

    private void Start()
    {
        camera = Manager.mainCamera;
    }

    private void OnRectTransformDimensionsChange()
    {
        if (rectTransform) {
            Resize();
        }
    }

    private void Resize()
    {
        var size = rectTransform.sizeDelta;
        shift = new Vector3(size.x / 2f, size.y / 2f, 0f);
    }
    
    private void Update()
    {
        FollowCursor();
    }

    private void FollowCursor()
    {
        var rect = rectTransform.rect;
        var newPos = Input.mousePosition + shift;
        //newPos.z = 0f;
        
        var rightEdgeToScreenEdgeDistance = Screen.width - (newPos.x + rect.width * popupCanvas.scaleFactor / 2f) - padding;
        if (rightEdgeToScreenEdgeDistance < 0f) {
            newPos.x += rightEdgeToScreenEdgeDistance;
        }
        var leftEdgeToScreenEdgeDistance = 0f - (newPos.x - rect.width * popupCanvas.scaleFactor / 2f) + padding;
        if (leftEdgeToScreenEdgeDistance > 0f) {
            newPos.x += leftEdgeToScreenEdgeDistance;
        }
        var topEdgeToScreenEdgeDistance = Screen.height - (newPos.y + rect.height * popupCanvas.scaleFactor) - padding;
        if (topEdgeToScreenEdgeDistance < 0f) {
            newPos.y += topEdgeToScreenEdgeDistance;
        }
        var bottomEdgeToScreenEdgeDistance = 0f - (newPos.y - rect.height * popupCanvas.scaleFactor / 2f) + padding;
        if (bottomEdgeToScreenEdgeDistance > 0f) {
            newPos.y += bottomEdgeToScreenEdgeDistance;
        }

        rectTransform.position = newPos;
    }

    protected void SetActive(bool value)
    {
        rectTransform.localScale = value ? Vector3.one : Vector3.zero;
    }
    
    protected void ForceRebuildLayoutImmediate()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }
}