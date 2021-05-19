using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string description;
    private Button button;
    
    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button.interactable) {
            
            StopAllCoroutines();
            StartCoroutine(Show());
        }
    }

    private IEnumerator Show()
    {
        yield return new WaitForSecondsRealtime(1f);
        Manager.fixedPopup.DisplayInfo("", description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        Manager.fixedPopup.HideInfo();
    }
}