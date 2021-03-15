using UnityEngine;
using UnityEngine.EventSystems;

public class SquadButton : MonoBehaviour, IPointerClickHandler
{
    public Squad squad;
    
    public void OnPointerClick(PointerEventData eventData)
    {
        squad.CanvasClick();
    }
}
