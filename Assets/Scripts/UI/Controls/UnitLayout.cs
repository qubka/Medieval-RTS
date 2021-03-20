using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitLayout : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	[HideInInspector] public float width;
	[HideInInspector] public RectTransform worldTransform;
	[HideInInspector] public Transform parentTransform;

	public Squad squad;
	private UnitManager manager;

	private void Start()
	{
		worldTransform = transform.GetComponent<RectTransform>();
		width = worldTransform.sizeDelta.x / 3f;
		parentTransform = worldTransform.parent;
		manager = Manager.unitManager;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		//manager.SelectSquad(squad);
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		manager.unitLayout = this;
		manager.AddSelected(squad);
	}
	
	public void OnDrag(PointerEventData eventData)
	{
		// Required to enable drag feature
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		manager.unitLayout = null;
	}

	private void OnDestroy()
	{
		if (manager.unitLayout == this) {
			manager.unitLayout = null;
		}
	}
}