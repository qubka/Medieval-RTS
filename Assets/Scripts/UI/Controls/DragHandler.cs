using UnityEngine;
using UnityEngine.EventSystems;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler
{
    private Vector3 startPosition;
    private Transform worldTransform;
    private DragOrderContainer container;
 
    private void Start() 
    {
        container = GetComponentInParent<DragOrderContainer>();
        worldTransform = transform;
    }
 
    public void OnBeginDrag(PointerEventData eventData)
    {
        container.objectBeingDragged = this;
        startPosition = worldTransform.position;
    }
    
    public void OnDrag(PointerEventData data)
    {
        worldTransform.position = Input.mousePosition;
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (container.objectBeingDragged == this) {
            container.objectBeingDragged = null;
            worldTransform.position = startPosition;
        }
    }
 
    public void OnPointerEnter(PointerEventData eventData)
    {
        var objectBeingDragged = container.objectBeingDragged;
        if (objectBeingDragged != null && objectBeingDragged != this) {
            objectBeingDragged.worldTransform.SetSiblingIndex(worldTransform.GetSiblingIndex());
            
            startPosition = objectBeingDragged.startPosition;
            objectBeingDragged.startPosition = worldTransform.position;
        }
    }
}
