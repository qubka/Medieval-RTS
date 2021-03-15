using UnityEngine;
using UnityEngine.EventSystems;

public class SquadButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public Squad squad;
    private UnitManager manager;

    private void Start()
    {
        manager = Manager.unitManager;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        /*if (squad.team != Team.Self) {
            if (!manager.target) {
                manager.target = squad;
            }
            squad.ChangeSelectState(true);
        }*/
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        /*if (squad.team != Team.Self) {
            if (manager.target == squad) {
                manager.target = null;
            }
            squad.ChangeSelectState(false);
        }*/
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (squad.team == Team.Self) {
            manager.SelectSquad(squad);
        }
    }
}
