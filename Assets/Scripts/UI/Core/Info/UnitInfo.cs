using UnityEngine;
using UnityEngine.EventSystems;

public class UnitInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private UnitLayout layout;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        Manager.chartPopup.DisplayInfo(layout.Troop);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Manager.chartPopup.HideInfo();
    }
}