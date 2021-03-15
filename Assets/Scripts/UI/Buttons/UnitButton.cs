using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
 
[RequireComponent(typeof(Button))]
public class UnitButton : MonoBehaviour, IPointerDownHandler
{
	[ReadOnly] public Squad squad; 
	private UnitManager manager;

	private void Start()
	{
		//find button stats and turn them off
		//stats = transform.Find("Stats").gameObject;
		//stats.SetActive(false);
		manager = Manager.unitManager;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		manager.SelectSquad(squad);
	}
}