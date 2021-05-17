using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class RecruitInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RecruitLayout layout;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Manager.chartPopup.DisplayInfo(layout.troop);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Manager.chartPopup.HideInfo();
    }
}