using UnityEngine;
using UnityEngine.EventSystems;

public class TroopIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TroopCard card;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        Manager.chartPopup.DisplayInfo(card.troop);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Manager.chartPopup.HideInfo();
    }
}