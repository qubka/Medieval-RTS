using UnityEngine;
using UnityEngine.EventSystems;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private DragCell cell;
    private Vector3 startPosition;
    private Transform worldTransform;
    private UnitManager manager;
 
    private void Start()
    {
        manager = Manager.unitManager;
        worldTransform = transform;
        cell = worldTransform.parent.GetComponent<DragCell>();
    }
 
    public void OnBeginDrag(PointerEventData eventData)
    {
        //container.objectBeingDragged = this;
        startPosition = worldTransform.position;
    }
    
    public void OnDrag(PointerEventData data)
    {
        worldTransform.position = Input.mousePosition;
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        worldTransform.position = startPosition;
    }
}
