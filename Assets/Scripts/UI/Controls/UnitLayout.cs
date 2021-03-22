using UnityEngine;
using UnityEngine.EventSystems;

public class UnitLayout : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler
{
	public Squad squad;
	private UnitManager manager;
	private Transform worldTransform;
	private Transform parentTransform;
	private Transform cardTransform;
	private LayoutPursue cardPursue;

	private void Start()
	{
		worldTransform = transform;
		parentTransform = worldTransform.parent;
		cardTransform = squad.cardTransform;
		cardPursue = cardTransform.GetComponent<LayoutPursue>();
		manager = Manager.unitManager;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (Input.GetKey(manager.addKey)) {
			if (manager.selectedCount > 0) {
				var current = worldTransform.GetSiblingIndex();
				var count = parentTransform.childCount;
				var squads = new Squad[count];

				var min = int.MaxValue;
				var max = int.MinValue;
				
				for (var i = 0; i < count; i++) {
					var squad = parentTransform.GetChild(i).GetComponent<UnitLayout>().squad;
					if (squad.IsSelect) {
						min = Mathf.Min(min, i);
						max = Mathf.Max(max, i);
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
		manager.unitLayout = this;
		manager.DeselectAllExcept(squad);
		cardTransform.SetAsLastSibling();
		cardPursue.enabled = false;
	}
	
	public void OnDrag(PointerEventData eventData)
	{
		// Do nothing
		// Apparently this interface needs to exist in order for BeginDrag and EndDrag to work,
		// but we don't actually have anything to do here
		cardTransform.position = Input.mousePosition;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		manager.unitLayout = null;
		cardPursue.enabled = true;
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