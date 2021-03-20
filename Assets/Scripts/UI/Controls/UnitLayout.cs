using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitLayout : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler
{
	public Squad squad;
	private UnitManager manager;
	private Transform worldTransform;

	private void Start()
	{
		worldTransform = transform;
		manager = Manager.unitManager;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		//manager.SelectSquad(squad);
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		manager.unitLayout = this;
		manager.DeselectAllExcept(squad);
		squad.layoutTransform.SetAsLastSibling();
	}
	
	public void OnDrag(PointerEventData eventData)
	{
		// Do nothing
		// Apparently this interface needs to exist in order for BeginDrag and EndDrag to work,
		// but we don't actually have anything to do here
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		manager.unitLayout = null;
	}
	
	public void OnPointerEnter(PointerEventData eventData)
	{
		if (manager.unitLayout && manager.unitLayout != this) {
			manager.unitLayout.worldTransform.SetSiblingIndex(worldTransform.GetSiblingIndex());
		}
	}

	private void OnDestroy()
	{
		if (manager.unitLayout == this) {
			manager.unitLayout = null;
		}
	}
}