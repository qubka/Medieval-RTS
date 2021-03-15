using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
 
[RequireComponent(typeof(Button))]
public class UnitButton : MonoBehaviour, IPointerDownHandler
{
	[ReadOnly] public Squad squad; 
	private UnitManager unitManager;

	private void Start()
	{
		//find button stats and turn them off
		//stats = transform.Find("Stats").gameObject;
		//stats.SetActive(false);
		unitManager = Manager.unitManager;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		unitManager.SelectSquad(squad);
	}
}