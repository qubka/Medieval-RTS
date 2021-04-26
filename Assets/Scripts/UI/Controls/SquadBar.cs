using UnityEngine;
using UnityEngine.EventSystems;

public class SquadBar : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public Squad squad;

    public void OnPointerEnter(PointerEventData eventData)
    {

    }

    public void OnPointerExit(PointerEventData eventData)
    {

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (squad.team == Team.Self) {
            /*if (Input.GetKey(inclusiveKey)) { //inclusive select
			    AddSelected(squad, true);
		    } else { //exclusive selected
			    DeselectAllExcept(squad);
		    }*/
        }
    }
}
