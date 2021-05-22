using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

public class SquadLayout : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler
{
	[SerializeField] private Squad squad;
 	
 	private SquadManager manager;
 	private Transform worldTransform;
 	private Transform parentTransform;
 	private LayoutPursue cardPursue;
 
 	private void Awake()
 	{
 		worldTransform = transform;
 		parentTransform = worldTransform.parent;
 		cardPursue = squad.squadCard.GetComponent<LayoutPursue>();
 	}
 
 	private void Start()
 	{
 		manager = SquadManager.Instance;
 	}
 
 	public void OnPointerDown(PointerEventData eventData)
 	{
 		if (Input.GetKey(Manager.global.addKey)) {
 			if (manager.selectedCount > 0) {
 				var current = worldTransform.GetSiblingIndex();
 				var count = parentTransform.childCount;
 				var squads = new Squad[count];
 
 				var min = int.MaxValue;
 				var max = int.MinValue;
 				
 				for (var i = 0; i < count; i++) {
 					var squad = parentTransform.GetChild(i).GetComponent<SquadLayout>().squad;
 					if (squad.IsSelect()) {
 						min = math.min(min, i);
 						max = math.max(max, i);
 					}
 					squads[i] = squad;
 				}
 				
 				if (min == current && max == current)
 					return;
 				
 				if (min >= current) {
 					for (var i = current; i < min; i++) {
 						manager.AddSelected(squads[i]);
 					}
 				} else if (max >= current) {
 					for (var i = min + 1; i <= current; i++) {
 						manager.AddSelected(squads[i]);
 					}
 				} else {
 					for (var i = max + 1; i <= current; i++) {
 						manager.AddSelected(squads[i]);
 					}
 				}
 			} else {
 				manager.AddSelected(squad);
 			}
 		} else {
 			manager.AddSelected(squad, true);
 		}
 	}
 
 	public void OnBeginDrag(PointerEventData eventData)
 	{
 		manager.squadLayout = this;
 		manager.DeselectAllExcept(squad);
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
 		manager.squadLayout = null;
 		cardPursue.enabled = true;
 	}
 	
 	public void OnPointerEnter(PointerEventData eventData)
 	{
 		var squadLayout = manager.squadLayout;
         if (squadLayout && squadLayout != this) {
 	        squadLayout.worldTransform.SetSiblingIndex(worldTransform.GetSiblingIndex());
         }
 	}
 
 	private void OnDestroy()
 	{
 		if (manager.squadLayout == this) {
 			manager.squadLayout = null;
 		}
 	}
 }