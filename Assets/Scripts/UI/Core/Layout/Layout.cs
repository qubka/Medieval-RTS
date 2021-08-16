using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

public class Layout<T> : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, ILayout where T: ISelectable
{
    public T layout;
    public LayoutPursue cardPursue;
    
    protected IManager<T> manager;
    private Transform worldTransform;
    private Transform parentTransform;
    
    private void Awake()
    {
        worldTransform = transform;
        parentTransform = worldTransform.parent;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Input.GetKey(Manager.global.addKey)) {
            if (manager.SelectedCount() > 0) {
                var current = worldTransform.GetSiblingIndex();
                var count = parentTransform.childCount;
                var objects = new T[count];

                var min = int.MaxValue;
                var max = int.MinValue;
				
                for (var i = 0; i < count; i++) {
                    var o = parentTransform.GetChild(i).GetComponent<Layout<T>>().layout;
                    if (o.IsSelect()) {
                        min = math.min(min, i);
                        max = math.max(max, i);
                    }
                    objects[i] = o;
                }
				
                if (min == current && max == current)
                    return;
				
                if (min >= current) {
                    for (var i = current; i < min; i++) {
                        manager.AddSelected(objects[i]);
                    }
                } else if (max >= current) {
                    for (var i = min + 1; i <= current; i++) {
                        manager.AddSelected(objects[i]);
                    }
                } else {
                    for (var i = max + 1; i <= current; i++) {
                        manager.AddSelected(objects[i]);
                    }
                }
            } else {
                manager.AddSelected(layout);
            }
        } else if (Input.GetKey(Manager.global.inclusiveKey)) {
            manager.AddSelected(layout, true);
        } else {
            switch (manager.SelectedCount()) {
                case 0:
                    manager.AddSelected(layout);
                    break;
                
                case 1:
                    if (layout.IsSelect()) {
                        manager.DeselectAll();
                    } else {
                        manager.DeselectAllExcept(layout);
                    }
                    break;
                
                default:
                    manager.DeselectAllExcept(layout);
                    break;
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        manager.SetLayout(this);
        manager.DeselectAllExcept(layout);
        cardPursue.worldTransform.SetAsLastSibling();
        cardPursue.enabled = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Do nothing
        // Apparently this interface needs to exist in order for BeginDrag and EndDrag to work,
        // but we don't actually have anything to do here
        cardPursue.worldTransform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        manager.SetLayout(null);
        cardPursue.enabled = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var troopLayout = manager.GetLayout();
        if (troopLayout != null && !ReferenceEquals(troopLayout, this)) {
            var newPos = worldTransform.GetSiblingIndex();
            var oldPos = troopLayout.GetTransform().GetSiblingIndex();
            troopLayout.GetTransform().SetSiblingIndex(newPos);
            manager.SwapSelected(newPos, oldPos);
        }
    }
    
    private void OnDestroy()
    {
        if (ReferenceEquals(manager.GetLayout(), this)) {
            manager.SetLayout(null);
        }
    }

    public Transform GetTransform()
    {
        return worldTransform;
    }
}