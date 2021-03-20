using UnityEngine;
using UnityEngine.EventSystems;

public class SquadBar : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public Squad squad;
    private UnitManager manager;

    private void Start()
    {
        manager = Manager.unitManager;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

    }

    public void OnPointerExit(PointerEventData eventData)
    {

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (squad.team == Team.Self) {
            manager.SelectSquad(squad);
        }
    }
}
