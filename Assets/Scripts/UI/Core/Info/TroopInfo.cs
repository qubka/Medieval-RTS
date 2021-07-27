using UnityEngine;
using UnityEngine.EventSystems;

public class TroopInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TroopCard card;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        Manager.chartPopup.DisplayInfo(card.Troop);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Manager.chartPopup.HideInfo();
    }
}