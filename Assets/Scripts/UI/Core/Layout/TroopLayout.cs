using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

public class TroopLayout : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler
{
    [HideInInspector] public Troop troop;
    [HideInInspector] public LayoutPursue cardPursue;
    
    private Transform worldTransform;
    private Transform parentTransform;
    private ArmyManager manager;
    
    private void Awake()
    {
        worldTransform = transform;
        parentTransform = worldTransform.parent;
    }

    private void Start()
    {
        manager = ArmyManager.Instance;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Input.GetKey(Manager.global.addKey)) {
            if (manager.selectedCount > 0) {
                var current = worldTransform.GetSiblingIndex();
                var count = parentTransform.childCount;
                var troops = new Troop[count];

                var min = int.MaxValue;
                var max = int.MinValue;
				
                for (var i = 0; i < count; i++) {
                    var troop = parentTransform.GetChild(i).GetComponent<TroopLayout>().troop;
                    if (troop.IsSelect()) {
                        min = math.min(min, i);
                        max = math.max(max, i);
                    }
                    troops[i] = troop;
                }
				
                if (min == current && max == current)
                    return;
				
                if (min >= current) {
                    for (var i = current; i < min; i++) {
                        manager.AddSelected(troops[i]);
                    }
                } else if (max >= current) {
                    for (var i = min + 1; i <= current; i++) {
                        manager.AddSelected(troops[i]);
                    }
                } else {
                    for (var i = max + 1; i <= current; i++) {
                        manager.AddSelected(troops[i]);
                    }
                }
            } else {
                manager.AddSelected(troop);
            }
        } else {
            manager.AddSelected(troop, true);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        manager.troopLayout = this;
        manager.DeselectAllExcept(troop);
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
        manager.troopLayout = null;
        cardPursue.enabled = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var troopLayout = manager.troopLayout;
        if (troopLayout && troopLayout != this) {
            var newPos = worldTransform.GetSiblingIndex();
            var oldPos = troopLayout.worldTransform.GetSiblingIndex();
            troopLayout.worldTransform.SetSiblingIndex(newPos);
            manager.SwapSelected(newPos, oldPos);
        }
    }
    
    private void OnDestroy()
    {
        if (manager.troopLayout == this) {
            manager.troopLayout = null;
        }
    }
}